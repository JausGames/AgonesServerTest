using UnityEngine;

[System.Serializable]
public class EnemySoundManager :  HitableSoundManager
{
    [SerializeField] AudioClip clipPreAttack;
    [SerializeField] AudioClip clipAttack;
    [SerializeField] AudioClip clipDetection;

    public void PlayAttack()
    {
        audioSource.PlayOneShot(clipAttack);
    }
    public void PlayPreAttack()
    {
        audioSource.PlayOneShot(clipPreAttack);
    }
    public void PlayDetection()
    {
        audioSource.PlayOneShot(clipDetection);
    }
}


