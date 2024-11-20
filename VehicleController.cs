using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    [SerializeField] private float baseMaxSpeed = 100f;
    [SerializeField] private float baseAcceleration = 20f;
    [SerializeField] private float baseBrakeForce = 30f;
    [SerializeField] private float baseHandling = 10f;
    [SerializeField] private float maxHealth = 100f;

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

        Vector2 moveInput = inputManager.GetVehicleInput();
        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;

        isAccelerating = verticalInput > 0;
        isBraking = verticalInput < 0;

        if (isAccelerating)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, currentMaxSpeed, currentAcceleration * Time.deltaTime * inputManager.accelerationSensitivity);
        }
        else if (isBraking)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, -currentMaxSpeed, currentAcceleration * Time.deltaTime * inputManager.accelerationSensitivity);
        }
        else
        {
            ApplyDeceleration();
        }

        float turnAmount = horizontalInput * currentHandling;
        if (Mathf.Abs(turnAmount) > 0.01f)
        {
            rb.AddTorque(0f, turnAmount * Time.deltaTime * 10f * inputManager.turnSensitivity, 0f, ForceMode.VelocityChange);
        }

        Vector3 moveDirection = transform.forward * currentSpeed;
        rb.velocity = moveDirection;
    }

    private void ApplyDeceleration()
    {
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

        moduleManager.GetActiveModules().TryGetValue(moduleName, out var module);
        if (module != null)
        {
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

        float healthPercentage = currentHealth / maxHealth;
        UpdateStatsBasedOnHealth(healthPercentage);

        if (currentHealth <= 0f)
        {
            HandleVehicleDestruction();
        }
    }

    private void ApplyModuleEffects(object module)
    {
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

        enabled = false;
    }
}
