using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MoveFalling : MovementBaseState
{
    private PlayerStateMachine stateManager;
    private Transform charTransform;
    public Animator wingAnimator;

    RaycastHit previousHit;
    RaycastHit hitGrounded;

    private bool fallingGrounded;

    float hitPointDelta;
    float lastHitPoint;

    float skinWidth = 0.015f;
    float maxBounces = 5;
    float maxSlopeAngle = 85f;
    float scale;

    Bounds bounds;

    CapsuleCollider capsuleCollider;


    public override void EnterState(PlayerStateMachine context)
    {
        stateManager = GameObject.Find("SugeyA").GetComponent<PlayerStateMachine>();
        charTransform = stateManager.transform.Find("SugeyA");
        Debug.Log("Entering Falling State");
        capsuleCollider = stateManager.gameObject.GetComponent<CapsuleCollider>();
        bounds = capsuleCollider.bounds;
        stateManager.StartCoroutine(AirFriction());
        Debug.Log(stateManager.momentum);
        //MAKE FALLCAM
    }

    public override void UpdateState(PlayerStateMachine context, Vector2 move, Vector2 MouseMove)
    {
        bounds = capsuleCollider.bounds;
        bounds.Expand(-2 * skinWidth);
        if (fallingGrounded)
        {
            Debug.Log("Exiting Falling State");
            stateManager.glideCam.enabled = false;
            context.ChangeState(context.GroundedState);
        }
        else if (InputManagerScript.Instance.glideInitiated)
        {
            context.ChangeState(context.GlidingState);
        }
    }

    public override void FixedUpdateState(PlayerStateMachine context, Vector2 move, Vector2 MouseMove)
    {
        fallingGrounded = GroundCheck(context);
        HandleMovement(context, move, MouseMove);              
    }

    public override void HandleMovement(PlayerStateMachine context, Vector2 move, Vector2 MouseMove)
    {

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camRight.y = 0;
        camForward.y = 0;

        Vector3 camRelativeVertical = move.y * camForward;

        Vector3 camRelativeHorizontal = move.x * camRight;


        Vector3 movement = (camRelativeHorizontal + camRelativeVertical) * context.fallMoveSpeed;

        movement = CollideAndSlide(movement + 100 * stateManager.momentum, stateManager.transform.position, 0, stateManager.gravityPass, movement);





        //Gravity
        Vector3 gravity = Physics.gravity;
        Vector3 velocity = Vector3.zero;

        // Calculate the new velocity using the gravity and acceleration
        velocity += gravity * stateManager.fallingGravity * Time.fixedDeltaTime;

        // Limit the fall speed to the maximum
        //velocity.y = Mathf.Clamp(velocity.y, 1f, 100f);

        // Update the object's position using the calculateddw velocity
        //stateManager.transform.position += velocity * Time.fixedDeltaTime;

        context.transform.Translate((velocity + movement) * Time.fixedDeltaTime, Space.World);
        //context.transform.Translate((movement) * Time.fixedDeltaTime, Space.World);


        //Stop rotation from resetting when Vector is equal to zero (Not moving)
        if (movement.magnitude > 0)
        {
            stateManager.transform.rotation = Quaternion.Lerp(stateManager.transform.rotation, Quaternion.LookRotation(movement), 0.15f);
        }
    }
    /*protected override void GroundCheck(PlayerStateMachine context)
    {
        RaycastHit hit;

        if (Physics.Raycast(context.transform.position, -context.transform.up, out hit, context.defaultGroundcheckDistance))
        {
            if (hit.collider == previousHit.collider)
            {
                //hitPointDelta = Mathf.Abs(hit.point.y - lastHitPoint);
                //stateManager.transform.position = new Vector3(stateManager.transform.position.x, stateManager.transform.position.y + hitPointDelta, stateManager.transform.position.z);
            }           
            fallingGrounded = true;
            lastHitPoint = hit.point.y;
        }
        else
            fallingGrounded = false;
    }*/
    protected override bool GroundCheck(PlayerStateMachine context)
    {

        
        if (Physics.CapsuleCast(new Vector3(stateManager.transform.position.x, stateManager.transform.position.y + 0.15f, stateManager.transform.position.z), new Vector3(stateManager.transform.position.x, stateManager.transform.position.y + 1.6f, stateManager.transform.position.z), bounds.extents.x ,Vector3.down, out stateManager.hit, context.defaultGroundcheckDistance))
        {
            if (hitGrounded.collider == previousHit.collider)
            {
                //hitPointDelta = Mathf.Abs(hit.point.y - lastHitPoint);
                //stateManager.transform.position = new Vector3(charPos.x, charPos.y + hitPointDelta, charPos.z);
            }
            return true;
            lastHitPoint = hitGrounded.point.y;
        }
        else
            return false;
    }
    private Vector3 CollideAndSlide(Vector3 moveDir, Vector3 pos, int depth, bool gravityPass, Vector3 velInit)
    {
        if (depth >= maxBounces)
        {
            return Vector3.zero;
        }
        float dist = moveDir.magnitude + skinWidth;
        RaycastHit hit;
        LayerMask layermask = LayerMask.GetMask("Default");
        if (Physics.CapsuleCast(new Vector3(stateManager.transform.position.x, stateManager.transform.position.y + 0.25f, stateManager.transform.position.z), new Vector3(stateManager.transform.position.x, stateManager.transform.position.y + 1.6f, stateManager.transform.position.z), bounds.extents.x, moveDir.normalized, out hit, dist, layermask))
        {           
            Vector3 snapToSurface = moveDir.normalized * (stateManager.hit.distance - skinWidth);
            Vector3 leftover = moveDir - snapToSurface;
            float angle = Vector3.Angle(Vector3.up, stateManager.hit.normal);
            leftover = ProjectAndScale(leftover, stateManager.hit.normal);

            if (snapToSurface.magnitude <= skinWidth)
            {
                snapToSurface = Vector3.zero;
            }
            if (angle <= maxSlopeAngle)
            {
                Debug.Log("On Slope");
                if (gravityPass)
                {
                    return snapToSurface;
                }
                leftover = ProjectAndScale(leftover, stateManager.hit.normal);
            }
            // wall or steep surface
            else
            {
                scale = 1 - Vector3.Dot(new Vector3(hit.normal.x, 0, hit.normal.z).normalized, -new Vector3(velInit.x, 0, velInit.z).normalized);
                //scale = Mathf.Clamp(scale, 0.01f, 1);
                leftover = ProjectAndScale(leftover, stateManager.hit.normal) * scale;
                if (fallingGrounded && !gravityPass)
                {
                    leftover = ProjectAndScale(new Vector3(leftover.x, 0, leftover.z).normalized, -new Vector3(hit.normal.y, 0, hit.normal.z).normalized);
                    leftover *= scale;
                }
                else
                {
                    leftover = ProjectAndScale(leftover, stateManager.hit.normal) * scale;
                }
            }
            return snapToSurface + CollideAndSlide(leftover, pos + snapToSurface, depth + 1, stateManager.gravityPass, velInit);
        }

        return moveDir;
    }
    private Vector3 ProjectAndScale(Vector3 vec, Vector3 normal)
    {
        float magnitude = vec.magnitude;
        vec = Vector3.ProjectOnPlane(vec, normal).normalized;
        return vec *= magnitude;
    }
    public IEnumerator AirFriction()
    {
        while (stateManager.momentum.magnitude > 0.2f)
        {
            stateManager.momentum *= 0.5f;
        }
        yield return new WaitForSeconds(0.05f);
    }
}

