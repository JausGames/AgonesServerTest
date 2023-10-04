using UnityEngine;

[System.Serializable]
public class PlayerSoundManager : HitableSoundManager
{
    [SerializeField] AudioClip clipPowerUp;

    public void PlayPowerUp()
    {
        audioSource.PlayOneShot(clipPowerUp);
    }
}