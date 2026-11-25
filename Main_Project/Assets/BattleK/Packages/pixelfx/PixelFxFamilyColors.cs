using System.Collections.Generic;
using BattleK.Scripts.Data;
using UnityEngine;

// 가문별 이펙트 색. 파란 기준색으로 뽑은 시트를 시전자 가문 색으로 바꿔 칠함 (Resources/PixelFxFamilyColors)
[CreateAssetMenu(menuName = "pixelfx/Family Colors", fileName = "PixelFxFamilyColors")]
public class PixelFxFamilyColors : ScriptableObject
{
    [System.Serializable]
    public class Family
    {
        public string key;       // FamilyID.FamilyKey
        public Color[] ramp;     // 어두운 쪽부터 8단계 (기준색 순서대로 바뀜)
    }

    public Shader shader;
    public Color[] from = Ramp("#0f1638", "#212f6b", "#2c58aa", "#3a8bd8", "#62c8f4", "#b8c9e8", "#e8f1ff", "#ffffff");
    public Family[] families =
    {
        F("Astra", "#170524", "#2a0a40", "#5a1a7a", "#8e2ab8", "#c24ae6", "#e68aff", "#f8d0ff", "#ffffff"),
        F("Caelus", "#0f1638", "#212f6b", "#2c58aa", "#3a8bd8", "#62c8f4", "#b8c9e8", "#e8f1ff", "#ffffff"),
        F("Flora", "#29091a", "#4a1030", "#8a2257", "#c9437f", "#f07aa8", "#ffb3cf", "#ffe3ee", "#ffffff"),
        F("Fulger", "#171703", "#2a2a05", "#6a6010", "#b8a812", "#f2e23a", "#fff27a", "#fffcc8", "#ffffff"),
        F("Ignis", "#200503", "#3a0a05", "#7a1a0a", "#c93a12", "#f26a2a", "#ffa64d", "#ffe0a0", "#ffffff"),
        F("Lumen", "#201709", "#3a2a10", "#6b4a10", "#b8862a", "#f2c94c", "#fff3b0", "#fffbe6", "#ffffff"),
        F("Mare", "#031a20", "#05303a", "#0a5a6b", "#128a9a", "#2ac0c4", "#7ae8e0", "#d0fff8", "#ffffff"),
        F("Mors", "#050e05", "#0a1a0a", "#1a3a1a", "#2e6a2a", "#4fa040", "#8ad870", "#d0ffc0", "#ffffff"),
        F("Nox", "#050308", "#0a0510", "#1e1030", "#3a2058", "#5a3a88", "#8a70b8", "#c8bde0", "#ffffff"),
        F("Terra", "#170e05", "#2a1a0a", "#5a3a14", "#8a5a22", "#b8863a", "#e0b870", "#f5e2c0", "#ffffff"),
    };

    private static PixelFxFamilyColors _data;
    private static readonly Dictionary<string, Material> Cache = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache()
    {
        _data = null;
        Cache.Clear();
    }

    // 시전자 가문 색으로 이펙트 그림을 칠함 (가문이 없거나 표에 없으면 그대로)
    public static void Apply(GameObject fx, Component owner)
    {
        if (!fx || !owner) return;
        var id = owner.GetComponentInParent<FamilyID>();
        if (!id) id = owner.GetComponentInChildren<FamilyID>();
        if (!id || string.IsNullOrWhiteSpace(id.FamilyKey)) return;

        var mat = MaterialFor(id.FamilyKey.Trim());
        if (!mat) return;
        foreach (var sr in fx.GetComponentsInChildren<SpriteRenderer>()) sr.sharedMaterial = mat;
    }

    private static Material MaterialFor(string key)
    {
        if (Cache.TryGetValue(key, out var cached)) return cached;
        if (!_data) _data = Resources.Load<PixelFxFamilyColors>("PixelFxFamilyColors");

        Material mat = null;
        var family = _data ? System.Array.Find(_data.families, f => string.Equals(f.key, key, System.StringComparison.OrdinalIgnoreCase)) : null;
        if (family != null && _data.shader && family.ramp.Length == 8 && _data.from.Length == 8)
        {
            mat = new Material(_data.shader) { name = $"PixelFxFamily_{key}" };
            mat.SetVectorArray("_From", System.Array.ConvertAll(_data.from, c => (Vector4)c));
            mat.SetVectorArray("_To", System.Array.ConvertAll(family.ramp, c => (Vector4)c));
        }
        Cache[key] = mat;
        return mat;
    }

    private static Family F(string key, params string[] hex) => new() { key = key, ramp = Ramp(hex) };

    private static Color[] Ramp(params string[] hex) =>
        System.Array.ConvertAll(hex, h => ColorUtility.TryParseHtmlString(h, out var c) ? c : Color.magenta);
}
