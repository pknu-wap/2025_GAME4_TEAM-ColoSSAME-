using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickSoundManager : MonoBehaviour
{
public static ClickSoundManager Instance;

    public AudioSource audioSource;
    public AudioClip clickClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PlayClick();
        }
    }

    public void PlayClick()
    {
        if (audioSource != null && clickClip != null)
        {
            audioSource.PlayOneShot(clickClip);
        }
        else
        {
            Debug.LogWarning("AudioSource 또는 Clip이 연결되지 않음");
        }
    }
}
