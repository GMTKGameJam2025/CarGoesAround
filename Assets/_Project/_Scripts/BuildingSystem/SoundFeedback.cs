using UnityEngine;

/// <summary>
/// Plays one-shot audio clips in response to building system events.
/// Clips are assigned in the Inspector; missing clips are silently skipped.
/// </summary>
public class SoundFeedback : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip removeSound;
    [SerializeField] private AudioClip wrongPlacementSound;

    [SerializeField] private AudioSource audioSource;

    /// <summary>Plays the clip that corresponds to <paramref name="soundType"/>.</summary>
    public void PlaySound(SoundType soundType)
    {
        AudioClip clip = soundType switch
        {
            SoundType.Click          => clickSound,
            SoundType.Place          => placeSound,
            SoundType.Remove         => removeSound,
            SoundType.wrongPlacement => wrongPlacementSound,
            _                        => null,
        };

        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}
