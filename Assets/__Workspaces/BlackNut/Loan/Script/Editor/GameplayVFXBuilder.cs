using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Builds the gameplay VFX (slide dust, jump impulse, alien blood pool), the enemy charge telegraph,
/// and wires everything on the player / enemy prefabs. Safe to run again (assets are overwritten).
/// Look: reuses the project's VFX sources (Unity Particle Pack smoke used by Marion's corridor dust,
/// URP particle materials), cyan for the player like its plasma shots, amber/orange for the aliens.
/// </summary>
public static class GameplayVFXBuilder
{
    private const string Root = "Assets/__Workspaces/BlackNut/Loan/VFX";
    private const string TexDir = Root + "/Textures";
    private const string MatDir = Root + "/Materials";
    private const string PrefabDir = Root + "/Prefabs";

    private const string PlayerPrefab = "Assets/__Workspaces/Jerome/Player/Player.prefab";
    private const string EnemyRangePrefab = "Assets/__Workspaces/Loan/Prefabs/Enemies/EnemiesRange_Prefabs.prefab";
    private const string EnemyCacPrefab = "Assets/__Workspaces/Loan/Prefabs/Enemies/EnemiesCac_Prefabs.prefab";

    // Marion's bullet material: URP Particles/Unlit, transparent alpha blend – proven to render in game.
    private const string AlphaParticleSource = "Assets/__Workspaces/Marion/VFX/VFX_Armes/04.mat";
    private const string SmokePuffTex = "Assets/UnityTechnologies/ParticlePack/EffectExamples/Smoke & Steam Effects/Textures/SmokePuff.png";
    private const string DustMoteTex = "Assets/UnityTechnologies/ParticlePack/EffectExamples/Misc Effects/Textures/DustMote.png";

    // Palette
    private static readonly Color DustColor = new Color(0.78f, 0.85f, 0.9f, 0.75f);    // cool light grey, corridor lighting
    private static readonly Color DebrisColor = new Color(0.82f, 0.88f, 0.92f, 0.9f);
    private static readonly Color PlayerCyan = new Color(0f, 0.95f, 1f, 0.85f);        // = player plasma #00F1FF
    private static readonly Color AlienCharge = new Color(1f, 0.45f, 0.08f, 1f);        // = alien shot #FF7200
    private static readonly Color AlienBlood = new Color(0.47f, 0.16f, 0.02f, 0.92f);  // dark amber alien blood

    [MenuItem("Tools/Gameplay VFX/Build And Wire All")]
    public static void BuildAll()
    {
        Directory.CreateDirectory(TexDir);
        Directory.CreateDirectory(MatDir);
        Directory.CreateDirectory(PrefabDir);

        var ringTex = SaveTexture(TexDir + "/T_VFX_SoftRing.png", BuildRingTexture(256));
        var bloodTex = SaveTexture(TexDir + "/T_AlienBloodSplat.png", BuildBloodTexture(512, 7));

        var dustMat = ParticleMaterial(MatDir + "/M_VFX_Dust.mat", AssetDatabase.LoadAssetAtPath<Texture2D>(SmokePuffTex), false);
        var debrisMat = ParticleMaterial(MatDir + "/M_VFX_Debris.mat", AssetDatabase.LoadAssetAtPath<Texture2D>(DustMoteTex), false);
        var ringMat = ParticleMaterial(MatDir + "/M_VFX_JumpRing.mat", ringTex, true);
        var bloodMat = BloodMaterial(MatDir + "/M_AlienBloodPool.mat", bloodTex);

        var slideDust = BuildSlideDust(dustMat, debrisMat);
        var jumpImpulse = BuildJumpImpulse(dustMat, ringMat);
        var bloodDecal = BuildBloodDecal(bloodMat);

        WirePlayer(slideDust, jumpImpulse);
        WireEnemy(EnemyRangePrefab, bloodDecal, true);
        WireEnemy(EnemyCacPrefab, bloodDecal, false);

        AssetDatabase.SaveAssets();
        Debug.Log("[GameplayVFXBuilder] VFX built and wired (player slide/jump, enemy charge, blood pool).");
    }

    // ================================================================== Textures

    private static Texture2D BuildRingTexture(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float u = (x + 0.5f) / size * 2f - 1f, v = (y + 0.5f) / size * 2f - 1f;
            float r = Mathf.Sqrt(u * u + v * v);
            // Sharp outer edge, soft inner trail: reads as a shockwave front.
            float outer = Mathf.Clamp01((0.92f - r) / 0.04f);
            float inner = Mathf.Exp(-Mathf.Pow((0.86f - r) / 0.12f, 2f));
            float a = Mathf.Clamp01(outer * inner);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D BuildBloodTexture(int size, int seed)
    {
        var rng = new System.Random(seed);
        float Rand(float a, float b) => a + (float)rng.NextDouble() * (b - a);

        // Main pool = union of soft blobs, plus droplets thrown around.
        var blobs = new System.Collections.Generic.List<Vector3>(); // x, y, radius (uv -1..1)
        blobs.Add(new Vector3(0f, 0f, 0.42f));
        for (int i = 0; i < 9; i++)
        {
            float ang = Rand(0f, Mathf.PI * 2f), dist = Rand(0.15f, 0.42f);
            blobs.Add(new Vector3(Mathf.Cos(ang) * dist, Mathf.Sin(ang) * dist, Rand(0.12f, 0.26f)));
        }
        for (int i = 0; i < 16; i++)
        {
            float ang = Rand(0f, Mathf.PI * 2f), dist = Rand(0.55f, 0.88f);
            blobs.Add(new Vector3(Mathf.Cos(ang) * dist, Mathf.Sin(ang) * dist, Rand(0.015f, 0.05f)));
        }

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        float px = 2f / size;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float u = (x + 0.5f) / size * 2f - 1f, v = (y + 0.5f) / size * 2f - 1f;
            // Ragged edge: angular noise on the distance.
            float ang = Mathf.Atan2(v, u);
            float wobble = 0.035f * Mathf.Sin(ang * 7f + 1.3f) + 0.02f * Mathf.Sin(ang * 13f + 4.1f);

            float field = float.MaxValue; // signed distance to the union of blobs
            foreach (var b in blobs)
            {
                float d = Mathf.Sqrt((u - b.x) * (u - b.x) + (v - b.y) * (v - b.y)) - b.z;
                field = Mathf.Min(field, d);
            }
            field -= wobble * Mathf.Clamp01(1f - Mathf.Abs(field) * 4f);

            float alpha = Mathf.Clamp01(-field / (px * 2.5f) + 0.5f);
            // Thicker (darker) in the middle, lighter rim: gives the pool some volume once tinted.
            float depth = Mathf.Clamp01(-field / 0.25f);
            float shade = Mathf.Lerp(1f, 0.55f, depth);
            float rim = Mathf.Clamp01(1f - Mathf.Abs(field + 0.02f) / 0.02f) * 0.35f;
            shade = Mathf.Clamp01(shade + rim);
            tex.SetPixel(x, y, new Color(shade, shade, shade, alpha));
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D SaveTexture(string path, Texture2D tex)
    {
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Default;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = true;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // ================================================================== Materials

    private static Material ParticleMaterial(string path, Texture tex, bool additive)
    {
        var source = AssetDatabase.LoadAssetAtPath<Material>(AlphaParticleSource);
        var mat = new Material(source) { name = Path.GetFileNameWithoutExtension(path) };
        mat.SetTexture("_BaseMap", tex);
        mat.SetColor("_BaseColor", Color.white);
        if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", Color.black);
        mat.DisableKeyword("_EMISSION");
        if (additive)
        {
            mat.SetFloat("_Blend", 2f);              // URP particles: Additive
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
        return SaveMaterial(path, mat);
    }

    private static Material BloodMaterial(string path, Texture tex)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = Path.GetFileNameWithoutExtension(path) };
        mat.SetTexture("_BaseMap", tex);
        mat.SetColor("_BaseColor", AlienBlood);
        mat.SetFloat("_Surface", 1f); // transparent
        mat.SetFloat("_Blend", 0f);   // alpha
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent - 50; // under the particles
        return SaveMaterial(path, mat);
    }

    private static Material SaveMaterial(string path, Material mat)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing)
        {
            existing.shader = mat.shader;
            existing.CopyPropertiesFromMaterial(mat);
            existing.shaderKeywords = mat.shaderKeywords;
            existing.renderQueue = mat.renderQueue;
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(mat);
            return existing;
        }
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    // ================================================================== Prefabs

    private static GameObject BuildSlideDust(Material dustMat, Material debrisMat)
    {
        var root = new GameObject("VFX_SlideDust");
        RootSystem(root, loop: true);

        // Puffs kicked up under the feet, spreading and fading behind the player.
        var puffs = NewSystem(root.transform, "Puffs", dustMat, upward: true);
        var main = puffs.main;
        main.loop = true;
        main.duration = 1f;
        // 6–10 smoke puffs on screen at most: ~14/s × ~0.6 s life ≈ 8, hard-capped at 10.
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = DustColor;
        main.maxParticles = 10;
        var emission = puffs.emission;
        emission.rateOverTime = 14f;
        var shape = puffs.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.25f;
        Grow(puffs, 0.6f, 2.3f);
        FadeInOut(puffs, 0.15f, 1f);
        Spin(puffs, 50f);
        FollowFloor(puffs);

        // Small grit flicked up by the friction.
        var debris = NewSystem(puffs.transform, "Grit", debrisMat, upward: false);
        main = debris.main;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.045f);
        main.startColor = DebrisColor;
        main.gravityModifier = 1.2f;
        main.maxParticles = 8;
        emission = debris.emission;
        emission.rateOverTime = 18f;
        shape = debris.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.15f;
        FadeInOut(debris, 0f, 1f);
        FollowFloor(debris);

        return SavePrefab(root, PrefabDir + "/VFX_SlideDust.prefab");
    }

    private static GameObject BuildJumpImpulse(Material dustMat, Material ringMat)
    {
        var root = new GameObject("VFX_JumpImpulse");
        RootSystem(root, loop: false);

        // Shockwave on the floor: the "push" of the jump, in the player's cyan.
        var ring = NewSystem(root.transform, "Shockwave", ringMat, upward: false);
        var main = ring.main;
        main.loop = false;
        main.duration = 0.5f;
        main.startLifetime = 0.35f;
        main.startSpeed = 0f;
        main.startSize = 0.5f;
        main.startColor = PlayerCyan;
        main.maxParticles = 4;
        main.simulationSpace = ParticleSystemSimulationSpace.Local; // centred on the player (pinned by PlayerMovementVFX)
        Burst(ring, 1);
        ring.GetComponent<ParticleSystemRenderer>().renderMode = ParticleSystemRenderMode.HorizontalBillboard;
        Grow(ring, 1f, 5.5f, easeOut: true);
        FadeInOut(ring, 0f, 1f);

        // Dust blown outward along the floor.
        var dust = NewSystem(root.transform, "DustBurst", dustMat, upward: false);
        dust.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // circle lies on the floor
        main = dust.main;
        main.loop = false;
        main.duration = 0.5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.55f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = DustColor;
        main.simulationSpace = ParticleSystemSimulationSpace.Local; // stays around the ring
        main.maxParticles = 8;
        Burst(dust, 8); // 6–10 smoke puffs max
        var shape = dust.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.15f;
        shape.radiusThickness = 0f; // emit from the edge, straight outward
        var limit = dust.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.limit = 0.6f;
        limit.dampen = 0.2f;
        Grow(dust, 0.7f, 1.7f);
        FadeInOut(dust, 0.1f, 1f);
        Spin(dust, 60f);

        return SavePrefab(root, PrefabDir + "/VFX_JumpImpulse.prefab");
    }

    private static BloodDecal BuildBloodDecal(Material mat)
    {
        var go = new GameObject("VFX_AlienBloodPool");
        go.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        go.AddComponent<BloodDecal>();
        return SavePrefab(go, PrefabDir + "/VFX_AlienBloodPool.prefab").GetComponent<BloodDecal>();
    }

    // ================================================================== Wiring

    private static void WirePlayer(GameObject slideDustPrefab, GameObject jumpPrefab)
    {
        var root = PrefabUtility.LoadPrefabContents(PlayerPrefab);
        try
        {
            ReplaceChild(root.transform, slideDustPrefab, new Vector3(0f, 0.05f, 0f));
            ReplaceChild(root.transform, jumpPrefab, new Vector3(0f, 0.03f, 0f));

            var vfx = root.GetComponent<PlayerMovementVFX>();
            if (vfx == null) vfx = root.AddComponent<PlayerMovementVFX>();
            var so = new SerializedObject(vfx);
            so.FindProperty("_player").objectReferenceValue = root.GetComponent<PlayerController>();
            so.FindProperty("_slideDust").objectReferenceValue = root.transform.Find(slideDustPrefab.name).GetComponent<ParticleSystem>();
            so.FindProperty("_jumpImpulse").objectReferenceValue = root.transform.Find(jumpPrefab.name).GetComponent<ParticleSystem>();
            so.ApplyModifiedPropertiesWithoutUndo();

            var pcSo = new SerializedObject(root.GetComponent<PlayerController>());
            pcSo.FindProperty("_movementVfx").objectReferenceValue = vfx;
            pcSo.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void ReplaceChild(Transform parent, GameObject prefab, Vector3 localPos)
    {
        var old = parent.Find(prefab.name);
        if (old) Object.DestroyImmediate(old.gameObject);
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        inst.name = prefab.name;
        inst.transform.localPosition = localPos;
        inst.transform.localRotation = Quaternion.identity;
    }

    private static void WireEnemy(string path, BloodDecal blood, bool isRange)
    {
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var enemy = root.GetComponent<EnemyHandler>();
            var so = new SerializedObject(enemy);
            so.FindProperty("_bloodDecalPrefab").objectReferenceValue = blood;
            so.FindProperty("_floorMask").intValue = LayerMask.GetMask("Modul");
            so.ApplyModifiedPropertiesWithoutUndo();

            if (isRange) TuneChargeTelegraph(root);

            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    /// <summary>
    /// The alien gun charge (VFX_AlienArme_Implosion) was one 0.2–0.5 s burst, dark brown, and played
    /// when the enemy first saw the player – not right before the shot. Now: a continuous, brighter
    /// charge that builds up over WeaponHandler._chargeDuration (0.9 s) and loops until the shot.
    /// Changes are prefab overrides on this enemy only; Marion's source VFX is untouched.
    /// </summary>
    private static void TuneChargeTelegraph(GameObject enemyRoot)
    {
        const float charge = 0.9f;
        var implosion = FindDeep(enemyRoot.transform, "VFX_AlienArme_Implosion");
        if (!implosion) { Debug.LogWarning("[GameplayVFXBuilder] Charge VFX not found."); return; }

        // Sparks sucked into the barrel.
        var sparks = implosion.GetComponent<ParticleSystem>();
        var main = sparks.main;
        main.loop = true;
        main.duration = charge;
        main.startColor = AlienCharge;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.35f);
        var emission = sparks.emission;
        emission.rateOverTime = 40f;

        // Core glow growing until the shot.
        var core = implosion.Find("Rond")?.GetComponent<ParticleSystem>();
        if (core)
        {
            main = core.main;
            main.loop = true;
            main.duration = charge;
            main.startLifetime = charge;
            main.startSize = 0.75f;
            main.startColor = AlienCharge;
            var sol = core.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.15f), new Keyframe(0.85f, 1f), new Keyframe(1f, 1.15f)));
            var col = core.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0.2f, 0f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
        }
    }

    // ================================================================== Particle helpers

    private static ParticleSystem NewSystem(Transform parent, string name, Material mat, bool upward)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        if (upward) go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // shape +Z → world up
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = mat;
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        return ps;
    }

    /// <summary>Empty parent system so Play / Stop(withChildren) drive the whole effect at once.</summary>
    private static void RootSystem(GameObject go, bool loop)
    {
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = loop;
        main.duration = loop ? 1f : 0.6f;
        var emission = ps.emission;
        emission.enabled = false;
        var shape = ps.shape;
        shape.enabled = false;
        go.GetComponent<ParticleSystemRenderer>().enabled = false;
    }

    private static void Burst(ParticleSystem ps, int count)
    {
        var e = ps.emission;
        e.rateOverTime = 0f;
        e.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
    }

    private static void Grow(ParticleSystem ps, float from, float to, bool easeOut = false)
    {
        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        var curve = easeOut
            ? new AnimationCurve(new Keyframe(0f, from, 0f, (to - from) * 3f), new Keyframe(1f, to, 0f, 0f))
            : AnimationCurve.Linear(0f, from, 1f, to);
        sol.size = new ParticleSystem.MinMaxCurve(1f, curve);
    }

    private static void FadeInOut(ParticleSystem ps, float fadeIn, float peakAlpha)
    {
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  fadeIn > 0f
                      ? new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(peakAlpha, fadeIn), new GradientAlphaKey(0f, 1f) }
                      : new[] { new GradientAlphaKey(peakAlpha, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = g;
    }

    private static void Spin(ParticleSystem ps, float degPerSec)
    {
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-degPerSec * Mathf.Deg2Rad, degPerSec * Mathf.Deg2Rad);
    }

    /// <summary>Marks the system as "glued to the scrolling floor" (PlayerMovementVFX sets the speed).</summary>
    private static void FollowFloor(ParticleSystem ps)
    {
        var vol = ps.velocityOverLifetime;
        vol.enabled = true;
        vol.space = ParticleSystemSimulationSpace.World;
        vol.x = new ParticleSystem.MinMaxCurve(0f);
        vol.y = new ParticleSystem.MinMaxCurve(0f);
        vol.z = new ParticleSystem.MinMaxCurve(-8f);
    }

    private static GameObject SavePrefab(GameObject go, string path)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static Transform FindDeep(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t)
        {
            var r = FindDeep(c, name);
            if (r) return r;
        }
        return null;
    }
}
