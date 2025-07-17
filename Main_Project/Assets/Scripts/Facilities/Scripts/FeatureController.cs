using UnityEngine;

public class FeatureController : MonoBehaviour
{
    public GameObject[] upgradeStages;  // 각 단계별 오브젝트들
    private int currentStage = 0;

    private void Start()
    {
        UpdateStage();
    }

    public void Upgrade()
    {
        if (currentStage < upgradeStages.Length - 1)
        {
            upgradeStages[currentStage].SetActive(false);
            currentStage++;
            upgradeStages[currentStage].SetActive(true);
            Debug.Log($"Upgraded to stage {currentStage}");
        }
        else
        {
            Debug.Log("Already at max upgrade.");
        }
    }

    private void UpdateStage()
    {
        for (int i = 0; i < upgradeStages.Length; i++)
        {
            upgradeStages[i].SetActive(i == currentStage);
        }
    }
}