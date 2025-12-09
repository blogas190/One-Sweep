using UnityEngine;

public class EnergyController : MonoBehaviour
{
    public float maxEnergy = 100f;
    public float currentEnergy;

    [Header("Trick Consumption Settings")]
    public float upTrickEnergy = 25f;
    public float downTrickEnergy = 25f;
    public float rightTrickEnergy = 50f;
    public float dashEnergy = 50f;
    [Header("Energy Recovery Settings")]
    public float leftTrickEnergy = 50f;

    void Start()
    {
        
    }

    public void SetEnergy(float amount)
    {
        currentEnergy = amount;
    }

    public void AddEnergy(float amount)
    {
        currentEnergy += amount;
        
        if(currentEnergy > maxEnergy)
        {
            currentEnergy = maxEnergy;
        }
    }

    public void RemoveEnergy(float amount)
    {
        currentEnergy -= amount;

        if(currentEnergy < 0)
        {
            currentEnergy = 0;
        }
    }

    public float EnergyPercentage()
    {
        return currentEnergy / maxEnergy * 100f;
    }
    
}
