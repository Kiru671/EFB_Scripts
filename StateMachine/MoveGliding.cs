using System.Collections;
using UnityEngine;

public class MoveGliding : MovementBaseState
{
    private PlayerStateMachine stateManager;
    private Transform charTransform;
    public Animator wingAnimator;

    private float vel;
    private Vector3 collisionVel;

    Vector3 boost;

    Vector3 sideMovement;

    Vector3 thrust;
    Vector3 lastMovement;

    Quaternion originalRotation;

    Quaternion glideRotation;

    Transform rigDeform;

    private float initalThrust = 0.2f;
    float simulatedThrustMultiplier;
    float xRotation;
    float yRotation;
    float yaw;
    float glideRoll;

    float lastYRotation;

    private bool thrustDone;
    private bool autoRotating;
    private bool leaveGlide;
    private bool boostOnCooldown;
    private bool boostDone = true;
    public override void EnterState(PlayerStateMachine context)
    {
        
        stateManager = GameObject.Find("SugeyA").GetComponent<PlayerStateMachine>();
        charTransform = GameObject.Find("SugeyA").GetComponent<Transform>();
        rigDeform = GameObject.Find("rig_deform").GetComponent<Transform>();

        //Save rotation to set again while leaving gliding state.
        originalRotation = Quaternion.Euler(charTransform.eulerAngles.x,0,charTransform.eulerAngles.z);

        Debug.Log("Entering Gliding State");
        stateManager.glideCam.enabled = true;
        thrustDone = false;
        stateManager.glideLean = 0;
        yRotation = charTransform.localEulerAngles.y;
    }

    public override void UpdateState(PlayerStateMachine context, Vector2 move, Vector2 MouseMove)
    {
        
        if (InputManagerScript.Instance.isGrounded)
        {
            context.ChangeState(context.GroundedState);
        }
        else if (!InputManagerScript.Instance.glideInitiated)
        {
            InputManagerScript.Instance.boost.Disable();

            //Keep momentum for the next state
            stateManager.momentum = thrust;

            //Set saved rotation
            charTransform.rotation = originalRotation;
            context.ChangeState(context.FallingState);       
        }
    }

    public override void FixedUpdateState(PlayerStateMachine context, Vector2 move, Vector2 MouseMove)
    {
        HandleMovement(context, move, MouseMove);
    }

    public override void HandleMovement(PlayerStateMachine context, Vector2 move, Vector2 MouseMove)
    {

        ManageRotation(MouseMove);
        
        thrust = stateManager.transform.forward * stateManager.glideSpeed;

        simulatedThrustMultiplier += RayAngleCos() * stateManager.glideSpeed * Time.fixedDeltaTime;
        simulatedThrustMultiplier = Mathf.Clamp(simulatedThrustMultiplier, 0, stateManager.maxThrustSpeed);
        

        //Boost
        if (!thrustDone)
        {
            thrust *= initalThrust;
            thrustDone = true;
        }
        else if (!InputManagerScript.Instance.boosted)
        {
            boostOnCooldown = false;
        }

        if (InputManagerScript.Instance.boosted && !boostOnCooldown)
        {
            boostDone = false;
            boostOnCooldown = true;   

            stateManager.StartCoroutine(Boost(stateManager.boostDuration));
        }
        else
        {
            thrust *= simulatedThrustMultiplier;
        }

        if (!boostDone)
        {
            boost = stateManager.transform.forward * stateManager.boostSpeed;
            thrust += boost;
        }
        stateManager.transform.Translate(thrust, Space.World);

        lastMovement = thrust;
    }
    private void ManageRotation(Vector2 MouseMove)
    {
        xRotation += MouseMove.y * stateManager.pitchSpeed;
        stateManager.glideLean += MouseMove.x;
        stateManager.glideLean = Mathf.Clamp(stateManager.glideLean, -1, 1);
        yRotation += MouseMove.x * stateManager.yawSpeed;

        xRotation = Mathf.Clamp(xRotation, -85f, 80f);          

        Vector3 rightLeft = Vector3.Lerp(stateManager.camLeft.transform.TransformDirection(Vector3.up), stateManager.camRight.transform.TransformDirection(Vector3.up), 0.5f);
        Vector3 upDown = Vector3.Lerp(stateManager.camUp.transform.TransformDirection(Vector3.forward), stateManager.camDown.transform.TransformDirection(Vector3.forward), 0.5f);

        stateManager.transform.forward = Vector3.Lerp(stateManager.transform.forward, rightLeft + upDown, 0.5f);

        //Handle pitch rotation at slow velocities

        stateManager.glideCam.transform.localRotation = Quaternion.Slerp(stateManager.glideCam.transform.localRotation, Quaternion.Euler(xRotation, yRotation, 0), 0.45f);

        if (MouseMove.y < 0)
        {
            stateManager.glideCam.transform.localRotation = Quaternion.Slerp(stateManager.glideCam.transform.localRotation, Quaternion.Euler(xRotation, yRotation, 0), 0.03f);
        }
        else if (simulatedThrustMultiplier <= 0.25f)
        {
            //xRotation = Mathf.Lerp(xRotation, -xRotation , 0.1f * Time.fixedDeltaTime);
            xRotation = Mathf.LerpAngle(xRotation, 30f, 0.5f * Time.fixedDeltaTime);
        }

        lastYRotation = yRotation;
    }
    private float RayAngleCos()
    {  
        float angle = Mathf.Abs(Vector3.SignedAngle(Vector3.up, stateManager.transform.forward, stateManager.transform.up));
        return -Mathf.Cos(angle * Mathf.PI / 180.0f);
    }
    private float movementDelta()
    {
        return vel = (thrust - lastMovement).magnitude / Time.deltaTime;
    }
    public IEnumerator Boost(float boostDuration)
    {
        yield return new WaitForSeconds(boostDuration);
        boostDone = true;
        Debug.Log("DONE");
    }
}
