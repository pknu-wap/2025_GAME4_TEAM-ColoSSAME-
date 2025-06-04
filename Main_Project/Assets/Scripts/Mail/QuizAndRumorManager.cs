using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuizAndRumorManager : MonoBehaviour
{
    [Header("퀴즈 관련")]
    public TextMeshProUGUI quizText;
    public Button[] quizButtons;
    public GameObject resultO;
    public GameObject resultX;

    [Header("루머 관련")]
    public TextMeshProUGUI rumorText;

    [System.Serializable]
    public class Quiz
    {
        public string question;
        public string[] options;
        public int correctIndex;
    }

    public List<Quiz> quizList = new List<Quiz>();
    public List<string> rumors = new List<string>();
    
    private Quiz currentQuiz;

    public Button bt;
    void Start()
    {
        InitializeQuiz();
        ShowRandomRumor();
    }
    
    void InitializeQuiz()
    {
        int rand = Random.Range(0, quizList.Count);
        currentQuiz = quizList[rand];

        quizText.text = currentQuiz.question;

        resultO.SetActive(false);
        resultX.SetActive(false);

        for (int i = 0; i < quizButtons.Length; i++)
        {
            int index = i; // 캡처 주의
            if (i < currentQuiz.options.Length)
            {
                quizButtons[i].gameObject.SetActive(true);
                quizButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuiz.options[i];
                quizButtons[i].onClick.RemoveAllListeners();
                quizButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
            }
            else
            {
                quizButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnAnswerSelected(int index)
    {
        resultO.SetActive(false);
        resultX.SetActive(false);

        if (index == currentQuiz.correctIndex)
        {
            resultO.SetActive(true);
        }
        else
        {
            resultX.SetActive(true);
        }
    }

    void ShowRandomRumor()
    {
        if (rumors.Count == 0) return;
        int rand = Random.Range(0, rumors.Count);
        rumorText.text = rumors[rand];
    }
}
