using UnityEngine;
using UnityEngine.Events;

public class Switch : MonoBehaviour
{
    public UnityEvent onTriggered;
    public bool isWorking = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isWorking)
        {
            onTriggered.Invoke();
            isWorking = false;
        }
    }
}
