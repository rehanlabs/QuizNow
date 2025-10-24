using UnityEngine;
using System.Collections.Generic;
using Ami.BroAudio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop;

        [HideInInspector] public AudioSource source;
    }

    [Header("Audio Clips")]
    public List<Sound> musicSounds;  // Music sounds list
    public List<Sound> sfxSounds;    // SFX sounds list

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        InitializeSounds(musicSounds, musicVolume);
        InitializeSounds(sfxSounds, sfxVolume);
    }

    private void InitializeSounds(List<Sound> sounds, float volumeMultiplier)
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume * volumeMultiplier;
            s.source.loop = s.loop;
        }
    }

    public void PlayMusic(string name)
    {
        Sound s = musicSounds.Find(sound => sound.name == name);
        if (s == null)
        {
            return;
        }
        s.source.Play();
    }

    public void PlaySFX(string name)
    {
        Sound s = sfxSounds.Find(sound => sound.name == name);
        if (s == null)
        {
            return;
        }
        s.source.Play();
    }

    public void PlaySFXIfNotPlaying(string name)
    {
        Sound s = sfxSounds.Find(sound => sound.name == name);
        if (s == null)
        {
            return;
        }

        // Play only if not already playing
        if (!s.source.isPlaying)
        {
            s.source.Play();
        }
    }

    public void StopMusic(string name)
    {
        Sound s = musicSounds.Find(sound => sound.name == name);
        if (s != null)
        {
            s.source.Stop();
        }
    }

    public void StopSFX(string name)
    {
        Sound s = sfxSounds.Find(sound => sound.name == name);
        if (s != null)
        {
            s.source.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        RefreshVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        RefreshVolumes();
    }

    public void RefreshVolumes()
    {
        foreach (Sound s in musicSounds)
        {
            s.source.volume = s.volume * musicVolume;
        }

        foreach (Sound s in sfxSounds)
        {
            s.source.volume = s.volume * sfxVolume;
        }
    }
}