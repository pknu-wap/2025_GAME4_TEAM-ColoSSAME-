using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DepthSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Z 정렬 기준: Y축 위치 기준
        float newZ = transform.position.y * 0.01f;
        transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
    }
}