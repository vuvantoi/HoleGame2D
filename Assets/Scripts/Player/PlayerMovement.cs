using UnityEngine;
// encapsulation, modularization, 
// con gà: biết bay, script nhảy cao, script đi theo ng chơi
// vịt: biết bơi, script bơi dưới nước, script đi theo ng chơi
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float normalMoveSpeed = 5f;
    [SerializeField] private float fasterMoveSpeed = 10f;
    [SerializeField] private float stamina = 100f; // Example stamina variable
    [SerializeField] private float staminaCostPerSecond = 30f; // Example stamina cost per second when sprinting
    [SerializeField] private float staminaRegeneratedPerSecond = 10f; // Example stamina regenerated per second when not sprinting
    private float moveSpeed;
    private Rigidbody2D rb;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() // update stamina, move, UI 
    {
        RunFast();
        Move();
    }

    private void Move()
    {
        Vector2 input = InputManager.Instance.moveDirection;
        rb.linearVelocity = input * moveSpeed;
    }

    public void RunFast()
    {
        // This method can be used to trigger fast running logic if needed
        // Currently, it does nothing but can be expanded later
        if (Input.GetKey(KeyCode.Space) && stamina > 0)
        {
            //check if player is moving
            if (rb.linearVelocity.magnitude > 0)
            {
                stamina -= staminaCostPerSecond * Time.deltaTime; // Decrease stamina while sprinting
                UpdateText.UpdateStamina(stamina); // Update stamina UI
            }

            if (stamina < 0) stamina = 0; // Prevent negative stamina
            moveSpeed = fasterMoveSpeed;
        }
        else if (UpdateText.Instance.isSprintButtonPressed && stamina > 0) //check if ui button is pressed
        {
            // You can handle sprint logic here if needed
            // For example, set moveSpeed = fasterMoveSpeed;
            moveSpeed = fasterMoveSpeed;
            if (rb.linearVelocity.magnitude > 0)
            {
                stamina -= staminaCostPerSecond * Time.deltaTime; // Decrease stamina while sprinting
                UpdateText.UpdateStamina(stamina); // Update stamina UI
            }
            if (stamina < 0) stamina = 0; // Prevent negative stamina
        }

        else
        {
            moveSpeed = normalMoveSpeed; // Reset to default speed

            if (stamina < 100f) // Regenerate stamina when not sprinting
            {
                stamina += Time.deltaTime * staminaRegeneratedPerSecond; // Regenerate stamina
                UpdateText.UpdateStamina(stamina); // Update stamina UI
                if (stamina > 100f) stamina = 100f; // Cap stamina at 100
            }
        }
    }

}
