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

            if (cardOutlineMaterial) cardOutlineMaterial = new Material(cardOutlineMaterial);
            if (bannerOutlineMaterial) bannerOutlineMaterial = new Material(bannerOutlineMaterial);
            if (normalOutlineMaterial) normalOutlineMaterial = new Material(normalOutlineMaterial);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
}