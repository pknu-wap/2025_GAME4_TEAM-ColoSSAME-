using System.Collections;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;
    public static CoroutineRunner Instance {
        get {
            if (_instance == null) {
                var go = new GameObject("[CoroutineRunner]");
                _instance = go.AddComponent<CoroutineRunner>();
                // DontDestroyOnLoad(go); // 전역 유지가 필요하면 주석 해제
            }
            return _instance;
        }
    }

    public static Coroutine Run(IEnumerator routine) => Instance.StartCoroutine(routine);
}