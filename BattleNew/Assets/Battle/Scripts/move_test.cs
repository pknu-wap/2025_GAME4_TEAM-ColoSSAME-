using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move_test : MonoBehaviour
{
    private Animator animator;
    public void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float movex = Input.GetAxis("Horizontal");
        float movey = Input.GetAxis("Vertical");
        
        Vector3 moveVector = new Vector3(movex, movey, 0f);
        
        transform.Translate(moveVector.normalized * Time.deltaTime*5f);
        if (movex != 0 || movey != 0)
        {
            animator.SetBool("1_Move", true);
        }
        else
        {
            animator.SetBool("1_Move", false);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            animator.SetTrigger("2_Attack");
        }
    }
}
