using System;
using System.Collections;
using BattleK.Scripts.Data;
using BattleK.Scripts.Manager;
using UnityEngine;

/// <summary>
/// 훈련소 / 치유소 / 뽑기 등 여러 화면에서 공통으로 사용하는
/// 캐릭터 정보(CharacterData) 및 초상화(Sprite) 조회 유틸리티.
///
/// 상태를 갖지 않는 static 클래스이며, Sprite 캐싱/해제 책임은
/// 호출하는 화면(리스트/부모)이 소유한 AddressableAssetLoader<Sprite>에 있음.
/// </summary>
public static class CharacterInfoProvider
{
    /// <summary>
    /// unitId로 CharacterData를 조회.
    /// </summary>
    public static CharacterData GetCharacterData(string unitId)
    {
        if (string.IsNullOrEmpty(unitId))
        {
            Debug.LogWarning("[CharacterInfoProvider] unitId가 비어있습니다.");
            return null;
        }

        CharacterData data = UnitDataManager.Instance.GetCharacterData(unitId);

        if (data == null)
        {
            Debug.LogWarning($"[CharacterInfoProvider] CharacterData를 찾을 수 없습니다: {unitId}");
        }

        return data;
    }

    /// <summary>
    /// CharacterData의 Visuals.Portrait 키로 초상화 Sprite를 로드.
    /// 로더는 호출부(화면)가 소유한 인스턴스를 전달받아 사용.
    /// </summary>
    public static IEnumerator LoadPortraitAsync(
        AddressableAssetLoader<Sprite> loader,
        CharacterData data,
        Action<Sprite> onSuccess,
        Action onFail = null)
    {
        if (loader == null)
        {
            Debug.LogError("[CharacterInfoProvider] loader가 null입니다.");
            onFail?.Invoke();
            yield break;
        }

        if (data?.Visuals == null || string.IsNullOrEmpty(data.Visuals.Portrait))
        {
            Debug.LogWarning($"[CharacterInfoProvider] Portrait 키가 없습니다. Unit_ID: {data?.Unit_ID}");
            onFail?.Invoke();
            yield break;
        }

        string key = BuildPortraitKey(data.Visuals.Portrait);
        yield return loader.LoadByKeyAsync(key, onSuccess, onFail);
    }

    /// <summary>
    /// Visuals.Portrait 원본 값(예: "Astra_Selene.png")을
    /// 실제 Addressable 키(예: "Portrait/Astra_Selene")로 변환.
    /// Visuals.Portrait에는 확장자가 포함된 파일명이 저장되어 있고,
    /// 실제 등록된 키는 "Portrait/{Family_Name}_{Unit_Name}" 형식이므로
    /// 확장자를 제거한 뒤 BuildKey로 prefix를 조합함.
    /// </summary>
    private static string BuildPortraitKey(string rawPortrait)
    {
        string nameOnly = System.IO.Path.GetFileNameWithoutExtension(rawPortrait);
        return AddressableAssetLoader<Sprite>.BuildKey(AddressableAssetType.Character, nameOnly);
    }

    /// <summary>
    /// unitId만으로 초상화 Sprite를 로드하는 편의 오버로드.
    /// 내부적으로 CharacterData를 조회한 뒤 Visuals.Portrait를 정본으로 사용하여 로드함.
    /// (Unit_ID로 키를 직접 조합하지 않는 이유: 스킨/이벤트 초상화 등으로
    ///  향후 Unit_ID와 Portrait 키가 어긋날 경우를 대비해 Visuals.Portrait 하나만 정본으로 유지하기 위함)
    /// </summary>
    public static IEnumerator LoadPortraitByUnitIdAsync(
        AddressableAssetLoader<Sprite> loader,
        string unitId,
        Action<Sprite> onSuccess,
        Action onFail = null)
    {
        CharacterData data = GetCharacterData(unitId);

        if (data == null)
        {
            onFail?.Invoke();
            yield break;
        }

        yield return LoadPortraitAsync(loader, data, onSuccess, onFail);
    }

    /// <summary>
    /// unitId로 CharacterData 조회 + 초상화 Sprite 로드를 한 번에 처리.
    /// (Data가 없으면 Sprite 로드를 시도하지 않고 즉시 실패 콜백)
    /// </summary>
    public static IEnumerator LoadCharacterInfoAsync(
        AddressableAssetLoader<Sprite> loader,
        string unitId,
        Action<CharacterData, Sprite> onSuccess,
        Action onFail = null)
    {
        CharacterData data = GetCharacterData(unitId);

        if (data == null)
        {
            onFail?.Invoke();
            yield break;
        }

        yield return LoadPortraitAsync(
            loader,
            data,
            sprite => onSuccess?.Invoke(data, sprite),
            onFail);
    }
}