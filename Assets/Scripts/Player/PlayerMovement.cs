using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float normalMoveSpeed = 5f;
    [SerializeField] private float fasterMoveSpeed = 10f;
    private float stamina = 100f; // Example stamina variable
    private float staminaCostPerSecond = 10f; // Example stamina cost per second when sprinting
    private float staminaRegeneratedPerSecond = 5f; // Example stamina regenerated per second when not sprinting
    private float moveSpeed;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() // update stamina, move, UI 
    {
        if (Input.GetKey(KeyCode.Space) && stamina > 0)
        {
            //check if player is moving
            if (rb.linearVelocity.magnitude > 0)
            {
                stamina -= staminaCostPerSecond * Time.deltaTime; // Decrease stamina while sprinting
                UpdateText.Instance.UpdateStamina(stamina); // Update stamina UI
            }

            if (stamina < 0) stamina = 0; // Prevent negative stamina
            moveSpeed = fasterMoveSpeed;
        }
        else
        {
            moveSpeed = normalMoveSpeed; // Reset to default speed

            if (stamina < 100f) // Regenerate stamina when not sprinting
            {
                stamina += Time.deltaTime * staminaRegeneratedPerSecond; // Regenerate stamina
                UpdateText.Instance.UpdateStamina(stamina); // Update stamina UI
                if (stamina > 100f) stamina = 100f; // Cap stamina at 100
            }
        }
        Move();
    }

    private void Move()
    {
        Vector2 input = InputManager.Instance.moveDirection;
        rb.linearVelocity = input * moveSpeed;
    }

}
