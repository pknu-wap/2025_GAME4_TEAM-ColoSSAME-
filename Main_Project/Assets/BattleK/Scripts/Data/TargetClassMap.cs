// TargetClassMap.cs
using System.Collections.Generic;

public static class TargetClassMap
{
    // 한글 라벨 → Enum
    public static readonly Dictionary<string, UnitClass> KoToEnum = new()
    {
        ["검사"]   = UnitClass.Swordsman, // 프로젝트 실제 Enum 명칭에 맞게 사용
        ["도끼병"] = UnitClass.Axeman,
        ["창병"]   = UnitClass.Spearman,
        ["도적"]   = UnitClass.Thief,
        ["궁수"]   = UnitClass.Archer,
        ["마법사"] = UnitClass.Mage,
    };

    // Enum → 한글 라벨 (UI 복원용)
    public static readonly Dictionary<UnitClass, string> EnumToKo = new()
    {
        [UnitClass.Swordsman] = "검사",
        [UnitClass.Axeman]    = "도끼병",
        [UnitClass.Spearman]  = "창병",
        [UnitClass.Thief]     = "도적",
        [UnitClass.Archer]    = "궁수",
        [UnitClass.Mage]      = "마법사",
    };

    /// <summary>
    /// 한글 라벨을 UnitClass로 변환. 어떤 경로로 끝나도 out 매개변수는 대입됨.
    /// </summary>
    public static bool TryKoToEnum(string label, out UnitClass cls)
        => KoToEnum.TryGetValue((label ?? string.Empty).Trim(), out cls);

    /// <summary>
    /// Enum을 한글 라벨로 변환. 매핑 없으면 enum 이름을 그대로 반환.
    /// </summary>
    public static string ToKo(UnitClass cls)
        => EnumToKo.TryGetValue(cls, out var ko) ? ko : cls.ToString();
}