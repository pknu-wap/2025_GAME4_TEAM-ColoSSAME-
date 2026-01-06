using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FighterNameBinder : MonoBehaviour
{
    [Header("fighter 슬롯들이 들어있는 부모(예: fighterList)")]
    public Transform fighterListParent;

    [Header("playerTrain 안의 표시 텍스트(Text Legacy)")]
    public Text curLevelText; // playerTrain/CurLevel/Text(Legacy)
    public Text curExpText;   // playerTrain/CurEXP/Text(Legacy)

    private IEnumerator Start()
    {
        yield return null;

        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogError("❌ UserManager 또는 user가 준비되지 않았습니다.");
            yield break;
        }

        if (fighterListParent == null)
        {
            Debug.LogError("❌ fighterListParent가 비어있습니다. Inspector에 fighterList를 넣어주세요.");
            yield break;
        }

        var myUnits = UserManager.Instance.user.myUnits;
        if (myUnits == null)
        {
            Debug.LogError("❌ myUnits가 null입니다.");
            yield break;
        }

        int count = Mathf.Min(fighterListParent.childCount, myUnits.Count);

        for (int i = 0; i < fighterListParent.childCount; i++)
        {
            Transform slot = fighterListParent.GetChild(i);

            // 1) 슬롯 내 텍스트 찾기(이름 표시용)
            Text nameText = slot.GetComponentInChildren<Text>(true);

            // 2) 슬롯 데이터 컴포넌트 확보(없으면 자동 추가)
            FighterSlotData data = slot.GetComponent<FighterSlotData>();
            if (data == null) data = slot.gameObject.AddComponent<FighterSlotData>();

            // 3) 클릭 표시 스크립트 확보(없으면 자동 추가)
            FighterSlotShowStats show = slot.GetComponent<FighterSlotShowStats>();
            if (show == null) show = slot.gameObject.AddComponent<FighterSlotShowStats>();

            // 4) 슬롯에 공용 UI 참조 연결(한 번만 해두면 됨)
            show.slotData = data;
            show.curLevelText = curLevelText;
            show.curExpText = curExpText;

            // 5) 유닛이 있는 슬롯이면 이름 + unitId 저장
            if (i < myUnits.Count && myUnits[i] != null)
            {
                // 이름 표시
                if (nameText != null) nameText.text = myUnits[i].unitName;

                // ✅ 추천 B의 핵심: 슬롯에 unitId 저장
                data.unitId = myUnits[i].unitId;
            }
            else
            {
                // 유닛이 없는 슬롯은 비워두고 클릭 시 무시되게
                if (nameText != null) nameText.text = "";
                data.unitId = "";
            }
        }

        Debug.Log("✅ FighterNameBinder: 슬롯 이름+unitId+클릭(표시) 세팅 완료");
    }
}
