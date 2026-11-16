using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Put this on every UI panel that must be usable with a gamepad.
/// When the panel is enabled it becomes the "top" panel and selects its default Selectable,
/// so the EventSystem always has something to navigate from.
/// The Cancel button (B / Escape) invokes <see cref="_onCancel"/> while this panel is on top.
/// </summary>
public class UINavigationPanel : MonoBehaviour
{
    private static readonly List<UINavigationPanel> s_Stack = new();

    /// <summary>The most recently opened panel that is still active, or null.</summary>
    public static UINavigationPanel Top
    {
        get
        {
            // Drop panels destroyed without OnDisable (scene unload edge cases).
            s_Stack.RemoveAll(p => p == null);
            return s_Stack.Count > 0 ? s_Stack[^1] : null;
        }
    }

    // "Enter Play Mode" without domain reload keeps statics: start each session clean.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => s_Stack.Clear();

    [Tooltip("Selected when the panel opens. Leave empty to use the first interactable Selectable found in children.")]
    [SerializeField] private Selectable _firstSelected;

    [Tooltip("Delay (unscaled seconds) before the first selection. Prevents an accidental Submit right after the panel appears.")]
    [SerializeField] private float _selectDelay;

    [Tooltip("Invoked when Cancel (gamepad B / Escape) or Start is pressed while this panel is on top.")]
    [SerializeField] private UnityEvent _onCancel = new();

    private GameObject _lastSelected;
    private Coroutine _selectRoutine;

    public bool HasCancel => _onCancel.GetPersistentEventCount() > 0;

    private void OnEnable()
    {
        s_Stack.Remove(this);
        s_Stack.Add(this);
        _lastSelected = null;

        if (_selectRoutine != null) StopCoroutine(_selectRoutine);
        _selectRoutine = StartCoroutine(SelectDefaultDelayed());
    }

    private void OnDisable()
    {
        bool wasTop = Top == this;
        s_Stack.Remove(this);

        if (_selectRoutine != null)
        {
            StopCoroutine(_selectRoutine);
            _selectRoutine = null;
        }

        // Give the focus back to the panel underneath (e.g. HUD → pause closed, sub-panel closed).
        var es = EventSystem.current;
        if (!wasTop || es == null) return;

        var current = es.currentSelectedGameObject;
        bool selectionWasHere = current == null || current.transform.IsChildOf(transform);
        if (!selectionWasHere) return;

        if (Top != null) Top.RestoreSelection();
        else if (current != null && !es.alreadySelecting) es.SetSelectedGameObject(null);
    }

    private IEnumerator SelectDefaultDelayed()
    {
        // Wait at least one frame so layout groups / instantiated children are in place.
        yield return null;
        if (_selectDelay > 0f) yield return new WaitForSecondsRealtime(_selectDelay);
        _selectRoutine = null;

        if (Top != this) yield break;
        if (UINavigationManager.UsingPointer)
        {
            // Mouse / touch user: don't force a highlight, but clear a stale selection from another panel.
            var es = EventSystem.current;
            if (es != null && es.currentSelectedGameObject != null && !es.currentSelectedGameObject.transform.IsChildOf(transform))
                es.SetSelectedGameObject(null);
            yield break;
        }

        SelectDefault();
    }

    /// <summary>True while the delayed first selection has not happened yet.</summary>
    public bool IsWaitingForSelection => _selectRoutine != null;

    public void SelectDefault()
    {
        var target = GetDefaultSelectable();
        Select(target ? target.gameObject : null);
    }

    public void RestoreSelection()
    {
        if (IsSelectable(_lastSelected)) Select(_lastSelected);
        else SelectDefault();
    }

    /// <summary>Called by <see cref="UINavigationManager"/> every frame to remember where the player was.</summary>
    public void TrackSelection(GameObject selected)
    {
        if (selected != null && selected.transform.IsChildOf(transform))
            _lastSelected = selected;
    }

    public void Cancel() => _onCancel.Invoke();

    private Selectable GetDefaultSelectable()
    {
        if (_firstSelected != null && IsSelectable(_firstSelected.gameObject))
            return _firstSelected;

        foreach (var s in GetComponentsInChildren<Selectable>(false))
            if (IsSelectable(s.gameObject)) return s;

        // Nothing selectable inside (e.g. an info panel): keep the configured one if it lives elsewhere and is usable.
        return _firstSelected != null && IsSelectable(_firstSelected.gameObject) ? _firstSelected : null;
    }

    public static bool IsSelectable(GameObject go) =>
        go != null && go.activeInHierarchy && go.TryGetComponent(out Selectable s) && s.IsInteractable();

    private static void Select(GameObject go)
    {
        var es = EventSystem.current;
        if (es == null || es.alreadySelecting) return;
        if (es.currentSelectedGameObject == go) return;
        es.SetSelectedGameObject(go);
    }
}
