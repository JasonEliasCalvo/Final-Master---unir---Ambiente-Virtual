using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialTableController : MonoBehaviour
{
    public Animator animator;
    public Rigidbody rb;

    public void OnAnimation()
    {
        animator.SetBool("IsSelected", true);   
        StartCoroutine(EndAnimation(true));
    }

    public IEnumerator EndAnimation(bool state)
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        rb.isKinematic = state;
    }

    public void OffAnimation()
    {
        rb.isKinematic = true;
        animator.SetBool("IsSelected", false);
        StartCoroutine(EndAnimation(true));
    }
}
