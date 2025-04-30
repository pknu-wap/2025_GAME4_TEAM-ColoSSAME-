using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class State : MonoBehaviour
{
    public TextMeshProUGUI stateText;//공격력, 방어력 텍스트

    public int fighterCount;//어떤 선수 선택했는지
    public int day;//몇 일에 훈련 선택

    public List<Button> attackButtons;//공격 버튼 지정
    public List<Button> defenseButtons;//방어 버튼 지정

    public List<Button> fighterButtons;//선수 지정

    public List<int> attack;//공격력
    public List<int> defense;//방어력

    public List<int> attackTraining;//공격력 훈련 시 얼마나 추가 될지
    public List<int> defenseTraining;//방어력 훈련 시 얼마나 추가 될지

    public List<int> attackTrainingSave;//공격력 훈련 양 저장
    public List<int> defenseTrainingSave;//방어력 훈련 양 저장


    void Update()
    {
        stateText.text = $"공격력 : {attack[fighterCount]} + <color=#00ffff>{attackTrainingSave[fighterCount]}</color>  방어력{defense[fighterCount]} + <color=#00ffff>{defenseTrainingSave[fighterCount]}</color>";//공격력, 방어력 표시
    }

    public void fighter1()//어떤 선수를 골랐는지
    {
        fighterCount = 0;
        fighterButtons[fighterCount].transform.position = new Vector3(70f, 450f, 0f);
    }
    public void fighter2()
    {
        fighterCount = 1;
        fighterButtons[fighterCount].transform.position = new Vector3(70f, 450f, 0f);
    }
    public void fighter3()
    {
        fighterCount = 2;
        fighterButtons[fighterCount].transform.position = new Vector3(70f, 450f, 0f);
    }
    public void fighter4()
    {
        fighterCount = 3;
        fighterButtons[fighterCount].transform.position = new Vector3(70f, 450f, 0f);
    }
    public void fighter5()
    {
        fighterCount = 4;
        fighterButtons[fighterCount].transform.position = new Vector3(70f, 450f, 0f);
    }

    public void week1()//1일차 훈련
    {
        day = 0;
    }

    public void week2()//2일차 훈련
    {
        day = 1;
    }

    public void week3()//3일차 훈련
    {
        day = 2;
    }

    public void week4()//4일차 훈련
    {
        day = 3;
    }

    public void week5()//5일차 훈련
    {
        day = 4;
    }

    public void week6()//6일차 훈련
    {
        day = 5;
    }

    public void week7()//7일차 훈련
    {
        day = 6;
    }

    public void attackClick()//공격 훈련 클릭
    {
        attackTraining[day] = 3;
        defenseTraining[day] = 0;
        attackTrainingSave[fighterCount]=attackTraining.Sum();//공격 훈련 양 저장
        defenseTrainingSave[fighterCount]=defenseTraining.Sum();//방어 훈련 양 저장
        
        //공격 버튼 클릭 시 버튼색 변경
        Color gray = new Color(143f / 255f, 143f / 255f, 143f / 255f);
        ColorBlock grayBlock = new ColorBlock //회색 컬러 블럭
        {
            normalColor = gray, //회색으로
            highlightedColor = gray,
            pressedColor = gray,
            selectedColor = gray,
            disabledColor = Color.gray,
            colorMultiplier = 1f,
            fadeDuration = 0.1f

        };
        //공격 버튼 회색
        attackButtons[day].colors = grayBlock;

        //방어 버튼 색 원래대로
        Color white = Color.white;
        ColorBlock whiteBlock = new ColorBlock//흰색 컬러 블럭
        {
            normalColor = white,//흰색으로
            highlightedColor = white,
            pressedColor = white,
            selectedColor = white,
            disabledColor = Color.gray,
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };
        defenseButtons[day].colors = whiteBlock;//방어 흰색으로 변경
    }

    public void defenseClick()//방어 훈련 클릭
    {
        attackTraining[day] = 0;
        defenseTraining[day] = 3;
        attackTrainingSave[fighterCount]=attackTraining.Sum();//공격 훈련 양 저장
        defenseTrainingSave[fighterCount]=defenseTraining.Sum();//방어 훈련 양 저장
        //방어 버튼 색 변경
        Color gray = new Color(143f / 255f, 143f / 255f, 143f / 255f);
        ColorBlock grayBlock = new ColorBlock //회색 컬러 블럭
        {
            normalColor = gray, //회색으로
            highlightedColor = gray,
            pressedColor = gray,
            selectedColor = gray,
            disabledColor = Color.gray,
            colorMultiplier = 1f,
            fadeDuration = 0.1f

        };
        //방어 버튼 회색
        defenseButtons[day].colors = grayBlock;
        //공격 버튼 색 원래대로
        Color white = Color.white;
        ColorBlock whiteBlock = new ColorBlock//흰색 컬러 블럭
        {
            normalColor = white,//흰색으로
            highlightedColor = white,
            pressedColor = white,
            selectedColor = white,
            disabledColor = Color.gray,
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };
        attackButtons[day].colors = whiteBlock;//방어 흰색으로 변경
    }

    public void trainBack()
    {
        attackTraining = attackTraining.ConvertAll(x => 0);//리스트 안 값 다시 0으로 변경
        defenseTraining = defenseTraining.ConvertAll(x => 0);//리스트 안 값 다시 0으로 변경
        fighterButtons[0].transform.position = new Vector3(336f, 356f, 0f);
        fighterButtons[1].transform.position = new Vector3(436f, 356f, 0f);
        fighterButtons[2].transform.position = new Vector3(336f, 256f, 0f);
        fighterButtons[3].transform.position = new Vector3(436f, 256f, 0f);
        fighterButtons[4].transform.position = new Vector3(336f, 156f, 0f);
    }

    public void nextWeek()//다음주로 넘김
    {
        for (int i = 0; i < attack.Count; i ++)
        {
            attack[i] += attackTrainingSave[i];//주를 넘겨 공격력 더해줌
            defense[i] += defenseTrainingSave[i];//주를 넘겨 방어력 더해줌
            attackTrainingSave[i]=0;//공격 훈련값 초기화
            defenseTrainingSave[i]=0;//방어 훈련값 초기화
            attackTraining = attackTraining.ConvertAll(x => 0);//리스트 안 값 다시 0으로 변경
            defenseTraining = defenseTraining.ConvertAll(x => 0);//리스트 안 값 다시 0으로 변경
        }
    }
}
