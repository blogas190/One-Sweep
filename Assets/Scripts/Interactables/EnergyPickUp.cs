using UnityEngine;

public class EnergyPickUp : MonoBehaviour
{
    public float pickUpEnergy = 50f;
    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            AirTricks player = other.GetComponent<AirTricks>();
            EnergyController energy = other.GetComponent<EnergyController>();
            if(player != null)
            {
                energy.AddEnergy(pickUpEnergy);
                player.LeftTrickFeedback.PlayFeedbacks();
                Destroy(gameObject);
            }
        }
    }
}
