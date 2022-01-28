using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager:MonoBehaviour
{
    public static AudioManager audioInstance = null;

    public static AudioManager AudioInstance
    {
        get
        {
            if (!audioInstance)
            {
                audioInstance = FindObjectOfType(typeof(AudioManager)) as AudioManager;
            }
            return audioInstance;
        }
    }

    public AudioSource _audioSource = null;

    private void Awake()
    {
        if (audioInstance == null)
        {
            audioInstance = this;
        }
        else if (audioInstance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        
    }

}
