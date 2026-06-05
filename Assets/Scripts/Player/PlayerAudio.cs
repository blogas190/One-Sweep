using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private AudioClip clip;

    private bool isPlaying = false;
    private PlayerMovement playerMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (playerMovement != null && AudioManager.Instance != null)
        {
            if (playerMovement.Grounded() && playerMovement.GetMagnitude() > 0.1 && playerMovement.IsOnRail() == false)
            {
                Debug.Log($"Playing footsteps. Grounded={playerMovement.Grounded()} Mag={playerMovement.GetMagnitude()} Rail={playerMovement.IsOnRail()}");
                AudioManager.Instance.PlaySFXLoop(clip);
                isPlaying = true;
            }
            else
            {
                if(isPlaying == true)
                {
                    AudioManager.Instance.StopSFXLoop(clip);
                    isPlaying = false;
                }
            }
        }
    }
}
