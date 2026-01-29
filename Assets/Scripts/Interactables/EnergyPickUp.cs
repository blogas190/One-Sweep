using UnityEngine;
using MoreMountains.Feedbacks;

public class EnergyPickUp : MonoBehaviour
{
    public MMFeedbacks energyPickUpFeedback;
    public float pickUpEnergy = 50f;
    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            EnergyController energy = other.GetComponent<EnergyController>();
            if(energy != null)
            {
                energy.AddEnergy(pickUpEnergy);
                energyPickUpFeedback.PlayFeedbacks();
                Destroy(gameObject);
            }
        }
    }
}
