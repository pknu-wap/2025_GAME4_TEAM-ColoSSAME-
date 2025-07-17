using UnityEngine;

public class ReturnButton : MonoBehaviour
{
    public PrefabManager manager;
    public int prefabIndex;

    public void OnReturnClick()
    {
        if (manager != null)
        {
            manager.ReturnToButtons(prefabIndex);
        }
    }
}