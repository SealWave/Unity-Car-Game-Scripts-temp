using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    public float baseMaxSpeed = 100f;
    public float baseAcceleration = 20f;
    public float baseBrakeForce = 30f;
    public float baseHandling = 10f;
    public float maxHealth = 100f;
    public float currentDeceleration = 5f;
    public float currentSpeed;
    public float currentHealth;
    public bool isAccelerating;
    public bool isBraking;

    private Rigidbody rb;
    private InputManager inputManager;
    private ModuleManager moduleManager;
    private float currentMaxSpeed;
    private float currentAcceleration;
    private float currentBrakeForce;
    private float currentHandling;
    private Dictionary<string, float> activeModifierEffects;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        inputManager = FindObjectOfType<InputManager>();
        moduleManager = GetComponent<ModuleManager>();
        
        currentHealth = maxHealth;
        activeModifierEffects = new Dictionary<string, float>();
        
        ResetStats();
    }

 public void Update()
{
    if (!inputManager) return;

    // Get input values (X for turning, Y for acceleration/braking)
    Vector2 moveInput = inputManager.GetVehicleInput();

    // Apply input sensitivity
    float horizontalInput = moveInput.x;  // Turning input (left/right)
    float verticalInput = moveInput.y;    // Acceleration/braking input (forward/backward)

    isAccelerating = verticalInput > 0;
    isBraking = verticalInput < 0;

    // Apply acceleration (forward movement)
    if (isAccelerating)
    {
        // Move towards max speed (forward)
        currentSpeed = Mathf.MoveTowards(currentSpeed, currentMaxSpeed, currentAcceleration * Time.deltaTime * inputManager.accelerationSensitivity);
    }
    // Apply reverse movement (backward movement)
    else if (isBraking)
    {
        // Move towards negative max speed (backward)
        currentSpeed = Mathf.MoveTowards(currentSpeed, -currentMaxSpeed, currentAcceleration * Time.deltaTime * inputManager.accelerationSensitivity);
    }
    else
    {
        // Decelerate the vehicle when no input is given
        ApplyDeceleration();
    }

    // Apply turning based on input (X-axis from input)
    float turnAmount = horizontalInput * currentHandling;

    // Apply turning using Rigidbody's torque (rotation around the Y-axis)
    if (Mathf.Abs(turnAmount) > 0.01f)  // Apply torque only if there's significant input
    {
        // Use the turnSensitivity from the InputManager to adjust how sensitive the turning is
        rb.AddTorque(0f, turnAmount * Time.deltaTime * 10f * inputManager.turnSensitivity, 0f, ForceMode.VelocityChange); // Apply torque around Y-axis
    }

    // Apply forward/backward movement based on currentSpeed
    Vector3 moveDirection = transform.forward * currentSpeed;
    rb.velocity = moveDirection;
}

// Simulate deceleration when no input is given
private void ApplyDeceleration()
{
    // Decelerate more slowly based on the current speed
    if (currentSpeed > 0)
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, currentDeceleration * Time.deltaTime);
    }
    else if (currentSpeed < 0)
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, currentDeceleration * Time.deltaTime);
    }
}



    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hazard"))
        {
            HazardManager hazardManager = collision.gameObject.GetComponent<HazardManager>();
            if (hazardManager != null)
            {
                hazardManager.CheckHazardCollision(GetComponent<Collider>());
            }
        }
    }

    public void ActivateModule(string moduleName)
    {
        if (moduleManager == null) return;

        // Request module activation from ModuleManager
        moduleManager.GetActiveModules().TryGetValue(moduleName, out var module);
        if (module != null)
        {
            // Apply module effects to vehicle stats
            ApplyModuleEffects(module);
        }
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public void ApplyDamage(float damage)
    {
        currentHealth = Mathf.Max(0f, currentHealth - damage);

        // Reduce performance based on damage
        float healthPercentage = currentHealth / maxHealth;
        UpdateStatsBasedOnHealth(healthPercentage);

        if (currentHealth <= 0f)
        {
            HandleVehicleDestruction();
        }
    }

    private void ApplyModuleEffects(object module)
    {
        // This would be implemented based on the Module class structure
        // Example implementation:
        ResetStats();
        foreach (var effect in activeModifierEffects)
        {
            switch (effect.Key)
            {
                case "speed":
                    currentMaxSpeed *= effect.Value;
                    break;
                case "acceleration":
                    currentAcceleration *= effect.Value;
                    break;
                case "handling":
                    currentHandling *= effect.Value;
                    break;
                case "braking":
                    currentBrakeForce *= effect.Value;
                    break;
            }
        }
    }

    private void UpdateStatsBasedOnHealth(float healthPercentage)
    {
        currentMaxSpeed = baseMaxSpeed * healthPercentage;
        currentAcceleration = baseAcceleration * healthPercentage;
        currentBrakeForce = baseBrakeForce * healthPercentage;
        currentHandling = baseHandling * healthPercentage;
    }

    private void ResetStats()
    {
        currentMaxSpeed = baseMaxSpeed;
        currentAcceleration = baseAcceleration;
        currentBrakeForce = baseBrakeForce;
        currentHandling = baseHandling;
    }

    private void HandleVehicleDestruction()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.EndRace();
        }
        
        // Disable vehicle controls
        enabled = false;
        
        // Optional: Trigger destruction effects
        // Implement vehicle destruction visualization
    }
}