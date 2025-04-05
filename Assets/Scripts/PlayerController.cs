using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float mouseSpeed = 10f;
    public float moveSpeed = 10f;

    public GameObject verticalPivot;
    public GameObject horizontalPivot;
    public Rigidbody playerBody;

    InputAction jumpAction;
    InputAction lookAction;
    InputAction moveAction;

    float rotationHorizontal;
    float rotationVertical;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        moveAction = InputSystem.actions.FindAction("Move");

        rotationVertical = verticalPivot.transform.eulerAngles.x;
        rotationHorizontal = horizontalPivot.transform.eulerAngles.y;
    }

    void Update()
    {
        var lookValue = lookAction.ReadValue<Vector2>();
        var rotationDelta = mouseSpeed * Time.deltaTime * lookValue;

        rotationHorizontal += rotationDelta.x;
        rotationVertical = Mathf.Clamp(rotationVertical - rotationDelta.y, -80, 80);


        horizontalPivot.transform.localRotation = Quaternion.Euler(0, rotationHorizontal, 0);
        verticalPivot.transform.localRotation = Quaternion.Euler(rotationVertical, 0, 0);

        var moveVelocity = moveAction.ReadValue<Vector2>() * moveSpeed;
        var yVelocity = playerBody.linearVelocity.y;

        var moveVelocity3d = playerBody.transform.forward * moveVelocity.y
            + playerBody.transform.right * moveVelocity.x
            + playerBody.transform.up * yVelocity;

        playerBody.linearVelocity = moveVelocity3d;
    }
}
