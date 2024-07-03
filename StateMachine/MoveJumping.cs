using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveJumping : MovementBaseState
{
    
    
    private float gravityForce = 0.1f;

    private float jumpStateTime;

    private PlayerStateMachine stateManager;
    private Transform charTransform;

    private Rigidbody rb;

    private bool isGrounded;

    public override void EnterState(PlayerStateMachine context)
    {       
        stateManager = GameObject.Find("SugeyA").GetComponent<PlayerStateMachine>();
        rb = stateManager.GetComponent<Rigidbody>();
        charTransform = stateManager.transform.Find("SugeyA");
        Debug.Log("Entering Jumping State");
        jumpStateTime = 0;
    }

    public override void UpdateState(PlayerStateMachine context, Vector2 move, Vector2 MouseMove)
    {
        HandleMovement(context, move, Vector2.zero);
        jumpStateTime += Time.deltaTime;
        if (InputManagerScript.Instance.isGrounded)
        {
            context.ChangeState(context.GroundedState);
        }
        else if (InputManagerScript.Instance.glideInitiated)
        {
            context.ChangeState(context.GlidingState);
        }
        else if(!stateManager.isJumping)
        {
            context.ChangeState(context.FallingState);
        }
    }

    public override void FixedUpdateState(PlayerStateMachine context, Vector2 move, Vector2 MouseMove)
    {

        /*if (!InputManagerScript.Instance.isGrounded &! stateManager.isJumping) 
        {
            float gravity = Mathf.Sin(Mathf.PI * jumpStateTime * gravityForce);
            //stateManager.transform.Translate(Vector3.down * gravity, Space.Self);
        }*/
    }

    public override void HandleMovement(PlayerStateMachine context, Vector2 move, Vector2 MouseMove)
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camRight.y = 0;
        camForward.y = 0;

        Vector3 camRelativeVertical = move.y * camForward;

        Vector3 camRelativeHorizontal = move.x * camRight;


        Vector3 movement = camRelativeHorizontal + camRelativeVertical;

        context.transform.Translate(movement * context.jumpSpeedAir * Time.deltaTime, Space.World);



        //Stop rotation from resetting when Vector is equal to zero (Not moving)
        if (movement.magnitude > 0)
        {
            stateManager.transform.rotation = Quaternion.Lerp(stateManager.transform.rotation, Quaternion.LookRotation(movement), 0.15f);
        }
    }
}
