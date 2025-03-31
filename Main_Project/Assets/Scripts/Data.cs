using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//돈, 날짜 등 데이터 저장

public class Data : MonoBehaviour
{
    public int money;

    public int month;
    public int date;

    //선수 구분
    public int playerCount;

    //선수 능력치
    public List<int> attack;
    public List<int> defense;

    //선수 훈련 상승
    public List<int> attackTraining;
    public List<int> defenseTraining;
}
