using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharAnimator : MonoBehaviour
{
    
    private Animator animator;

    /// <summary>
    /// 오브젝트 애니메이터 참조, 처음 상태 지정 
    /// </summary>
    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("isDeath",false);
    }
    
    public void Move()
    {
        animator.SetBool("1_Move",true);
    }

    public void Attack()
    {
        animator.SetTrigger("2_Attack");
    }

    public void Idle()
    {
        animator.SetBool("1_Move",false);
    }

    public void Death()
    {
        animator.SetTrigger("4_Death");
        animator.SetBool("isDeath",true);
    }
}
