using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    public Texture2D cursorTexture;
    public Vector2 hotSpot = Vector2.zero;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ApplyCursor();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ApplyCursor()
    {
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
        }
        else
        {
            Debug.LogWarning("Cursor Texture가 설정되지 않았습니다!");
        }
    }
}