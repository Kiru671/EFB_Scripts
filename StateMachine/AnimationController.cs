using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Animator animator;
    public Animator wingAnimator;
    public ParticleSystem leftTurbine;
    public ParticleSystem rightTurbine;
    public ParticleSystem leftWing;
    public ParticleSystem rightWing;
    private PlayerStateMachine playerState;
    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        playerState = gameObject.GetComponent<PlayerStateMachine>();
    }

    // Update is called once per frame
    void Update()
    {

        if (playerState.currentState is MoveGliding)
        {
            if (!leftTurbine.isPlaying) leftTurbine.Play();
            if (!rightTurbine.isPlaying) rightTurbine.Play();
            //if (!leftWing.isPlaying) leftWing.Play();
            //if (!rightWing.isPlaying) rightWing.Play();
        }
        else
        {
            leftTurbine.Stop();
            rightTurbine.Stop();
            //leftWing.Stop();
            //rightWing.Stop();
        }

        wingAnimator.SetBool("Gliding", playerState.currentState is MoveGliding);
        animator.SetBool("IsGrounded", playerState.currentState is MoveGrounded);
        animator.SetBool("IsJumping", playerState.currentState is MoveJumping);
        animator.SetBool("IsGliding", playerState.currentState is MoveGliding);
        animator.SetBool("IsFalling", playerState.currentState is MoveFalling);
        animator.SetBool("IsRunning", playerState.moveDelta > 0.00001f && playerState.currentState is MoveGrounded);
        animator.SetFloat("Velocity", Mathf.Clamp01(playerState.accel * 1.5f), 0.1f, Time.deltaTime);
        animator.SetFloat("Glide Lean", playerState.glideLean);
    }
}
