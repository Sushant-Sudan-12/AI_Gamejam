using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float normalSpeed = 5f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.3f;

    [Header("Look Settings")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    private CharacterController controller;
    private float verticalRotation = 0f;

    private bool isHoldingRightClick = false;
    private bool isDashing = false;
    private float dashTimer = 0f;
    public float shootingDistance = 2f;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    public UnityEngine.UI.Slider healthBar; // Make sure to drag your UI Slider into this!

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        currentHealth = maxHealth;
        UpdateHealthBar();

    }

    void Update()
    {
        HandleLook();
        HandleInput();
        HandleMovement();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) // Right Click down
        {
            isHoldingRightClick = true;
        }

        if (Input.GetMouseButtonUp(0)) // Right Click released
        {
            if (isHoldingRightClick)
            {
                ShootShotgun(); // Fire shotgun on release
                isHoldingRightClick = false;
            }
        }

        if (isHoldingRightClick && Input.GetMouseButtonDown(1)) // Dash trigger
        {
            StartDash();
        }
    }

    void HandleMovement()
    {
        float currentSpeed = normalSpeed;

        if (isHoldingRightClick)
        {
            if (isDashing)
            {
                currentSpeed = dashSpeed;
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0f)
                {
                    isDashing = false;
                }
            }

            Vector3 move = cameraTransform.forward;
            move.y = 0f;
            move.Normalize();
            controller.Move(move * currentSpeed * Time.deltaTime);
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
    }

    void ShootShotgun()
    {
        Debug.Log("Shotgun Fired!");

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, shootingDistance)) // 100 units range
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                Debug.Log("Enemy Hit! ");
                Destroy(hit.collider.gameObject); // Destroy the enemy
            }
            else
            {
                Debug.Log("Hit something else: " + hit.collider.name);
            }
        }
        else
        {
            Debug.Log("Missed!");
        }
    }
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        Debug.Log("☠️ Player Died!");
        // Here you can reload scene, show death screen, etc.
    }


}
