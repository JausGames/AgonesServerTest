using System;
using UnityEngine;

[System.Serializable]
public class HitableSoundManager : MonoBehaviour
{
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] AudioClip clipHurt;
    [SerializeField] AudioClip clipDie;

    public void PlayHurt()
    {
        audioSource.PlayOneShot(clipHurt);
    }
    public void PlayDie()
    {
        audioSource.PlayOneShot(clipDie);
    }

    internal void PlayClip(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
