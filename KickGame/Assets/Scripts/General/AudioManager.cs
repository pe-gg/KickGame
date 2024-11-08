using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [SerializeField] AudioClip[] sfxclips;
    public AudioClip bgm;

    // Start is called before the first frame update
    void Start()
    {
        musicSource.clip = bgm;
        musicSource.Play();
        musicSource.loop = true;
    }
    
    public void PlayVFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
