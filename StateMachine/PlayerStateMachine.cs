using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    [Range(10f,20f)]
    public float speed;
    public float maxThrustSpeed;
    public float minThrustSpeed;
    public float glideSpeed;
    [Range(0.0f, 100.0f)]
    public float fallingGravity = 53.9f;
    [Range(0.0f, 100.0f)]
    public float fallMoveSpeed = 3f;
    [Range(0.0f, 100.0f)]
    public float gravityForceGliding = 0.1f;
    [Range(0.0f, 1000.0f)]
    public float pitchSpeed = 1f;
    [Range(0.0f, 1000.0f)]
    public float yawSpeed = 1f;
    [Range(0.0f, 1f)]
    public float defaultGroundcheckDistance = 0.07f;
    [Range(0.2f, 3f)]
    public float boostSpeed = 1f;
    [Range(0.2f, 3f)]
    public float boostDuration = 1f;
    [Range(1f, 20f)]
    public float jumpSpeedAir = 6.5f;
    [Range(1f, 30f)]
    public float jumpForce = 15f;

    [HideInInspector] public Vector3 momentum;
    [HideInInspector] public float moveDelta;
    [HideInInspector] public float accel;
    [HideInInspector] public float glideLean;

    public Transform cam;
    public Transform camRight;
    public Transform camLeft;
    public Transform camUp;
    public Transform camDown;

    public CinemachineVirtualCamera glideCam;

    public bool isJumping = false;
    public bool gravityPass;

    public MovementBaseState currentState;
    public MoveGrounded GroundedState = new MoveGrounded();
    public MoveJumping JumpingState = new MoveJumping();
    public MoveGliding GlidingState = new MoveGliding();
    public MoveFalling FallingState = new MoveFalling();

    public Animator wingAnimator;

    public RaycastHit hit;
    public float rayOffset = 0f;

    void Start()
    {
        currentState = GroundedState;
        currentState.EnterState(this);

        glideCam.enabled = false;
    }

    void Update()
    {
        currentState.UpdateState(this, InputManagerScript.Instance.moveDirection, InputManagerScript.Instance.mouseDirection); 
    }

    void FixedUpdate()
    {
        currentState.FixedUpdateState(this, InputManagerScript.Instance.moveDirection, InputManagerScript.Instance.mouseDirection);
    }

    public void ChangeState(MovementBaseState state)
    {
        currentState= state;
        state.EnterState(this);
    }

}
