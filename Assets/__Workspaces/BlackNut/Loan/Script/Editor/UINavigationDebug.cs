using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// Play-mode helpers to test gamepad navigation without a physical controller
/// (and while the editor is not focused). A virtual gamepad is added on first use.
/// </summary>
public static class UINavigationDebug
{
    private static Gamepad s_Pad;
    private static double s_PumpUntil;

    [MenuItem("Tools/UI Navigation/Debug/Pump Frames (5s)")]
    public static void Pump()
    {
        s_PumpUntil = EditorApplication.timeSinceStartup + 5.0;
        EditorApplication.update -= PumpTick;
        EditorApplication.update += PumpTick;
    }

    private static void PumpTick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > s_PumpUntil)
        {
            EditorApplication.update -= PumpTick;
            return;
        }
        EditorApplication.QueuePlayerLoopUpdate();
    }

    [MenuItem("Tools/UI Navigation/Debug/Run In Background ON")]
    public static void RunInBackgroundOn() => PlayerSettings.runInBackground = true;

    [MenuItem("Tools/UI Navigation/Debug/Run In Background OFF")]
    public static void RunInBackgroundOff() => PlayerSettings.runInBackground = false;

    [MenuItem("Tools/UI Navigation/Debug/Pad Up")] public static void Up() => Press(GamepadButton.DpadUp);
    [MenuItem("Tools/UI Navigation/Debug/Pad Down")] public static void Down() => Press(GamepadButton.DpadDown);
    [MenuItem("Tools/UI Navigation/Debug/Pad Left")] public static void Left() => Press(GamepadButton.DpadLeft);
    [MenuItem("Tools/UI Navigation/Debug/Pad Right")] public static void Right() => Press(GamepadButton.DpadRight);
    [MenuItem("Tools/UI Navigation/Debug/Pad A (Submit)")] public static void South() => Press(GamepadButton.South);
    [MenuItem("Tools/UI Navigation/Debug/Pad B (Cancel)")] public static void East() => Press(GamepadButton.East);
    [MenuItem("Tools/UI Navigation/Debug/Pad Start")] public static void Start() => Press(GamepadButton.Start);

    private static void Press(GamepadButton button)
    {
        if (!EditorApplication.isPlaying) { Debug.LogWarning("[UINavigationDebug] Enter Play mode first."); return; }
        if (s_Pad == null || !s_Pad.added) s_Pad = InputSystem.AddDevice<Gamepad>("VirtualPad");

        // Hold the button for a few player-loop frames, then release (a real press lasts ~100 ms).
        InputSystem.QueueStateEvent(s_Pad, new GamepadState().WithButton(button));
        s_ReleaseAt = EditorApplication.timeSinceStartup + 0.12;
        EditorApplication.update -= ReleaseTick;
        EditorApplication.update += ReleaseTick;
        Pump();
    }

    private static double s_ReleaseAt;

    private static void ReleaseTick()
    {
        EditorApplication.QueuePlayerLoopUpdate();
        if (EditorApplication.timeSinceStartup < s_ReleaseAt) return;
        EditorApplication.update -= ReleaseTick;
        if (s_Pad != null && s_Pad.added)
            InputSystem.QueueStateEvent(s_Pad, new GamepadState());
    }

    [MenuItem("Tools/UI Navigation/Debug/Log Selection")]
    public static void LogSelection()
    {
        var es = EventSystem.current;
        Debug.Log($"[UINavigationDebug] Selected={(es && es.currentSelectedGameObject ? es.currentSelectedGameObject.name : "none")} Top={(UINavigationPanel.Top ? UINavigationPanel.Top.name : "none")} timeScale={Time.timeScale} frame={Time.frameCount} t={Time.realtimeSinceStartup:F1}");
    }
}
