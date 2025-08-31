using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderController : MonoBehaviour
{
    
    public static ShaderController Instance { get; private set; }
    
    private SpriteRenderer spriteRenderer;
    public Material cardOutlineMaterial;
    public Material bannerOutlineMaterial;
    public Material normalOutlineMaterial;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
    
}