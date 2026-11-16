using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>Dumps the particle systems of a few prefabs (timings, look, materials). Temp/VFXAudit.txt</summary>
public static class VFXAudit
{
    private static readonly string[] Prefabs =
    {
        "Assets/__Workspaces/Marion/VFX/VFX_Armes/VFX_AlienArme_Implosion.prefab",
        "Assets/__Workspaces/Marion/VFX/VFX_Box/VFX_BoxCollision.prefab",
        "Assets/__Workspaces/Marion/VFX/Composant/VFX_PoussiereCouloir.prefab",
        "Assets/__Workspaces/Marion/VFX/Composant/VFX_GroundFog.prefab",
        "Assets/__Workspaces/Marion/VFX/VFX_Player/VFX_Stunt.prefab",
        "Assets/__Workspaces/Marion/VFX/VFX_Player/VFX_GainXP.prefab",
        "Assets/__Workspaces/Loan/Prefabs/Enemies/EnemiesRange_Prefabs.prefab",
        "Assets/__Workspaces/Jerome/Player/Player.prefab",
    };

    [MenuItem("Tools/UI Navigation/Debug/Audit VFX Prefabs")]
    public static void Audit()
    {
        var sb = new StringBuilder();
        foreach (var path in Prefabs)
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            sb.AppendLine($"##### {path} {(root ? "" : "(missing)")}");
            if (!root) continue;

            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                var m = ps.main;
                var r = ps.GetComponent<ParticleSystemRenderer>();
                var e = ps.emission;
                var s = ps.shape;
                sb.AppendLine($"  [{Path(ps.transform, root.transform)}] dur={m.duration} loop={m.loop} delay={Curve(m.startDelay)} life={Curve(m.startLifetime)} speed={Curve(m.startSpeed)} size={Curve(m.startSize)} simSpeed={m.simulationSpeed} space={m.simulationSpace} playOnAwake={m.playOnAwake} gravity={Curve(m.gravityModifier)}");
                sb.AppendLine($"      color={Grad(m.startColor)} rate={Curve(e.rateOverTime)} rateDist={Curve(e.rateOverDistance)} bursts={e.burstCount} shape={(s.enabled ? s.shapeType + " r=" + s.radius + " angle=" + s.angle : "off")} max={m.maxParticles}");
                sb.AppendLine($"      renderer={(r ? r.renderMode + " mat=" + (r.sharedMaterial ? r.sharedMaterial.name + " (" + r.sharedMaterial.shader.name + ") " + AssetDatabase.GetAssetPath(r.sharedMaterial) : "null") : "none")} col={ps.colorOverLifetime.enabled} sol={ps.sizeOverLifetime.enabled} vol={ps.velocityOverLifetime.enabled} noise={ps.noise.enabled}");
            }

            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue;
                string tn = mb.GetType().Name;
                if (tn != "WeaponHandler" && tn != "EnemyRange" && tn != "PlayerController") continue;
                sb.AppendLine($"  == {tn} on {mb.name}");
                var so = new SerializedObject(mb);
                var it = so.GetIterator();
                for (bool enter = true; it.NextVisible(enter); enter = false)
                    if (it.propertyType == SerializedPropertyType.ObjectReference)
                        sb.AppendLine($"     {it.name} = {(it.objectReferenceValue ? it.objectReferenceValue.name : "null")}");
                    else if (it.propertyType == SerializedPropertyType.Float || it.propertyType == SerializedPropertyType.Vector3)
                        sb.AppendLine($"     {it.name} = {(it.propertyType == SerializedPropertyType.Float ? it.floatValue.ToString() : it.vector3Value.ToString())}");
            }
        }

        string outPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Temp", "VFXAudit.txt");
        File.WriteAllText(outPath, sb.ToString());
        Debug.Log($"[VFXAudit] {outPath}");
    }

    private static string Path(Transform t, Transform root) => t == root ? t.name : Path(t.parent, root) + "/" + t.name;

    private static string Curve(ParticleSystem.MinMaxCurve c) => c.mode switch
    {
        ParticleSystemCurveMode.Constant => c.constant.ToString("0.###"),
        ParticleSystemCurveMode.TwoConstants => $"{c.constantMin:0.###}-{c.constantMax:0.###}",
        _ => $"curve*{c.curveMultiplier:0.###}",
    };

    private static string Grad(ParticleSystem.MinMaxGradient g) => g.mode switch
    {
        ParticleSystemGradientMode.Color => "#" + ColorUtility.ToHtmlStringRGBA(g.color),
        ParticleSystemGradientMode.TwoColors => "#" + ColorUtility.ToHtmlStringRGBA(g.colorMin) + "-#" + ColorUtility.ToHtmlStringRGBA(g.colorMax),
        _ => g.mode.ToString(),
    };
}
