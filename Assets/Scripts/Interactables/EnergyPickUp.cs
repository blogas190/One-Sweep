using UnityEngine;

public class EnergyPickUp : MonoBehaviour
{
    public float pickUpEnergy = 50f;
    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            EnergyController energy = other.GetComponent<EnergyController>();
            if(energy != null)
            {
                energy.AddEnergy(pickUpEnergy);
                Destroy(gameObject);
            }
        }
    }
}
