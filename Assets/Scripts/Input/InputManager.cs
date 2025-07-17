using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : SingletonBase<InputManager>
{
    [SerializeField] private FixedJoystick joystick;

    public Vector2 moveDirection {get; private set;}

    private void Update()
    {
        ReadInput();
    }

    private void ReadInput()
    {
        if (joystick != null && HasJoystickInput())
        {
            moveDirection = new Vector2(joystick.Horizontal, joystick.Vertical);
        }
        else
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            moveDirection = new Vector2(horizontal, vertical).normalized;
        }
    }

    private bool HasJoystickInput()
    {
        return Mathf.Abs(joystick.Horizontal) > 0.01f || Mathf.Abs(joystick.Vertical) > 0.01f;
    }
}
