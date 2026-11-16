using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One-click gamepad navigation setup for the BlackNut scenes (menu panels, pause, lose, upgrade choices).
/// Safe to run several times.
/// </summary>
public static class UINavigationSetup
{
    private const string ChoicePrefabPath = "Assets/__Workspaces/Jerome/Upgrades/UI/ChoicePrefab.prefab";

    [MenuItem("Tools/UI Navigation/Setup Gamepad Navigation (Open Scene)")]
    public static void Setup()
    {
        var log = new StringBuilder();
        var ui = Object.FindObjectsByType<UIHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
        if (ui == null) { Debug.LogError("[UINavigationSetup] No UIHandler in the open scene."); return; }

        // ---------- EventSystem ----------
        var es = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(e => e.GetComponent<InputSystemUIInputModule>() && e.transform.parent == null && e.name == "EventSystem");
        if (es == null) { Debug.LogError("[UINavigationSetup] No root EventSystem with InputSystemUIInputModule."); return; }

        var manager = GetOrAdd<UINavigationManager>(es.gameObject);
        var so = new SerializedObject(manager);
        so.FindProperty("_inputModule").objectReferenceValue = es.GetComponent<InputSystemUIInputModule>();
        so.ApplyModifiedPropertiesWithoutUndo();
        SetEvent(manager, "_onPauseRequested", ui.TogglePause);
        log.AppendLine("EventSystem: UINavigationManager (Start/Escape → UIHandler.TogglePause)");

        // ---------- Main menu panels ----------
        var bGame = Find<Selectable>("B_Game");
        var bCredits = Find<Selectable>("B_Credits");

        Panel(log, "P_Game", Find<Selectable>("B_Start"));
        Panel(log, "P_Settings", Find<Selectable>("S_General"), cancel: ui.OpenGamePanel);
        Panel(log, "P_Crontrol", bGame, cancel: ui.OpenGamePanel);
        Panel(log, "P_Credit", bCredits, cancel: ui.OpenGamePanel);
        Panel(log, "P_Score", null, cancel: ui.OpenGamePanel);
        Panel(log, "P_Username", Find<Selectable>("IF_Username"));

        // ---------- In-game panels ----------
        Panel(log, "P_Pause", Find<Selectable>("B_Resume"), cancel: ui.GameResume);
        Panel(log, "P_Loose", Find<Selectable>("B_ReturnMenu"));
        Panel(log, "UpgradeCanvas", null, delay: 0.35f);

        // ---------- Explicit navigation where Automatic is wrong ----------
        // Horizontal sliders only change their value with Left/Right when nothing is selectable on that side,
        // so the settings grid uses columns: Up/Down moves, Left/Right changes the volume.
        var sGeneral = Find<Selectable>("S_General");
        var sMusic = Find<Selectable>("S_Music");
        var sAmbiance = Find<Selectable>("S_Ambiance");
        var sSfx = Find<Selectable>("S_SFX");
        var bEnglish = Find<Selectable>("B_English");
        var bFrench = Find<Selectable>("B_French");

        Explicit(sGeneral, down: sAmbiance);
        Explicit(sMusic, down: sSfx);
        Explicit(sAmbiance, up: sGeneral, down: bEnglish);
        Explicit(sSfx, up: sMusic, down: bFrench);
        Explicit(bEnglish, up: sAmbiance, down: bGame, right: bFrench);
        Explicit(bFrench, up: sSfx, down: bGame, left: bEnglish);
        log.AppendLine("P_Settings: explicit column navigation (Left/Right = volume)");

        // Pause: volume slider on top, Resume, Quit.
        var pause = FindGO("P_Pause");
        var pauseSlider = pause ? pause.GetComponentsInChildren<Slider>(true).FirstOrDefault() : null;
        var bResume = Find<Selectable>("B_Resume");
        var bQuit = Find<Selectable>("B_Quit");
        Explicit(pauseSlider, down: bResume);
        Explicit(bResume, up: pauseSlider, down: bQuit);
        Explicit(bQuit, up: bResume);
        log.AppendLine("P_Pause: explicit Slider ↕ Resume ↕ Quit");

        // ---------- Upgrade choice prefab ----------
        FixChoicePrefab(log);

        // ---------- Volume sliders ----------
        FixVolumeSliders(log);

        // ---------- Localized labels ----------
        FixLocalizedTexts(log);

        // ---------- UIHandler references ----------
        var buttonParent = FindGO("ButtonParent");
        if (buttonParent)
        {
            var uiSo = new SerializedObject(ui);
            uiSo.FindProperty("_buttonParent").objectReferenceValue = buttonParent;
            uiSo.FindProperty("_usernameOfflineButton").objectReferenceValue = Find<Button>("B_Offline");
            uiSo.ApplyModifiedPropertiesWithoutUndo();
            log.AppendLine("UIHandler._buttonParent = ButtonParent");
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("[UINavigationSetup] Done:\n" + log);
    }

    /// <summary>Play mode: writes every visible Selectable and where Up/Down/Left/Right lead. Temp/UINavigationGraph.txt</summary>
    [MenuItem("Tools/UI Navigation/Dump Live Navigation Graph")]
    public static void DumpGraph()
    {
        var sb = new StringBuilder();
        var es = EventSystem.current;
        sb.AppendLine($"Selected: {(es && es.currentSelectedGameObject ? es.currentSelectedGameObject.name : "none")}  Top panel: {(UINavigationPanel.Top ? UINavigationPanel.Top.name : "none")}");
        foreach (var s in Selectable.allSelectablesArray)
        {
            if (!s.IsInteractable()) continue;
            sb.AppendLine($"{s.name,-16} U={N(s.FindSelectableOnUp())} D={N(s.FindSelectableOnDown())} L={N(s.FindSelectableOnLeft())} R={N(s.FindSelectableOnRight())}");
        }
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "UINavigationGraph.txt");
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"[UINavigationSetup] Graph written to {path}");
    }

    // ------------------------------------------------------------------

    private static void Panel(StringBuilder log, string name, Selectable first, UnityAction cancel = null, float delay = 0f)
    {
        var go = FindGO(name);
        if (go == null) { log.AppendLine($"!! {name} not found"); return; }

        var panel = GetOrAdd<UINavigationPanel>(go);
        var so = new SerializedObject(panel);
        so.FindProperty("_firstSelected").objectReferenceValue = first;
        so.FindProperty("_selectDelay").floatValue = delay;
        so.ApplyModifiedPropertiesWithoutUndo();
        SetEvent(panel, "_onCancel", cancel);

        log.AppendLine($"{name}: first={(first ? first.name : "auto")} cancel={(cancel != null ? cancel.Method.Name : "none")}");
    }

    private static void Explicit(Selectable s, Selectable up = null, Selectable down = null, Selectable left = null, Selectable right = null)
    {
        if (s == null) return;
        Undo.RecordObject(s, "UI navigation");
        s.navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = up,
            selectOnDown = down,
            selectOnLeft = left,
            selectOnRight = right,
        };
        EditorUtility.SetDirty(s);
    }

    private static void FixChoicePrefab(StringBuilder log)
    {
        var root = PrefabUtility.LoadPrefabContents(ChoicePrefabPath);
        try
        {
            foreach (var b in root.GetComponentsInChildren<Button>(true))
            {
                var nav = b.navigation;
                nav.mode = Navigation.Mode.Automatic; // was None → impossible to move between choices
                b.navigation = nav;
            }
            PrefabUtility.SaveAsPrefabAsset(root, ChoicePrefabPath);
            log.AppendLine("ChoicePrefab: Button navigation None → Automatic");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>
    /// - Removes GameObjectLocalizer from slider parts: they stored a different Fill/Handle anchor per
    ///   language, so switching EN/FR moved the bars away from the real value.
    /// - Adds VolumeSliderSync so each slider shows the saved volume.
    /// </summary>
    private static void FixVolumeSliders(StringBuilder log)
    {
        var byKey = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Slider>>();
        var scene = SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "IngameDebugConsole") continue;
            foreach (var slider in root.GetComponentsInChildren<Slider>(true))
            {
                foreach (var loc in slider.GetComponentsInChildren<UnityEngine.Localization.PropertyVariants.GameObjectLocalizer>(true))
                {
                    log.AppendLine($"{slider.name}: removed GameObjectLocalizer on {loc.name}");
                    Undo.DestroyObjectImmediate(loc);
                }

                string key = null;
                for (int i = 0; i < slider.onValueChanged.GetPersistentEventCount(); i++)
                {
                    switch (slider.onValueChanged.GetPersistentMethodName(i))
                    {
                        case "SetMasterVolume": key = "MasterVolume"; break;
                        case "SetMusicVolume": key = "MusicVolume"; break;
                        case "SetSFXVolume": key = "SFXVolume"; break;
                        case "SetAmbianceVolume": key = "AmbianceVolume"; break;
                    }
                }
                if (key == null) continue;

                var sync = GetOrAdd<VolumeSliderSync>(slider.gameObject);
                var so = new SerializedObject(sync);
                so.FindProperty("_prefsKey").stringValue = key;
                so.ApplyModifiedPropertiesWithoutUndo();
                log.AppendLine($"{slider.name}: VolumeSliderSync({key})");
                byKey.TryAdd(key, new System.Collections.Generic.List<Slider>());
                byKey[key].Add(slider);
            }
        }

        // Twin sliders must share the same range, otherwise the same dB value sits at a different
        // position on each bar (pause master was -80..0, settings master -70..0).
        foreach (var kv in byKey)
        {
            var reference = kv.Value.FirstOrDefault(s => s.name.StartsWith("S_")) ?? kv.Value[0];
            foreach (var s in kv.Value)
            {
                if (s == reference) continue;
                if (Mathf.Approximately(s.minValue, reference.minValue) && Mathf.Approximately(s.maxValue, reference.maxValue)) continue;
                Undo.RecordObject(s, "Volume slider range");
                s.minValue = reference.minValue;
                s.maxValue = reference.maxValue;
                s.wholeNumbers = reference.wholeNumbers;
                EditorUtility.SetDirty(s);
                log.AppendLine($"{s.name}: range aligned on {reference.name} ({reference.minValue}..{reference.maxValue})");
            }
        }
    }

    /// <summary>
    /// A LocalizeStringEvent wired to TextMeshProUGUI.BroadcastMessage (instead of set_text) never
    /// updates its label: it stays "New Text" and logs "BroadcastMessage ... has no receiver".
    /// </summary>
    private static void FixLocalizedTexts(StringBuilder log)
    {
        var events = Object.FindObjectsByType<UnityEngine.Localization.Components.LocalizeStringEvent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var lse in events)
        {
            var evt = lse.OnUpdateString;
            for (int i = evt.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                if (evt.GetPersistentMethodName(i) != "BroadcastMessage") continue;
                if (evt.GetPersistentTarget(i) is not TMPro.TMP_Text text) continue;

                Undo.RecordObject(lse, "Fix localized text");
                UnityEventTools.RemovePersistentListener(evt, i);
                var setText = (UnityAction<string>)System.Delegate.CreateDelegate(typeof(UnityAction<string>), text, "set_text");
                UnityEventTools.AddPersistentListener(evt, setText);
                EditorUtility.SetDirty(lse);
                log.AppendLine($"{text.transform.parent?.name}/{text.name}: localized text BroadcastMessage → set_text");
            }
        }
    }

    private static void SetEvent(Object owner, string field, UnityAction action)
    {
        var fi = owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
        var evt = (UnityEvent)fi.GetValue(owner);
        for (int i = evt.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(evt, i);
        if (action != null)
            UnityEventTools.AddVoidPersistentListener(evt, action);
        EditorUtility.SetDirty(owner);
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component =>
        go.GetComponent<T>() ? go.GetComponent<T>() : Undo.AddComponent<T>(go);

    private static GameObject FindGO(string name)
    {
        var scene = SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "IngameDebugConsole") continue;
            var t = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == name);
            if (t) return t.gameObject;
        }
        return null;
    }

    private static T Find<T>(string name) where T : Component
    {
        var go = FindGO(name);
        return go ? go.GetComponent<T>() : null;
    }

    private static string N(Object o) => o ? o.name : "-";
}
