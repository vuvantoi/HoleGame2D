using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float stamina = 100f;
    [SerializeField] private float staminaDrainRate = 30f;
    [SerializeField] private float staminaRegenRate = 10f;

    private float currentSpeed;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        HandleSprinting();
        Move();
    }

    private void Move()
    {
        Vector2 input = InputManager.Instance.moveDirection;
        rb.linearVelocity = input * currentSpeed;
    }

    private void HandleSprinting()
    {
        bool isSprinting = (Input.GetKey(KeyCode.Space) || UpdateText.Instance.isSprintButtonPressed) && stamina > 0f;
        bool isMoving = rb.linearVelocity.sqrMagnitude > 0.01f;

        if (isSprinting && isMoving)
        {
            currentSpeed = sprintSpeed;
            stamina -= staminaDrainRate * Time.deltaTime;
        }
        else
        {
            currentSpeed = walkSpeed;
            if (stamina < maxStamina)
                stamina += staminaRegenRate * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        UpdateText.UpdateStamina(stamina);
    }
}
