using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManagerScript : MonoBehaviour
{
    public static InputManagerScript Instance;

    public GroundCheck groundCheck;
    public bool isGrounded;

    public InputManager playerInputs;
    public InputAction move;
    public InputAction mouseMove;
    public InputAction jump;
    public InputAction glide;
    public InputAction boost;

    public Vector2 moveDirection = Vector2.zero;
    public Vector2 mouseDirection = Vector2.zero;

    public bool jumpInitiated;
    public bool glideInitiated;
    public bool boosted;

    

    private void Awake()
    {
        playerInputs = new InputManager();
        Instance = this;
        //Might want to add destroy if already attached
    }
    private void OnEnable()
    {
        move = playerInputs.Player.Move;
        move.Enable();

        mouseMove = playerInputs.Player.GlideControl;


        jump = playerInputs.Player.Jump;
        jump.Enable();
        jump.performed += Jump;

        glide = playerInputs.Player.Glide;
        glide.Enable();
        glide.performed += Glide;

        boost = playerInputs.Player.Boost;
        boost.Enable();
        boost.performed += Boost;


    }
    private void OnDisable()
    {
        move.Disable();
        jump.Disable();
        glide.Disable();
        boost.Disable();
    }

    private void Update()
    {
        //isGrounded = groundCheck.isGrounded;

        moveDirection = move.ReadValue<Vector2>();
        mouseDirection = Mouse.current.delta.ReadValue()*Time.smoothDeltaTime;

    }
    private void Jump(InputAction.CallbackContext context)
    {


    }
    private void Glide(InputAction.CallbackContext context)
    {
        Debug.Log("Gliding");
        if(isGrounded)
        {
            glideInitiated = false;
        }
        else if (glideInitiated)
        {
            glideInitiated = false;
        }
        else
        {
            boost.Enable();
            glideInitiated = true;
        }
    }
    private void Boost(InputAction.CallbackContext context)
    {
        if(!boosted)
        {
            boosted = true;
            Debug.Log("Boost!");
            StartCoroutine(ResetAfterTime(boosted));
        }

    }
    private IEnumerator ResetAfterTime(bool x)
    {
        while(x)
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Reverted");
            x = false;
            boosted = false;
        }        
    }
}