using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }

    [Header("Audio Manager Configuration")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource effectsSource;

    [Header("Scene Change Sound Effect")]
    [SerializeField] private AudioClip sceneChangeSoundEffect;

    private Coroutine currentFade;
    private readonly float fadeDuration = 1f;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        PlayerPrefs.DeleteKey("MusicVolume");
        PlayerPrefs.DeleteKey("EffectsVolume");
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("EffectsVolume", 1f);
        SetMusicVolume(music);
        SetSFXVolume(sfx);
    }

    public void SetMusicVolume(float value) {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value) {
        audioMixer.SetFloat("EffectsVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("EffectsVolume", value);
    }

    public void PlayMusic(AudioClip clip, bool loop = true) {
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeMusic(clip, loop));
    }

    public void StopMusic() {
        musicSource.Stop();
    }

    public void PlayClip(AudioClip audioClip) {
        if (audioClip != null) effectsSource.PlayOneShot(audioClip);
    }

    public void PlaySceneChangeSoundEffect() {
        float volume = 0.1f;
        effectsSource.PlayOneShot(sceneChangeSoundEffect, volume);
    }

    public AudioSource PlayLoopingSoundEffect(AudioClip clip, Transform followTarget) {
        if (clip == null) return null;
        GameObject audioObj = new GameObject("TempLoopingSound");
        audioObj.transform.SetParent(followTarget);
        audioObj.transform.localPosition = Vector3.zero;
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f;
        audioSource.outputAudioMixerGroup = effectsSource.outputAudioMixerGroup;
        audioSource.volume = effectsSource.volume;
        audioSource.Play();
        return audioSource;
    }

    private IEnumerator FadeMusic(AudioClip newClip, bool loop) {
        float startVolume = musicSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime) {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }
        musicSource.volume = 0f;

        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime) {
            musicSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }
        musicSource.volume = startVolume;
    }
}