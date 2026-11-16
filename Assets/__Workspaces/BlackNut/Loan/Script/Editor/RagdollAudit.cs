using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>Dumps rigidbodies / colliders / layers of the enemy prefab and module floors. Temp/RagdollAudit.txt</summary>
public static class RagdollAudit
{
    [MenuItem("Tools/UI Navigation/Debug/Audit Ragdoll")]
    public static void Audit()
    {
        var sb = new StringBuilder();
        Dump(sb, "Assets/__Workspaces/Loan/Prefabs/Enemies/EnemiesRange_Prefabs.prefab", true);
        Dump(sb, "Assets/__Workspaces/Marion/Prefab/M_ModuleCorridor_01.prefab", false);

        sb.AppendLine("##### Layer collision matrix (layers in use)");
        for (int a = 0; a < 32; a++)
        {
            string na = LayerMask.LayerToName(a);
            if (string.IsNullOrEmpty(na)) continue;
            var ignored = new StringBuilder();
            for (int b = 0; b < 32; b++)
            {
                string nb = LayerMask.LayerToName(b);
                if (!string.IsNullOrEmpty(nb) && Physics.GetIgnoreLayerCollision(a, b)) ignored.Append(nb).Append(", ");
            }
            sb.AppendLine($"  {a}:{na} ignores [{ignored}]");
        }
        sb.AppendLine($"Physics: defaultSolverIterations={Physics.defaultSolverIterations} velocityIterations={Physics.defaultSolverVelocityIterations} fixedDelta={Time.fixedDeltaTime} contactOffset={Physics.defaultContactOffset}");

        string outPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "RagdollAudit.txt");
        File.WriteAllText(outPath, sb.ToString());
        Debug.Log($"[RagdollAudit] {outPath}");
    }

    private static void Dump(StringBuilder sb, string path, bool bodies)
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        sb.AppendLine($"##### {path}");
        if (!root) return;
        sb.AppendLine($"  root layer={LayerMask.LayerToName(root.layer)} tag={root.tag} scale={root.transform.lossyScale}");
        foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true))
            sb.AppendLine($"  RB {rb.name} layer={LayerMask.LayerToName(rb.gameObject.layer)} kin={rb.isKinematic} grav={rb.useGravity} mass={rb.mass} drag={rb.linearDamping} cd={rb.collisionDetectionMode} interp={rb.interpolation} constraints={rb.constraints} detectCol={rb.detectCollisions}");
        foreach (var c in root.GetComponentsInChildren<Collider>(true))
        {
            if (!bodies && c.attachedRigidbody) continue;
            string extra = c switch
            {
                BoxCollider b => $"size={b.size.ToString("F4")} center={b.center.ToString("F4")} worldSize={Vector3.Scale(b.size, b.transform.lossyScale).ToString("F3")} rot={b.transform.rotation.eulerAngles} pos={b.transform.position.ToString("F2")}",
                CapsuleCollider cc => $"r={cc.radius} h={cc.height} dir={cc.direction}",
                SphereCollider s => $"r={s.radius}",
                MeshCollider m => $"mesh={(m.sharedMesh ? m.sharedMesh.name : "null")} convex={m.convex}",
                _ => ""
            };
            sb.AppendLine($"  COL {c.GetType().Name} on {c.name} layer={LayerMask.LayerToName(c.gameObject.layer)} tag={c.tag} trigger={c.isTrigger} enabled={c.enabled} {extra} worldY={c.bounds.min.y:F2}..{c.bounds.max.y:F2}");
        }
    }
}
