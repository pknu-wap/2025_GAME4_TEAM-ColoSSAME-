using UnityEngine;
using UnityEngine.UI;

public class PrefabManager : MonoBehaviour
{
    [Header("Parent object holding the buttons")]
    public GameObject buttonsParent;

    [Header("List of buttons (connect in Inspector)")]
    public Button[] buttons;

    [Header("Prefab list (connect prefab assets in Inspector)")]
    public GameObject[] prefabPrefabs;

    private GameObject[] prefabInstances;

    private void Start()
    {
        prefabInstances = new GameObject[prefabPrefabs.Length];

        // 각 버튼에 OnClick 이벤트 등록
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;  // 람다 캡처용 지역 변수
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    public void OnButtonClick(int index)
    {
        buttonsParent.SetActive(false);

        if (prefabInstances[index] == null)
        {
            prefabInstances[index] = Instantiate(prefabPrefabs[index], transform);

            // prefab 안 ReturnButton에 PrefabManager 연결
            ReturnButton returnButton = prefabInstances[index].GetComponentInChildren<ReturnButton>();
            if (returnButton != null)
            {
                returnButton.manager = this;
                returnButton.prefabIndex = index;
            }
        }

        prefabInstances[index].SetActive(true);
    }

    public void ReturnToButtons(int index)
    {
        if (prefabInstances[index] != null)
        {
            prefabInstances[index].SetActive(false);  // Destroy ❌, 꺼두기만
        }

        buttonsParent.SetActive(true);
    }
}