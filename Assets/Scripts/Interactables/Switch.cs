using UnityEngine;
using UnityEngine.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

public class Switch : MonoBehaviour
{
    public UnityEvent onTriggered;
    public bool isWorking = true;
    public MMFeedbacks ActivateFeedback;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isWorking)
        {
            onTriggered.Invoke();
            if (ActivateFeedback != null)
            {
                ActivateFeedback.PlayFeedbacks();
            }
            isWorking = false;
        }
    }
}
