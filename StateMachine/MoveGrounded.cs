using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MoveGrounded : MovementBaseState
{
    [SerializeField] private float groundSpeed = 12f;
    private PlayerStateMachine stateManager;
    private Transform charTransform;
    private CapsuleCollider capsuleCollider;

    
    private float jumpDuration = 0.4f;
    private float groundCheckDistance;

    private bool initiatedJump;
    private bool grounded;
    private bool colliding;

    private bool[] raysHit;


    int maxBounces = 5;
    float skinWidth = 0.015f;
    float maxSlopeAngle = 80;

    float vel;
    float accel;
    float scale;
    float angle;

    float hitPointDelta;
    float lastHitPoint;

    Bounds bounds;

    Vector3 movement;
    Vector3 lastMovement;
    Vector3 downSlopeDir;

    Vector3 charPos;

    RaycastHit previousHit;
    RaycastHit hitGrounded;
    public override void EnterState(PlayerStateMachine context)
    {
        stateManager = GameObject.Find("SugeyA").GetComponent<PlayerStateMachine>();
        stateManager.momentum = Vector3.zero;
        capsuleCollider = stateManager.gameObject.GetComponent<CapsuleCollider>();
        bounds = capsuleCollider.bounds;      
        charTransform = stateManager.transform.Find("SugeyA");
        Debug.Log("Entering Grounded State");
        movement = Vector3.zero;
        //Subscribe to events
        InputManagerScript.Instance.playerInputs.Player.Jump.performed += Jump;
    }

    public override void UpdateState(PlayerStateMachine context, Vector2 move, Vector2 MouseMove)
    {


        bounds = capsuleCollider.bounds;
        bounds.Expand(-2 * skinWidth);
        grounded = GroundCheck(context);



        HandleMovement(context, move, Vector2.zero);
        

        if (initiatedJump &! InputManagerScript.Instance.isGrounded)
        {
            InputManagerScript.Instance.playerInputs.Player.Jump.performed -= Jump;
            context.ChangeState(context.JumpingState);
        }
        else if(!initiatedJump &! grounded)
        {
            context.ChangeState(context.FallingState);
            InputManagerScript.Instance.playerInputs.Player.Jump.performed -= Jump;
        }
    }
    public override void FixedUpdateState(PlayerStateMachine context, Vector2 move, Vector2 MouseMove)
    {

    }

    public override void HandleMovement(PlayerStateMachine context, Vector2 move, Vector2 MouseMove)
    {

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camRight.y = 0;
        camForward.y = 0;

        Vector3 camRelativeVertical = move.y * camForward;

        Vector3 camRelativeHorizontal = move.x * camRight;     

        movement = camRelativeHorizontal + camRelativeVertical;


        //Stop rotation from resetting when Vector is equal to zero (Not moving)
        if (movement.magnitude > 0)
        {
            stateManager.transform.rotation = Quaternion.Lerp(stateManager.transform.rotation, Quaternion.LookRotation(movement), 0.15f);
        }

        //Debug.Log(movementDelta());
        stateManager.accel = Acceleration();
        stateManager.moveDelta = movementDelta();

        movement = CollideAndSlide(movement, stateManager.transform.position, 0, stateManager.gravityPass, movement);

        context.transform.Translate((movement + downSlopeDir) * context.speed * Time.deltaTime, Space.World);
        

        movement = lastMovement;
        //Debug.Log(stateManager.accel);
        //Collision bounce
    }
    private void Jump(InputAction.CallbackContext context)
    {
        initiatedJump = true;
        stateManager.StartCoroutine(JumpCoroutine());
    }
    public IEnumerator JumpCoroutine()
    {
        stateManager.isJumping = true;

        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration)
        {
            float jumpHeight = Mathf.Sin(Mathf.PI * (elapsedTime / jumpDuration)) * stateManager.jumpForce;
            stateManager.transform.Translate(Vector3.up * jumpHeight * Time.deltaTime, Space.World);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        initiatedJump = false;
        stateManager.isJumping = false;
        stateManager.StopCoroutine(JumpCoroutine());
    }
    protected override bool GroundCheck(PlayerStateMachine context)
    {
        Debug.DrawLine(new Vector3(stateManager.transform.position.x, stateManager.transform.position.y + 0.5f, stateManager.transform.position.z), stateManager.hit.point, Color.green);
        if (Physics.Raycast(new Vector3(stateManager.transform.position.x, stateManager.transform.position.y + 0.30f, stateManager.transform.position.z), Vector3.down, out stateManager.hit, context.defaultGroundcheckDistance))
        {
            
            return true;
        }
        else
            return false;
    }
    private Vector3 CollideAndSlide(Vector3 moveDir, Vector3 pos, int depth, bool gravityPass, Vector3 velInit)
    {  
        if(depth >= maxBounces)
        {
            return Vector3.zero;
        }     
        float dist = moveDir.magnitude + skinWidth;
        RaycastHit hit;
        Debug.DrawRay(pos, moveDir.normalized);
        LayerMask layermask = LayerMask.GetMask("Default");
        Vector3 downSlopeCastPos = new Vector3(stateManager.transform.position.x, stateManager.transform.position.y + 3f, stateManager.transform.position.z) + (stateManager.transform.TransformVector(movement.normalized) * groundSpeed);
        RaycastHit slopeHit;

        if (Physics.CapsuleCast(new Vector3(stateManager.transform.position.x, stateManager.transform.position.y + 0.15f, stateManager.transform.position.z), new Vector3(stateManager.transform.position.x, stateManager.transform.position.y + 1.6f, stateManager.transform.position.z), bounds.extents.x, moveDir.normalized, out hit, dist, layermask))
        {
            Vector3 snapToSurface = moveDir.normalized * (hit.distance - skinWidth);
            Vector3 leftover = moveDir - snapToSurface;
            angle = Vector3.Angle(Vector3.up, hit.normal);
            Debug.Log(angle);
            Debug.DrawLine(new Vector3(stateManager.transform.position.x, stateManager.transform.position.y + 1.6f, stateManager.transform.position.z), hit.point, Color.red, 3f);

            if (snapToSurface.magnitude <= skinWidth)
            {
                snapToSurface = Vector3.zero;
            }
            
            else
            {
                downSlopeDir = Vector3.zero;
            }
            if (angle <= maxSlopeAngle)
            {
                Debug.Log("Slope");
                /// Gravity check with another spherecast and correct downwards slope movement with vector. Make slide state.
                /// 
               
                if (gravityPass)
                {
                    return snapToSurface;
                }
                leftover = ProjectAndScale(leftover, hit.normal);
                Debug.DrawRay(stateManager.transform.position, leftover, Color.red, 10f);
            }
            // wall or steep surface
            else
            {
                
                Debug.LogAssertion("WALL");
                scale = 1 - Vector3.Dot(new Vector3(hit.normal.x, 0, hit.normal.z).normalized, -new Vector3(velInit.x, 0, velInit.z).normalized);
                //scale = Mathf.Clamp(scale, 0.01f, 1);
                leftover = ProjectAndScale(leftover, hit.normal) * scale;
                if (grounded && !gravityPass)
                {
                    leftover = ProjectAndScale(new Vector3(leftover.x, 0, leftover.z).normalized, -new Vector3(hit.normal.y, 0, hit.normal.z).normalized);
                    leftover *= scale;
                }
                else
                {
                    leftover = ProjectAndScale(leftover, hit.normal) * scale;
                }
            }
            return snapToSurface + CollideAndSlide(leftover, pos + snapToSurface, depth + 1, stateManager.gravityPass, velInit);
        }
        /*else
        {
            Vector3 snapToSurface = moveDir.normalized * (stateManager.hit.distance - skinWidth);
            Vector3 leftover = moveDir - snapToSurface;
            leftover = ProjectAndScale(leftover, stateManager.hit.normal);
        }
        *//*
        else if(Physics.Raycast(downSlopeCastPos, Vector3.down, out slopeHit, 4f, layermask))
        {
            Debug.Log("Down!");
            Vector3 downSlope = stateManager.transform.position - slopeHit.point;
            Vector3 snapToSurface = moveDir.normalized * (stateManager.hit.distance - skinWidth);
            Vector3 leftover = moveDir - snapToSurface;
            leftover = ProjectAndScale(leftover, slopeHit.normal);
        }*/
        return moveDir;
    }
    private Vector3 ProjectAndScale(Vector3 vec, Vector3 normal)
    {
        float magnitude = vec.magnitude;
        vec = Vector3.ProjectOnPlane(vec, normal).normalized;
        //Debug.DrawRay(stateManager.transform.position,vec,Color.yellow, 10f);
        return vec *= magnitude;
    }
    private float movementDelta()
    {
        return vel = (movement - lastMovement).magnitude * Time.deltaTime;
    }
    private float Acceleration()
    {     
        return accel = movementDelta() / Time.deltaTime;
    }
}
