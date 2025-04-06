using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float mouseSpeed = 10f;
    public float moveSpeed = 10f;
    public float grapplingForce = 10f;

    public GameObject verticalPivot;
    public GameObject horizontalPivot;
    public Rigidbody playerBody;

    public GrapplingHook grapplingHook;

    InputAction jumpAction;
    InputAction lookAction;
    InputAction moveAction;
    InputAction fireAction;
    InputAction unfireAction;

    float rotationHorizontal;
    float rotationVertical;

    enum State
    {
        Free,
        Firing,
        Grappling,
    }

    State state = State.Free;

    public void GrappleOk()
    {
        state = State.Grappling;
        // Debug.Log($"{state}");
    }

    public void GrappleFailed()
    {
        state = State.Free;
        // Debug.Log($"{state}");
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        moveAction = InputSystem.actions.FindAction("Move");
        fireAction = InputSystem.actions.FindAction("Attack");
        unfireAction = InputSystem.actions.FindAction("UnAttack");

        rotationVertical = verticalPivot.transform.eulerAngles.x;
        rotationHorizontal = horizontalPivot.transform.eulerAngles.y;
    }

    void Update()
    {
        if (state == State.Free)
        {
            if (fireAction.IsPressed())
            {
                state = State.Firing;
                StartCoroutine(grapplingHook.FlyForward(this));
                Debug.Log($"{state}");

                return;
            }

            FreeMovement();
            FreeLook();
        }
        else if (state == State.Grappling)
        {
            if (unfireAction.IsPressed())
            {
                state = State.Firing;
                StartCoroutine(grapplingHook.FlyBack(this));
                // Debug.Log($"{state}");

                return;
            }
            else if (fireAction.IsPressed())
            {
                var thisPosition = playerBody.transform.position;
                var hookPosition = grapplingHook.TipPosition;

                var additionalForce = (thisPosition.y < hookPosition.y ? Vector3.up : -Vector3.up) * 5; 
                
                playerBody.AddForce((hookPosition - thisPosition).normalized * grapplingForce + additionalForce);
            }
            else
            {
                FreeLook();
            }
        }
    }

    void FreeLook()
    {
        var lookValue = lookAction.ReadValue<Vector2>();
        var rotationDelta = mouseSpeed * Time.deltaTime * lookValue;

        rotationHorizontal += rotationDelta.x;
        rotationVertical = Mathf.Clamp(rotationVertical - rotationDelta.y, -80, 80);


        horizontalPivot.transform.localRotation = Quaternion.Euler(0, rotationHorizontal, 0);
        verticalPivot.transform.localRotation = Quaternion.Euler(rotationVertical, 0, 0);
    }

    void FreeMovement()
    {
        var jump = jumpAction.IsPressed() ? 3.0f : 0;

        var moveVelocity = moveAction.ReadValue<Vector2>() * moveSpeed;
        var yVelocity = playerBody.linearVelocity.y;

        var moveVelocity3d = playerBody.transform.forward * moveVelocity.y
            + playerBody.transform.right * moveVelocity.x
            + playerBody.transform.up * yVelocity + playerBody.transform.up * jump;

        playerBody.linearVelocity = Vector3.Lerp(playerBody.linearVelocity, moveVelocity3d, Time.deltaTime);
    }
}
