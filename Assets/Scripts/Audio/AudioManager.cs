using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AudioID { UISelect, Hit, Capture, Obtain }

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource sfxPlayer;

    [SerializeField] List<AudioData> sfxList;
    private Dictionary<AudioID, AudioData> sfxLookup;

    [SerializeField] float fadeDuration;

    private AudioClip currMusic;

    private float ogMusicVolume;



    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    private void Start()
    {
        ogMusicVolume = musicPlayer.volume;

        sfxLookup = sfxList.ToDictionary(x => x.id);
    }

    public void PlayMusic(AudioClip clip, bool loop = true, bool fade = false)
    {
        if (clip == null || clip == currMusic) { return; }

        currMusic = clip;

        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    private IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        if (fade) { yield return musicPlayer.DOFade(0f, fadeDuration).WaitForCompletion(); }

        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();

        if (fade) { yield return musicPlayer.DOFade(ogMusicVolume, fadeDuration).WaitForCompletion(); }
    }

    public void PlaySFX(AudioClip clip, bool pause = false)
    {
        if (clip == null) { return; }

        if (pause) 
        { 
            musicPlayer.Pause();
            StartCoroutine(UnPauseMusic(clip.length));
        }

        sfxPlayer.PlayOneShot(clip);
    }

    public void PlaySFX(AudioID id, bool pause = false)
    {
        if (!sfxLookup.ContainsKey(id)) { return; }

        var audioData = sfxLookup[id];
        PlaySFX(audioData.clip, pause);
    }

    private IEnumerator UnPauseMusic(float delay)
    {
        yield return new WaitForSeconds(delay);

        musicPlayer.volume = 0f;
        musicPlayer.UnPause();
        musicPlayer.DOFade(ogMusicVolume, fadeDuration);
    }
}

[Serializable]
public class AudioData
{
    public AudioID id;
    public AudioClip clip;
}
