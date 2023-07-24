using System;
using UnityEngine;


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public Sound[] sounds;


    private void Awake()
    {
        Instance = this;

        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.loop = sound.loop;
            sound.source.pitch = sound.pitch;
            sound.source.volume = sound.volume;
            sound.source.playOnAwake = sound.playOnAwake;
            sound.source.outputAudioMixerGroup = sound.output;
        }
    }

    private void Start() { Play("BGMusic"); }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }
}
