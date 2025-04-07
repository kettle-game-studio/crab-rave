using System;
using System.Collections;
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
    public CapsuleCollider playerCollider;

    public GrapplingHook grapplingHook;
    public TrailLine trailLine;

    InputAction jumpAction;
    InputAction unjumpAction;
    InputAction lookAction;
    InputAction moveAction;
    InputAction fireAction;
    InputAction unfireAction;
    InputAction escapeAction;

    float rotationHorizontal;
    float rotationVertical;
    int worldBoundLayerMask;

    enum State
    {
        Free,
        Firing,
        Grappling,
        Returning,
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

    bool isMinimized = false;
    void MinimizeHitbox()
    {
        if (isMinimized) return;

        isMinimized = true;
        playerCollider.radius = 0.3f;
        playerCollider.height = 0.6f;
    }

    void MaximizeHitbox()
    {
        if (!isMinimized) return;

        isMinimized = false;
        playerCollider.radius = 0.5f;
        playerCollider.height = 2f;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        worldBoundLayerMask = 1 << LayerMask.NameToLayer("WorldBounds");

        Cursor.lockState = CursorLockMode.Locked;
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        unjumpAction = InputSystem.actions.FindAction("UnJump");
        moveAction = InputSystem.actions.FindAction("Move");
        fireAction = InputSystem.actions.FindAction("Attack");
        unfireAction = InputSystem.actions.FindAction("UnAttack");
        escapeAction = InputSystem.actions.FindAction("Escape");

        rotationVertical = verticalPivot.transform.eulerAngles.x;
        rotationHorizontal = horizontalPivot.transform.eulerAngles.y;
    }

    void Update()
    {
        if (escapeAction.IsPressed())
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (Cursor.lockState != CursorLockMode.Locked && !escapeAction.IsPressed())
        {
            if (fireAction.IsPressed() || unfireAction.IsPressed())
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            return;
        }

        if (state == State.Free)
        {
            trailLine.moveBackFlag = unfireAction.IsPressed();

            if (unfireAction.IsPressed())
            {
                MinimizeHitbox();
                var dir = trailLine.RollBackDirection();
                playerBody.linearVelocity = dir * 10;

                FreeLook();
                return;
            }

            MaximizeHitbox();

            if (fireAction.IsPressed())
            {
                state = State.Firing;
                trailLine.moveBackFlag = false;
                StartCoroutine(grapplingHook.FlyForward(this));
                // Debug.Log($"{state}");

                return;
            }

            FreeMovement();
            FreeLook();
        }
        else if (state == State.Grappling)
        {
            FreeLook();

            if (fireAction.IsPressed())
            {
                var thisPosition = playerBody.transform.position;
                var hookPosition = grapplingHook.TipPosition;

                var additionalForce = (thisPosition.y < hookPosition.y ? Vector3.up : -Vector3.up) * 5;

                MinimizeHitbox();
                playerBody.AddForce((hookPosition - thisPosition).normalized * grapplingForce + additionalForce);

                return;
            }

            MaximizeHitbox();

            if (unfireAction.IsPressed())
            {
                trailLine.moveBackFlag = false;
                state = State.Firing;
                StartCoroutine(grapplingHook.FlyBack(this));
                // Debug.Log($"{state}");

                return;
            }
        }
    }

    void FreeLook()
    {
        Debug.Log($"Cursor.lockState = {Cursor.lockState}");
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        var lookValue = lookAction.ReadValue<Vector2>();
        var rotationDelta = mouseSpeed * Time.deltaTime * lookValue;

        rotationHorizontal += rotationDelta.x;
        rotationVertical = Mathf.Clamp(rotationVertical - rotationDelta.y, -80, 80);


        horizontalPivot.transform.localRotation = Quaternion.Euler(0, rotationHorizontal, 0);
        verticalPivot.transform.localRotation = Quaternion.Euler(rotationVertical, 0, 0);
    }

    void FreeMovement()
    {
        var moveVelocity = moveAction.ReadValue<Vector2>();

        var jump = jumpAction.IsPressed() ? 1.0f : unjumpAction.IsPressed() ? -1.0f : 0;
        // 2d wasd
        // var moveVelocity3d = playerBody.transform.forward * moveVelocity.y
        //     + playerBody.transform.right * moveVelocity.x
        //     + playerBody.transform.up * yVelocity + playerBody.transform.up * jump;
        // 3d wasd
        var moveVelocity3d = verticalPivot.transform.forward * moveVelocity.y
                    + verticalPivot.transform.right * moveVelocity.x
                    + playerBody.transform.up * jump;

        playerBody.linearVelocity = Vector3.Lerp(playerBody.linearVelocity, moveVelocity3d * moveSpeed, Mathf.Clamp(Time.deltaTime * 5, 0, 1));
        var speed = playerBody.linearVelocity.magnitude;
        var direction = playerBody.linearVelocity.normalized;
        playerBody.linearVelocity = direction * Mathf.Min(speed, 5);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (state == State.Returning)
            return;

        if ((worldBoundLayerMask & (1 << collision.collider.gameObject.layer)) == 0)
            return;

        Debug.Log($"Cant due to {collision.collider.gameObject.name} ({collision.collider.gameObject.layer}), worldBoundLayerMask = {worldBoundLayerMask}, ");
        StartCoroutine(WorldEndCoroutine());
    }

    IEnumerator WorldEndCoroutine()
    {
        Debug.Log($"{state}");
        if (state == State.Grappling)
        {
            StartCoroutine(grapplingHook.FlyBack(this, 0.02f));
        }

        trailLine.moveBackFlag = true;
        state = State.Returning;

        MinimizeHitbox();

        for (var i = 0; i < 20; i++)
        {
            var dir = trailLine.RollBackDirection();
            playerBody.linearVelocity = dir * 10;

            FreeLook();
            yield return null;
        }


        trailLine.moveBackFlag = false;
        state = State.Free;
    }
}
