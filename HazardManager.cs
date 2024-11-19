using UnityEngine;

public class HazardManager : MonoBehaviour
{
    public float hazardDamage = 20f;  // The damage this hazard deals

    // This function is called when a vehicle collides with this hazard
    public void CheckHazardCollision(Collider vehicleCollider)
    {
        // Check if the colliding object has the VehicleController component
        VehicleController vehicle = vehicleCollider.GetComponent<VehicleController>();
        if (vehicle != null)
        {
            // If the vehicle is found, apply damage to it
            vehicle.ApplyDamage(hazardDamage);

            // Optionally, add visual or audio feedback here, such as playing a sound
            Debug.Log("Vehicle hit a hazard and took " + hazardDamage + " damage!");
        }
    }
}
