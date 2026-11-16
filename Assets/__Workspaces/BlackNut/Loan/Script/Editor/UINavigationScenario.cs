using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Play-mode scenario: drives the virtual gamepad through every menu / in-game panel
/// and checks which object is selected after each step. Report: Temp/UINavigationScenario.txt
/// </summary>
public static class UINavigationScenario
{
    private struct Step
    {
        public string Label;
        public Action Do;
        public string ExpectSelected; // null = don't check, "" = expect nothing selected
        public string ExpectTop;      // null = don't check
        public Func<string> Check;    // custom check: returns null if OK, else the error
    }

    private static readonly Queue<Step> s_Steps = new();
    private static readonly StringBuilder s_Report = new();
    private static double s_Next;
    private static Step? s_Pending;
    private static int s_Fails;

    private const double StepDelay = 0.6;

    [MenuItem("Tools/UI Navigation/Debug/Run Menu Scenario")]
    public static void RunMenu()
    {
        Begin();
        S("pad wakes selection", UINavigationDebug.Down, "B_Start", "P_Game");
        S("down", UINavigationDebug.Down, "B_Command", "P_Game");
        S("down → bottom bar", UINavigationDebug.Down, "B_Game", "P_Game");
        S("left", UINavigationDebug.Left, "B_Settings", "P_Game");
        S("A → settings", UINavigationDebug.South, "S_General", "P_Settings");
        S("right = volume (stay)", UINavigationDebug.Right, "S_General", "P_Settings");
        S("down", UINavigationDebug.Down, "S_Ambiance", "P_Settings");
        S("down", UINavigationDebug.Down, "B_English", "P_Settings");
        S("right", UINavigationDebug.Right, "B_French", "P_Settings");
        S("up", UINavigationDebug.Up, "S_SFX", "P_Settings");
        S("up", UINavigationDebug.Up, "S_Music", "P_Settings");
        S("B → back", UINavigationDebug.East, "B_Start", "P_Game");
        S("down", UINavigationDebug.Down, "B_Command", "P_Game");
        S("A → controls", UINavigationDebug.South, "B_Game", "P_Crontrol");
        S("B → back", UINavigationDebug.East, "B_Start", "P_Game");
        S("down", UINavigationDebug.Down, "B_Command", "P_Game");
        S("down", UINavigationDebug.Down, "B_Game", "P_Game");
        S("right", UINavigationDebug.Right, "B_Credits", "P_Game");
        S("A → credits", UINavigationDebug.South, "B_Credits", "P_Credit");
        S("left (home)", UINavigationDebug.Left, "B_Game", "P_Credit");
        S("A home → game panel", UINavigationDebug.South, "B_Start", "P_Game");
        Run();
    }

    [MenuItem("Tools/UI Navigation/Debug/Run InGame Scenario")]
    public static void RunInGame()
    {
        Begin();
        S("start game", () => Ui()?.GameStart(), null, null);
        S("wait countdown", null, null, null, 4.0);
        S("Start → pause", UINavigationDebug.Start, "B_Resume", "P_Pause");
        S("down", UINavigationDebug.Down, "B_Quit", "P_Pause");
        S("up", UINavigationDebug.Up, "B_Resume", "P_Pause");
        S("up", UINavigationDebug.Up, "Slider", "P_Pause");
        S("B → resume", UINavigationDebug.East, "", "");
        S("wait countdown", null, null, null, 4.0);
        S("level up", LevelUp, null, "UpgradeCanvas");
        S("wait select delay", null, null, null, 0.8);
        S("check first choice", null, "*", "UpgradeCanvas");
        S("right", UINavigationDebug.Right, "*", "UpgradeCanvas");
        S("A → pick upgrade", UINavigationDebug.South, null, "");
        Run();
    }

    [MenuItem("Tools/UI Navigation/Debug/Run Bugfix Scenario")]
    public static void RunBugfix()
    {
        Begin();
        var savedPrefs = new Dictionary<string, float?>();
        foreach (var k in new[] { "MasterVolume", "MusicVolume" })
            savedPrefs[k] = PlayerPrefs.HasKey(k) ? PlayerPrefs.GetFloat(k) : (float?)null;
        var locales = UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.Locales;
        int savedLocale = Mathf.Max(0, locales.IndexOf(UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale));

        // Volume sliders vs language change
        C("open settings", () => Ui()?.OpenOptionPanel(), () => Active("P_Settings") ? null : "settings hidden");
        C("set master to -35", () => SliderNamed("S_General").value = -35f, () => null);
        C("music to -20", () => SliderNamed("S_Music").value = -20f, () => null);
        C("switch to French", () => Ui()?.ChangeLangue(1), () => CheckSliders(-35f, -20f), 1.5);
        C("switch to English", () => Ui()?.ChangeLangue(0), () => CheckSliders(-35f, -20f), 1.5);
        C("prefs saved", null, () => Mathf.Approximately(PlayerPrefs.GetFloat("MasterVolume"), -35f) ? null : "MasterVolume not saved");

        // End of game → main menu
        C("start game", () => Ui()?.GameStart(), () => null, 4.0);
        C("player dies", () => EventBus.OnPlayerDead?.Invoke(), () => Active("P_Loose") ? null : "P_Loose not shown", 1.0);
        S("lose screen top panel", null, null, "P_Loose");
        C("A → main menu", () => Go("B_ReturnMenu").GetComponent<UnityEngine.UI.Button>().onClick.Invoke(),
            () => !Active("MainMenu") ? "MainMenu hidden" :
                  Active("P_Username") ? "P_Username visible" :
                  !Active("P_Game") ? "P_Game hidden" : null, 1.0);
        S("menu top panel", null, null, "P_Game");
        C("pause slider synced", () => Ui()?.GameStart(), () => null, 4.0);
        C("open pause", () => Ui()?.GamePause(),
            () => { var s = SliderInChildren("P_Pause"); return s && Mathf.Approximately(s.value, -35f) ? null : $"pause slider={s?.value}"; });
        C("resume", () => Ui()?.GameResume(), () => null, 4.0);

        // Upgrade cards scale from their centre
        C("level up", LevelUp, () =>
        {
            var choice = UnityEngine.Object.FindAnyObjectByType<UpgradeChoiceUI>();
            if (!choice) return "no choice";
            var p = ((RectTransform)choice.transform).pivot;
            return p == new Vector2(0.5f, 0.5f) ? null : $"pivot={p}";
        }, 1.0);
        C("pick", SubmitSelected, () => Active("UpgradeCanvas") ? "upgrade panel still open" : null);

        // Put the player's own volume / language settings back.
        C("restore settings", () =>
        {
            foreach (var kv in savedPrefs)
                if (kv.Value.HasValue) PlayerPrefs.SetFloat(kv.Key, kv.Value.Value);
                else PlayerPrefs.DeleteKey(kv.Key);
            Ui()?.ChangeLangue(savedLocale);
        }, () => null);
        Run();
    }

    [MenuItem("Tools/UI Navigation/Debug/Run Audit Scenario")]
    public static void RunAudit()
    {
        Begin();
        float? savedMaster = PlayerPrefs.HasKey("MasterVolume") ? PlayerPrefs.GetFloat("MasterVolume") : null;
        int players = 0;

        // Twin master sliders: settings ↔ pause, live, same position.
        C("open settings", () => Ui()?.OpenOptionPanel(), () => Active("P_Settings") ? null : "settings hidden");
        C("settings master -42", () => SliderNamed("S_General").value = -42f, () => null);
        C("double Start click", () => { Ui()?.GameStart(); Ui()?.GameStart(); }, () => null, 4.0);
        C("one player only", () => players = CountPlayers(), () => players == 1 ? null : $"{players} players");
        C("open pause", () => Ui()?.GamePause(), () =>
        {
            var p = SliderInChildren("P_Pause");
            var g = SliderNamed("S_General");
            if (!Mathf.Approximately(p.value, -42f)) return $"pause master={p.value}";
            return Mathf.Approximately(p.minValue, g.minValue) && Mathf.Approximately(p.maxValue, g.maxValue)
                ? null : $"ranges differ pause={p.minValue}..{p.maxValue} settings={g.minValue}..{g.maxValue}";
        });
        C("pause master -10", () => SliderInChildren("P_Pause").value = -10f,
            () => Mathf.Approximately(SliderNamed("S_General").value, -10f) ? null : $"settings master={SliderNamed("S_General").value}");
        C("double resume", () => { Ui()?.GameResume(); Ui()?.GameResume(); }, () => Active("P_Pause") ? "still paused" : null);
        C("pause during countdown", () => Ui()?.GamePause(), () => Active("P_Countdown") ? "countdown still running" : null, 4.0);
        C("countdown can't resume", null, () => Active("P_Pause") ? null : "pause closed by countdown");
        C("resume", () => Ui()?.GameResume(), () => null, 4.0);

        // Two level-ups at once: second one waits, both are shown.
        C("double level up", () => { LevelUp(); LevelUp(); }, () => ChoiceCount() > 0 ? null : "no choices", 1.0);
        C("double pick", () => { SubmitFirstChoice(); SubmitFirstChoice(); },
            () => Active("UpgradeCanvas") && Mathf.Approximately(Time.timeScale, 0f) ? null : "second level-up lost", 1.0);
        C("pick second", SubmitFirstChoice,
            () => !Active("UpgradeCanvas") && Mathf.Approximately(Time.timeScale, 1f) ? null : $"canvas={Active("UpgradeCanvas")} timeScale={Time.timeScale}", 1.0);

        // Death then double "return to menu".
        C("player dies", () => EventBus.OnPlayerDead?.Invoke(), () => Active("P_Loose") ? null : "no lose screen", 1.0);
        C("double return", () => { var b = Go("B_ReturnMenu").GetComponent<UnityEngine.UI.Button>(); b.onClick.Invoke(); b.onClick.Invoke(); },
            () => Active("P_Game") && !Active("P_Username") && !Active("Game") ? null : "menu state wrong", 1.0);
        C("restore", () =>
        {
            if (savedMaster.HasValue) PlayerPrefs.SetFloat("MasterVolume", savedMaster.Value); else PlayerPrefs.DeleteKey("MasterVolume");
        }, () => null);
        Run();
    }

    private static int CountPlayers() => UnityEngine.Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length;
    private static int ChoiceCount() => UnityEngine.Object.FindObjectsByType<UpgradeChoiceUI>(FindObjectsSortMode.None).Length;

    private static void SubmitFirstChoice()
    {
        foreach (var c in UnityEngine.Object.FindObjectsByType<UpgradeChoiceUI>(FindObjectsSortMode.InstanceID))
        {
            var b = c.GetComponentInChildren<UnityEngine.UI.Button>();
            if (b && b.isActiveAndEnabled) { b.onClick.Invoke(); return; }
        }
    }

    // ================================================================== Gameplay VFX

    [MenuItem("Tools/UI Navigation/Debug/Run Gameplay VFX Scenario")]
    public static void RunGameplayVfx()
    {
        Begin();
        var killed = new System.Collections.Generic.List<EnemyHandler>();

        C("start game", () => Ui()?.GameStart(), () => Player() ? null : "no player", 4.5);
        C("grounded", null, () => Player().IsGrounded ? null : "player not grounded");
        C("slide", () => Player().TrySlide(), () => Emitting("VFX_SlideDust") ? null : "no slide dust", 0.35);
        C("screenshot slide", () => Shot("vfx_slide"), () => null, 0.3);
        C("slide ended", null, () => !Player().IsSliding && !Emitting("VFX_SlideDust") ? null : $"sliding={Player().IsSliding} dust={Emitting("VFX_SlideDust")}", 1.5);
        C("jump", () => Player().TryJump(), () => Alive("VFX_JumpImpulse") ? null : "no jump impulse", 0.12);
        C("screenshot jump", () => Shot("vfx_jump"), () => null, 1.5);
        C("air slide = no dust", () => { Player().TryJump(); }, () => null, 0.15);
        C("slide in air", () => Player().TrySlide(), () => Emitting("VFX_SlideDust") && !Player().IsGrounded ? "dust in the air" : null, 1.2);

        C("kill enemies", () => killed.AddRange(KillVisibleEnemies()), () => killed.Count > 0 ? null : "no enemy to kill", 0.5);
        C("blood pools", null, () => UnityEngine.Object.FindObjectsByType<BloodDecal>(FindObjectsSortMode.None).Length >= killed.Count ? null : $"pools={UnityEngine.Object.FindObjectsByType<BloodDecal>(FindObjectsSortMode.None).Length} kills={killed.Count}");
        C("screenshot death", () => Shot("vfx_death"), () => null, 2.0);
        C("ragdolls above floor", null, () => RagdollsAboveFloor(killed), 2.0);
        C("ragdolls still above", null, () => RagdollsAboveFloor(killed));
        Run();
    }

    [MenuItem("Tools/UI Navigation/Debug/Run Ragdoll Scenario")]
    public static void RunRagdoll()
    {
        Begin();
        var killed = new System.Collections.Generic.List<EnemyHandler>();
        C("start game", () => Ui()?.GameStart(), () => Player() ? null : "no player", 10.0);
        C("kill enemies", () => killed.AddRange(KillVisibleEnemies()),
            () => killed.Count > 0 ? null : $"no enemy to kill (alive enemies: {UnityEngine.Object.FindObjectsByType<EnemyHandler>(FindObjectsSortMode.None).Length})", 0.25);
        C("pools right after", null, () =>
        {
            int pools = UnityEngine.Object.FindObjectsByType<BloodDecal>(FindObjectsSortMode.None).Length;
            int safety = 0;
            foreach (var e in killed) if (e && e.transform.Find("RagdollSafetyFloor")) safety++;
            return pools >= killed.Count && safety == killed.Count ? null : $"kills={killed.Count} pools={pools} safetyFloors={safety}";
        }, 0.0);
        C("t=0.25s", null, () => RagdollsAboveFloor(killed), 0.25);
        C("t=0.5s", null, () => RagdollsAboveFloor(killed), 0.5);
        C("t=1s", null, () => RagdollsAboveFloor(killed), 1.0);
        C("t=2s", null, () => RagdollsAboveFloor(killed), 1.0);
        C("t=3s", null, () => RagdollsAboveFloor(killed));
        C("kills / pools", null, () =>
        {
            int pools = UnityEngine.Object.FindObjectsByType<BloodDecal>(FindObjectsSortMode.None).Length;
            return pools >= killed.Count ? null : $"kills={killed.Count} pools={pools}";
        });
        Run();
    }

    [MenuItem("Tools/UI Navigation/Debug/Run Jump Slide Scenario")]
    public static void RunJumpSlide()
    {
        Begin();
        int maxSlideSmoke = 0, maxJumpSmoke = 0;
        C("start game", () => Ui()?.GameStart(), () => Player() ? null : "no player", 4.5);

        // Jump while changing lane: the ring must stay under the player.
        C("lane change + jump", () => { Player().TryMoveLane(1); Player().TryJump(); }, () => RingOffset(), 0.1);
        C("ring centred t=0.2", () => maxJumpSmoke = Mathf.Max(maxJumpSmoke, Count("VFX_JumpImpulse/DustBurst")), () => RingOffset(), 0.1);
        C("ring centred t=0.3", () => maxJumpSmoke = Mathf.Max(maxJumpSmoke, Count("VFX_JumpImpulse/DustBurst")), () => RingOffset(), 0.1);
        C("screenshot jump", () => Shot("vfx_jump"), () => null, 0.05);
        C("jump smoke 6-10", null, () => maxJumpSmoke is >= 6 and <= 10 ? null : $"jump smoke={maxJumpSmoke}", 1.5);

        // Slide: count smoke puffs over the whole slide.
        C("slide", () => Player().TrySlide(), () => null, 0.2);
        for (int i = 0; i < 4; i++)
            C($"slide sample {i}", () => maxSlideSmoke = Mathf.Max(maxSlideSmoke, Count("VFX_SlideDust/Puffs")), () => null, 0.15);
        C("screenshot slide", () => Shot("vfx_slide"), () => null, 0.05);
        C("slide smoke <= 10", null, () => maxSlideSmoke is >= 1 and <= 10 ? null : $"slide smoke max={maxSlideSmoke}");
        Run();
    }

    private static int Count(string path)
    {
        var player = Player();
        var t = player ? player.transform.Find(path) : null;
        var ps = t ? t.GetComponent<ParticleSystem>() : null;
        return ps ? ps.particleCount : -1;
    }

    private static string RingOffset()
    {
        var player = Player();
        var ring = player ? player.transform.Find("VFX_JumpImpulse") : null;
        if (!ring) return "no jump VFX";
        float dx = Mathf.Abs(ring.position.x - player.transform.position.x);
        float dy = ring.position.y - player.transform.position.y;
        return dx < 0.05f ? null : $"ring off-centre by {dx:F2}m (dy={dy:F2})";
    }

    private static PlayerController Player() => UnityEngine.Object.FindAnyObjectByType<PlayerController>();

    private static bool Emitting(string name)
    {
        var go = Go(name);
        if (!go) return false;
        foreach (var ps in go.GetComponentsInChildren<ParticleSystem>())
            if (ps.isEmitting && ps.emission.enabled) return true; // the root is an empty driver
        return false;
    }

    private static bool Alive(string name)
    {
        var go = Go(name);
        var ps = go ? go.GetComponent<ParticleSystem>() : null;
        return ps && ps.IsAlive(true);
    }

    private static void Shot(string file)
    {
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine("Temp", file + ".png"));
    }

    private static System.Collections.Generic.List<EnemyHandler> KillVisibleEnemies()
    {
        var list = new System.Collections.Generic.List<EnemyHandler>();
        foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHandler>(FindObjectsSortMode.None))
        {
            if (e.IsDead) continue;
            float z = e.transform.position.z;
            if (z < 3f) continue; // in front of the player
            e.CurrentHealth = 0f;
            list.Add(e);
        }
        return list;
    }

    private static string RagdollsAboveFloor(System.Collections.Generic.List<EnemyHandler> enemies)
    {
        int mask = LayerMask.GetMask("Modul");
        var sb = new StringBuilder();
        foreach (var e in enemies)
        {
            if (!e) continue;
            foreach (var col in e.GetComponentsInChildren<Collider>())
            {
                if (!col.attachedRigidbody || col.attachedRigidbody.gameObject == e.gameObject) continue; // bones only
                Vector3 c = col.bounds.center;
                if (!Physics.Raycast(new Vector3(c.x, c.y + 3f, c.z), Vector3.down, out RaycastHit hit, 10f, mask)) continue;
                float depth = hit.point.y - col.bounds.min.y;
                if (depth > 0.08f) sb.Append($"{e.name}/{col.name} {depth:F2}m under; ");
            }
        }
        return sb.Length == 0 ? null : sb.ToString();
    }

    private static string CheckSliders(float master, float music)
    {
        var g = SliderNamed("S_General");
        var m = SliderNamed("S_Music");
        if (!Mathf.Approximately(g.value, master) || !Mathf.Approximately(m.value, music))
            return $"values changed: general={g.value} music={m.value}";

        // The fill must match the value (GameObjectLocalizer used to overwrite its anchors per language).
        float expected = Mathf.InverseLerp(g.minValue, g.maxValue, g.value);
        float anchor = g.fillRect.anchorMax.x;
        return Mathf.Abs(anchor - expected) < 0.01f ? null : $"fill anchor={anchor:F2} expected={expected:F2}";
    }

    // ------------------------------------------------------------------

    private static void C(string label, Action action, Func<string> check, double wait = StepDelay)
    {
        s_Steps.Enqueue(new Step { Label = label, Do = action, Check = check });
        s_Waits[s_Steps.Count - 1] = wait;
    }

    private static GameObject Go(string name)
    {
        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
            if (t.name == name && t.gameObject.scene.IsValid()) return t.gameObject;
        return null;
    }

    /// <summary>Same as the gamepad A button on the current selection, without depending on frame timing.</summary>
    private static void SubmitSelected()
    {
        var es = EventSystem.current;
        var go = es ? es.currentSelectedGameObject : null;
        if (go) ExecuteEvents.Execute(go, new BaseEventData(es), ExecuteEvents.submitHandler);
    }

    private static bool Active(string name) { var g = Go(name); return g && g.activeInHierarchy; }
    private static UnityEngine.UI.Slider SliderNamed(string name) => Go(name)?.GetComponent<UnityEngine.UI.Slider>();
    private static UnityEngine.UI.Slider SliderInChildren(string name) => Go(name)?.GetComponentInChildren<UnityEngine.UI.Slider>(true);

    private static UIHandler Ui() => UnityEngine.Object.FindAnyObjectByType<UIHandler>();

    private static void LevelUp()
    {
        var gen = UnityEngine.Object.FindAnyObjectByType<UpgradeGenerator>();
        if (gen == null) { s_Report.AppendLine("  !! no UpgradeGenerator"); return; }
        EventBus.OnPlayerLevelUp?.Invoke(gen.GenerateUpgradeChoices());
    }

    private static void Begin()
    {
        s_Steps.Clear();
        s_Waits.Clear();
        s_Report.Clear();
        s_Fails = 0;
        s_Pending = null;
        PlayerSettings.runInBackground = true;
    }

    private static void S(string label, Action action, string expectSelected, string expectTop, double wait = StepDelay)
    {
        s_Steps.Enqueue(new Step { Label = label, Do = action, ExpectSelected = expectSelected, ExpectTop = expectTop });
        s_Waits[s_Steps.Count - 1] = wait; // delay before this step is checked
    }

    private static readonly Dictionary<int, double> s_Waits = new();
    private static int s_Index;

    private static void Run()
    {
        if (!EditorApplication.isPlaying) { Debug.LogWarning("[UINavigationScenario] Enter Play mode first."); return; }
        s_Index = 0;
        s_Next = EditorApplication.timeSinceStartup + 0.2;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        EditorApplication.QueuePlayerLoopUpdate();
        if (EditorApplication.timeSinceStartup < s_Next) return;

        if (s_Pending.HasValue)
        {
            Check(s_Pending.Value);
            s_Pending = null;
        }

        if (!EditorApplication.isPlaying || s_Steps.Count == 0)
        {
            EditorApplication.update -= Tick;
            s_Waits.Clear();
            s_Report.AppendLine(s_Fails == 0 ? "RESULT: ALL OK" : $"RESULT: {s_Fails} FAIL(S)");
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "UINavigationScenario.txt");
            File.WriteAllText(path, s_Report.ToString());
            Debug.Log($"[UINavigationScenario] Finished ({s_Fails} fail). Report: {path}");
            return;
        }

        var step = s_Steps.Dequeue();
        try { step.Do?.Invoke(); }
        catch (Exception e) { s_Report.AppendLine($"  !! {step.Label}: {e.Message}"); }

        s_Pending = step;
        s_Next = EditorApplication.timeSinceStartup + (s_Waits.TryGetValue(s_Index, out var w) ? w : StepDelay);
        s_Index++;
    }

    private static void Check(Step step)
    {
        var es = EventSystem.current;
        var sel = es && es.currentSelectedGameObject && es.currentSelectedGameObject.activeInHierarchy ? es.currentSelectedGameObject.name : "";
        var top = UINavigationPanel.Top ? UINavigationPanel.Top.name : "";

        if (step.Check != null)
        {
            string err;
            try { err = step.Check(); } catch (Exception e) { err = e.Message; }
            if (err != null) s_Fails++;
            s_Report.AppendLine($"{(err == null ? "OK  " : "FAIL")} {step.Label,-26} {err}");
            return;
        }

        bool okSel = step.ExpectSelected == null || (step.ExpectSelected == "*" ? sel != "" : sel == step.ExpectSelected);
        bool okTop = step.ExpectTop == null || top == step.ExpectTop;
        if (!okSel || !okTop) s_Fails++;

        s_Report.AppendLine($"{(okSel && okTop ? "OK  " : "FAIL")} {step.Label,-26} selected='{sel}' top='{top}'" +
                            (okSel ? "" : $"  expected selected='{step.ExpectSelected}'") +
                            (okTop ? "" : $"  expected top='{step.ExpectTop}'"));
    }
}
