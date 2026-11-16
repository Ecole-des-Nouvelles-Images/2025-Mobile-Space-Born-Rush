using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Dumps every Selectable of the open scene (incl. inactive) with its navigation setup.
/// Output: Temp/UINavigationAudit.txt
/// </summary>
public static class UINavigationAudit
{
    [MenuItem("Tools/UI Navigation/Audit Open Scene")]
    public static void Audit()
    {
        var sb = new StringBuilder();
        var scene = SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
            Dump(root.transform, 0, sb);

        foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string tn = mb.GetType().Name;
            if (tn != "UIHandler" && tn != "UpgradeUIManager" && tn != "EventSystem" && tn != "InputSystemUIInputModule") continue;
            sb.AppendLine($"== {tn} on {mb.name}");
            var so = new SerializedObject(mb);
            var it = so.GetIterator();
            for (bool enter = true; it.NextVisible(enter); enter = false)
            {
                if (it.propertyType == SerializedPropertyType.ObjectReference)
                    sb.AppendLine($"   {it.name} = {(it.objectReferenceValue ? it.objectReferenceValue.name + " (" + AssetDatabase.GetAssetPath(it.objectReferenceValue) + ")" : "null")}");
                else if (it.propertyType == SerializedPropertyType.Boolean)
                    sb.AppendLine($"   {it.name} = {it.boolValue}");
            }
        }

        string path = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "UINavigationAudit.txt");
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"[UINavigationAudit] Written to {path}");
    }

    private static bool HasUI(Transform t) =>
        t.GetComponentsInChildren<RectTransform>(true).Length > 0;

    private static void Dump(Transform t, int depth, StringBuilder sb)
    {
        if (!HasUI(t)) return;

        var line = new StringBuilder();
        line.Append(new string(' ', depth * 2));
        if (!t.gameObject.activeSelf) line.Append("[off] ");
        line.Append(t.name).Append(" :");

        foreach (var c in t.GetComponents<Component>())
        {
            if (c == null) { line.Append(" MISSING"); continue; }
            if (c is Transform || c is CanvasRenderer) continue;
            line.Append(' ').Append(c.GetType().Name);
            if (c is Selectable s)
            {
                var n = s.navigation;
                line.Append($"[nav={n.mode} interact={s.interactable}");
                if (n.mode == Navigation.Mode.Explicit)
                    line.Append($" U={N(n.selectOnUp)} D={N(n.selectOnDown)} L={N(n.selectOnLeft)} R={N(n.selectOnRight)}");
                line.Append(']');
            }
            if (c is Button b)
            {
                line.Append(" onClick{");
                for (int i = 0; i < b.onClick.GetPersistentEventCount(); i++)
                    line.Append($"{N(b.onClick.GetPersistentTarget(i))}.{b.onClick.GetPersistentMethodName(i)} ");
                line.Append('}');
            }
            if (c is Canvas cv) line.Append($"[order={cv.sortingOrder}]");
            if (c is CanvasGroup cg) line.Append($"[interact={cg.interactable} block={cg.blocksRaycasts} a={cg.alpha}]");
        }
        if (t is RectTransform rt) line.Append($"  pos={rt.anchoredPosition}");
        sb.AppendLine(line.ToString());

        foreach (Transform child in t) Dump(child, depth + 1, sb);
    }

    private static string N(Object o) => o ? o.name : "null";
}
