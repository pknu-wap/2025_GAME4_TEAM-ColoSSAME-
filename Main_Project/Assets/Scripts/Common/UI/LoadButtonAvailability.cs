using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 저장 데이터가 없으면 불러오기 버튼을 비활성으로 둔다.
/// 확인하는 파일은 LoadManager.OnClickLoad와 같다.
/// </summary>
[RequireComponent(typeof(Button))]
public class LoadButtonAvailability : MonoBehaviour
{
    private static readonly string[] SaveFiles = { "LeagueSave.json", "UserSave.json" };

    private void OnEnable()
    {
        GetComponent<Button>().interactable = HasSaveData();
    }

    public static bool HasSaveData()
    {
        foreach (var file in SaveFiles)
        {
            if (!File.Exists(Path.Combine(Application.persistentDataPath, file))) return false;
        }
        return true;
    }
}
