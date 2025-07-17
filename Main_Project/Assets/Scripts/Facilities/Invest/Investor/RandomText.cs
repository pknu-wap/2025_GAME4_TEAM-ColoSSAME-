using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class RandomText : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;

    public Button[] stage1Buttons; 
    public Button[] stage2Buttons;
    public Transform[] finalStageParents;

    private List<Button> stage3Buttons = new List<Button>();    

    private string[] names = {
        "마르쿠스 율리우스",
        "가이우스 카시우스",
        "루키우스 코르넬리우스",
        "티투스 플라미니누스",
        "세르빌리우스 프리스쿠스",
        "아우렐리우스 바수스",
        "데키우스 브루투스",
        "가이우스 안토니우스"
    };

    private string[] infos = {
        "나는 정의의 칼을 쥔 자, 나와의 거래는 너의 명예에 흠이 없어야 한다.",
        "신들이여, 이 협상에 축복을! 너의 제안을 들어보지.",
        "나 루키우스의 조건은 명확하다. 감정을 앞세우지 말지어다.",
        "플라미니누스 가문은 전통을 중시하지. 너는 그 가치를 이해할 자인가?",
        "프리스쿠스는 이익보다 명분을 따르지. 너의 논리를 펼쳐보아라.",
        "진실만을 말하라. 나는 말 속의 숨겨진 칼날을 감지할 수 있다.",
        "브루투스는 기회를 본다. 너의 제안이 날 움직일 수 있을지 궁금하군.",
        "내 관심을 사려면 그저 금이나 권력으론 부족하네. 무얼 준비했나?"
    };

    string[] stage1Choices = {
        "당신의 명성을 익히 들었습니다. 평화롭게 시작해봅시다.",
        "먼저 인사를 드리는 것이 예의겠지요. 제 이야기를 들어보시겠습니까?",
        "제가 먼저 제안을 드리겠습니다. 부담 갖지 마시고 들어주세요.",
        "우리는 서로에게 이득이 될 길을 찾을 수 있을 겁니다.",
        "단도직입적으로 말하진 않겠습니다. 마음을 여는 것이 먼저니까요.",
        "제가 가진 정보가 흥미로우실지도 모르겠습니다."
    };

    string[] stage2Choices = {
        "이 제안은 귀하의 입장도 충분히 고려한 것입니다.",
        "조건을 조정할 여지는 있습니다. 그러나 상호 존중이 전제되어야 합니다.",
        "저는 신뢰를 중요시합니다. 지금이 그 첫걸음이겠지요.",
        "양보할 수 있는 지점과 고수할 조건을 분명히 하고 싶습니다.",
        "제안을 수락하신다면, 당신도 큰 이익을 얻게 될 겁니다.",
        "협상이란 결국 타이밍입니다. 지금이 그때 아닐까요?",
        "지금이 바로 그 때입니다. 망설일 이유가 없습니다.",
        "이 제안은 당신에게도 새로운 기회를 열어줄 것입니다.",
        "우리는 이 제안을 통해 미래를 함께 설계할 수 있습니다."
    };

    string[] stage3Choices = {
        "지금 결단하지 않으면 기회는 다시 오지 않을 겁니다.",
        "이제는 서로의 최종 의도를 확인해야 할 때입니다.",
        "제가 가진 마지막 카드를 보여드리겠습니다.",
        "이 제안이 받아들여지지 않는다면, 나도 물러날 수밖에 없습니다.",
        "우리 모두는 결과를 감수할 준비가 되어 있어야 합니다.",
        "당신의 선택이 미래를 결정짓습니다. 신중히 선택하십시오.",
        "이 기회는 양측에게 유일무이한 선택지입니다.",
        "더는 숨길 것도, 미룰 것도 없습니다. 결단의 순간입니다.",
        "당신의 결단이 우리 모두의 운명을 바꿉니다.",
        "이 협상은 단순한 거래가 아니라 믿음의 표현입니다.",
        "이제는 말이 아닌 행동으로 보여줘야 할 시간입니다.",
        "내 제안은 단순하지만 강력합니다. 받아들이시겠습니까?",
        "이 선택은 당신에게 명예로운 길이 될 것입니다.",
        "우리는 이미 많은 것을 공유했습니다. 결론을 내려봅시다.",
        "내가 제안한 조건은 최선입니다. 더는 후퇴하지 않겠습니다.",
        "당신의 응답을 기다리겠습니다. 지금 바로.",
        "결정하지 않으면 모든 것이 무의미해질 수 있습니다.",
        "내가 가진 카드는 마지막 하나입니다. 이 순간을 놓치지 마세요.",
        "이 순간을 놓치면, 모든 것은 무로 돌아갑니다.",
        "신중하게 선택하십시오. 이것이 마지막 기회입니다.",
        "이 결정은 당신의 리더십을 증명할 수 있는 기회입니다.",
        "우리는 지금 매우 중요한 갈림길에 서 있습니다.",
        "이 조건은 전례 없는 혜택을 제공합니다.",
        "우리는 서로를 이해할 수 있는 마지막 기회를 맞이했습니다.",
        "결단은 당신의 몫입니다. 하지만 그 결과는 모두의 것입니다.",
        "지금의 선택은 오랫동안 기억될 것입니다.",
        "나는 모든 준비를 마쳤습니다. 이제 당신의 차례입니다."
    };

    public void SetIndex(int index)
    {
        nameText.text = names[index];
        infoText.text = infos[index];

        AssignRandomTexts(stage1Buttons, stage1Choices);
        AssignRandomTexts(stage2Buttons, stage2Choices);

        stage3Buttons.Clear();
        foreach (Transform parent in finalStageParents)
        {
            if (parent == null) continue;
            Button[] found = parent.GetComponentsInChildren<Button>(true);
            stage3Buttons.AddRange(found);
        }

        AssignRandomTexts(stage3Buttons.ToArray(), stage3Choices);
        AssignHoverEffect(stage1Buttons);
        AssignHoverEffect(stage2Buttons);
        AssignHoverEffect(stage3Buttons.ToArray());
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    void AssignRandomTexts(Button[] buttons, string[] texts)
    {
        List<string> availableTexts = new List<string>(texts);
        Shuffle(availableTexts);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (i >= availableTexts.Count) break;

            Button btn = buttons[i];
            StartCoroutine(ApplyTextAndStyleWithActivation(btn, availableTexts[i]));
        }
    }

    IEnumerator ApplyTextAndStyleWithActivation(Button btn, string text)
    {
        bool wasActive = btn.gameObject.activeSelf;

        if (!wasActive)
            btn.gameObject.SetActive(true);

        yield return null;
        yield return null;

        TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null)
        {
            tmp.text = text;
            tmp.ForceMeshUpdate();
        }
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
        }
        if (!wasActive)
            btn.gameObject.SetActive(false);
    }
    void AssignHoverEffect(Button[] buttons)
    {
        foreach (Button btn in buttons)
        {
            if (btn.GetComponent<HoverTextGlow>() == null)
            {
                HoverTextGlow glow = btn.gameObject.AddComponent<HoverTextGlow>();
                glow.tmpText = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }
    }
}
