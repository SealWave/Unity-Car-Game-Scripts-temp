using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float turnSensitivity = 1f;
    public float accelerationSensitivity = 1f;

    public Vector2 GetVehicleInput()
    {
        float verticalInput = Input.GetAxis("Vertical"); // W/S or up/down arrow keys
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D or left/right arrow keys

        return new Vector2(horizontalInput * turnSensitivity, verticalInput * accelerationSensitivity);
    }
}
