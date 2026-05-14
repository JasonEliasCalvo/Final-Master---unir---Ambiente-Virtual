using UnityEngine;

public class Footstep : MonoBehaviour
{
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;

    public void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;
        int index = Random.Range(0, footstepClips.Length);
        footstepSource.PlayOneShot(footstepClips[index]);
    }
}

