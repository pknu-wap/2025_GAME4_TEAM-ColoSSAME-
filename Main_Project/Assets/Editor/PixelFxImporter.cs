using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

// pixelfx 시트(PNG + JSON)로 스프라이트, 애니메이션, 프리팹을 만듦
public static class PixelFxImporter
{
    private const string Root = "Assets/BattleK/Packages/pixelfx";
    private const int PixelsPerUnit = 32; // 기존 이펙트, SPUM과 같게

    [System.Serializable] private class Rect4 { public int x, y, w, h; }
    [System.Serializable] private class Frame { public Rect4 frame; }
    [System.Serializable] private class FxMeta { public int fps = 30; public bool loop; public int[] frameSize; public int[] pivot; }
    [System.Serializable] private class Size { public int w, h; }
    [System.Serializable] private class Meta { public Size size; public FxMeta pixelfx; }
    [System.Serializable] private class Sheet { public Frame[] frames; public Meta meta; }

    [MenuItem("Tools/pixelfx/시트 가져오기")]
    public static void ImportAll()
    {
        EnsureFolder(Root + "/Animations");
        EnsureFolder(Root + "/Prefabs");

        var jsons = Directory.GetFiles(Root + "/Sheets", "*.json").Select(p => p.Replace('\\', '/')).ToArray();
        foreach (var json in jsons)
        {
            var png = Path.ChangeExtension(json, ".png");
            if (File.Exists(png)) Import(png, json);
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"pixelfx: {jsons.Length}개 가져옴");
    }

    // 시트 하나만 다시 가져오기 (시트를 바꾼 뒤 스킬 메뉴에서 씀)
    public static void ImportSheet(string name)
    {
        EnsureFolder(Root + "/Animations");
        EnsureFolder(Root + "/Prefabs");
        var json = $"{Root}/Sheets/{name}.json";
        var png = $"{Root}/Sheets/{name}.png";
        if (File.Exists(json) && File.Exists(png)) Import(png, json);
        else Debug.LogWarning($"pixelfx: {name} 시트 없음");
    }

    private static void Import(string png, string jsonPath)
    {
        var sheet = JsonUtility.FromJson<Sheet>(File.ReadAllText(jsonPath));
        var fx = sheet.meta.pixelfx;
        var name = Path.GetFileNameWithoutExtension(png);

        // 1. 스프라이트 자르기
        var ti = (TextureImporter)AssetImporter.GetAtPath(png);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        ti.spritePixelsPerUnit = PixelsPerUnit;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.maxTextureSize = 4096;
        int texH = sheet.meta.size.h;

        // pixelfx 기준점은 왼쪽 위에서 잰 px, Unity는 왼쪽 아래 기준 0~1
        var pivot = new Vector2(fx.pivot[0] / (float)fx.frameSize[0], 1f - fx.pivot[1] / (float)fx.frameSize[1]);
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dp = factory.GetSpriteEditorDataProviderFromObject(ti);
        dp.InitSpriteEditorDataProvider();
        // 다시 가져올 때 같은 이름은 같은 ID 유지
        var oldIds = dp.GetSpriteRects().GroupBy(r => r.name).ToDictionary(g => g.Key, g => g.First().spriteID);
        var rects = new SpriteRect[sheet.frames.Length];
        for (int i = 0; i < rects.Length; i++)
        {
            var f = sheet.frames[i].frame;
            var spriteName = $"{name}_{i:000}";
            rects[i] = new SpriteRect
            {
                name = spriteName,
                rect = new Rect(f.x, texH - f.y - f.h, f.w, f.h),
                alignment = SpriteAlignment.Custom,
                pivot = pivot,
                spriteID = oldIds.TryGetValue(spriteName, out var id) ? id : GUID.Generate(),
            };
        }
        dp.SetSpriteRects(rects);
        dp.GetDataProvider<ISpriteNameFileIdDataProvider>()
          .SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
        dp.Apply();
        ti.SaveAndReimport();

        var sprites = AssetDatabase.LoadAllAssetsAtPath(png).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (sprites.Length == 0) { Debug.LogWarning($"pixelfx: {name} 스프라이트 없음"); return; }

        // 2. 애니메이션 클립
        var clip = new AnimationClip { frameRate = fx.fps };
        var keys = new ObjectReferenceKeyframe[sprites.Length + 1];
        for (int i = 0; i < sprites.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / (float)fx.fps, value = sprites[i] };
        keys[sprites.Length] = new ObjectReferenceKeyframe { time = sprites.Length / (float)fx.fps, value = sprites[sprites.Length - 1] };
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = fx.loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        clip = SaveAsset(clip, $"{Root}/Animations/{name}.anim");

        // 3. 컨트롤러 (다시 가져와도 같은 파일 유지)
        var ctrlPath = $"{Root}/Animations/{name}.controller";
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath)
                   ?? AnimatorController.CreateAnimatorControllerAtPathWithClip(ctrlPath, clip);

        // 4. 프리팹
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprites[0];
        sr.sortingOrder = 1;
        go.AddComponent<Animator>().runtimeAnimatorController = ctrl;
        if (!fx.loop) go.AddComponent<PixelFxAutoDestroy>().lifetime = clip.length;
        PrefabUtility.SaveAsPrefabAsset(go, $"{Root}/Prefabs/{name}.prefab");
        Object.DestroyImmediate(go);
    }

    private static T SaveAsset<T>(T obj, string path) where T : Object
    {
        var old = AssetDatabase.LoadAssetAtPath<T>(path);
        if (old == null) { AssetDatabase.CreateAsset(obj, path); return obj; }
        EditorUtility.CopySerialized(obj, old);
        return old;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
