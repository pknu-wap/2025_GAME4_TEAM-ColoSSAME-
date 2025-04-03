using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class State : MonoBehaviour
{
    public TextMeshProUGUI stateText;//공격력, 방어력 텍스트

    public int fighterCount;//어떤 선수 선택했는지
    public int day;//몇 일에 훈련 선택


    public List<int> attack;//공격력
    public List<int> defense;//방어력


    public List<int> attackTraining;//공격력 훈련 시 얼마나 추가 될지
    public List<int> defenseTraining;//방어력 훈련 시 얼마나 추가 될지

    /*[SerializeField]private List<int> attack;
    [SerializeField]private List<int> defense;
    // Start is called before the first frame update

    public void attackTraining(int index, int attackAmonunt)
    {
        attack[index] += attackAmount;
    }

    public void defenseTraining(int index, int defenseAmount)
    {
        defense[index] += defenseAmount;
    }
    void Start()
    {
        
    }*/

    void Update()
    {
        stateText.text = $"공격력 : {attack[fighterCount]} + <color=#00ffff>{attackTraining.Sum()}</color>  방어력{defense[fighterCount]} + <color=#00ffff>{defenseTraining.Sum()}</color>";//공격력, 방어력 표시
    }

    public void fighter1()//어떤 선수를 골랐는지
    {
        fighterCount = 0;
    }

    public void fighter2()
    {
        fighterCount = 1;
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
    }

    public void defenseClick()//방어 훈련 클릭
    {
        attackTraining[day] = 0;
        defenseTraining[day] = 3;
    }

    public void nextWeek()//다음주로 넘김
    {
        attack[fighterCount] += attackTraining.Sum();//주를 넘겨 공격력 더해줌
        defense[fighterCount] += defenseTraining.Sum();//주를 넘겨 방어력 더해줌
        attackTraining = attackTraining.ConvertAll(x => 0);//리스트 안 값 다시 0으로 변경
        defenseTraining = defenseTraining.ConvertAll(x => 0);//리스트 안 값 다시 0으로 변경
    }
}
