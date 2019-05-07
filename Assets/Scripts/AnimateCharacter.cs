using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateCharacter : MonoBehaviour
{

    private Animator animator;

    public float speed = 0f;


    private void Awake()
    {

        animator = GetComponent<Animator>();

    }


    public void Animation()
    {

        Animation_Walk();

    }


    private void Animation_Walk()
    {

        animator.SetFloat("speed", speed);

    }
}
