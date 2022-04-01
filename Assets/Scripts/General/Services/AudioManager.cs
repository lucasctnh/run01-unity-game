using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
	public static AudioManager Instance;

	[SerializeField] private List<Sound> _sounds = new List<Sound>();

	[Tooltip("The tracks available to play sounds. By default, the 1st is for BGM and the 2nd is for SFX.")]
	[SerializeField] private List<AudioSource> _tracks = new List<AudioSource>();

	[Tooltip("Duration in seconds of the audio fade ins and outs.")]
	[SerializeField] private float _fadeTime = 1f;

	private void OnEnable() => GameManager.OnUpdateVolume += (track, volume) => ChangeTrackVolume(track, volume);

	private void OnDisable() => GameManager.OnUpdateVolume -= (track, volume) => ChangeTrackVolume(track, volume);

	private void Awake() {
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	public void PauseTrack(int trackNumber) {
		AudioSource audioSource = GetTrack(trackNumber);

		if (audioSource != null)
			audioSource.Pause();
	}

	public void ResumeTrack(int trackNumber) {
		AudioSource audioSource = GetTrack(trackNumber);

		if (audioSource != null)
			audioSource.UnPause();
	}

	public void StopTrack(int trackNumber) {
		AudioSource audioSource = GetTrack(trackNumber);

		if (audioSource != null)
			audioSource.Stop();
	}

	public void PauseAllTracks() {
		foreach (AudioSource track in FindObjectsOfType<AudioSource>())
			PauseTrack(track);
	}

	public void ResumeAllTracks() {
		foreach (AudioSource track in FindObjectsOfType<AudioSource>())
			ResumeTrack(track);
	}

	public void StopAllTracks() {
		foreach (AudioSource track in FindObjectsOfType<AudioSource>())
			StopTrack(track);
	}

	public void PlaySound(Sound.Type soundType, int trackNumber) {
		AudioSource audioSource = GetTrack(trackNumber);
		audioSource.clip = GetAudioClip(soundType);
		audioSource.loop = true;

		if (!audioSource.isPlaying)
			audioSource.Play();
	}

	public void PlaySoundOneShot(Sound.Type soundType, int trackNumber) {
		AudioSource audioSource = GetTrack(trackNumber);
		if (audioSource != null)
			audioSource.PlayOneShot(GetAudioClip(soundType));
	}

	public IEnumerator WaitPlaySoundOneShot(Sound.Type soundType, int trackNumber, Action action) {
		AudioSource audioSource = GetTrack(trackNumber);
		if (audioSource != null)
			audioSource.PlayOneShot(GetAudioClip(soundType));

		yield return new WaitWhile(() => audioSource.isPlaying);
		action();
	}

	public float GetTrackVolume(int trackNumber) {
		return GetTrack(trackNumber).volume;
	}

	public void ChangeSoundWithFade(Sound.Type soundType, int trackNumber) => StartCoroutine(StartChangeSoundWithFade(soundType, trackNumber, GetTrackVolume(trackNumber)));

	private AudioSource GetTrack(int trackNumber) {
		return _tracks[trackNumber - 1];
	}

	private AudioClip GetAudioClip(Sound.Type soundType) {
		foreach (Sound sound in _sounds) {
			if (sound.soundType == soundType)
				return sound.audioClip;
		}

		Debug.LogError("Sound " + soundType + " not found.");
		return null;
	}

	private void ChangeTrackVolume(int trackNumber, float volume) {
		AudioSource audioSource = GetTrack(trackNumber);
		if (audioSource != null)
			audioSource.volume = volume;
	}

	private void PauseTrack(AudioSource track) => track.Pause();

	private void ResumeTrack(AudioSource track) => track.UnPause();

	private void StopTrack(AudioSource track) => track.Stop();

	private IEnumerator StartChangeSoundWithFade(Sound.Type soundType, int trackNumber, float targetVolume) {
		AudioSource audioSource = GetTrack(trackNumber);

		yield return Fade(audioSource, 0);

		audioSource.clip = GetAudioClip(soundType);
		audioSource.loop = true;

		if (!audioSource.isPlaying)
			audioSource.Play();

		yield return Fade(audioSource, targetVolume);
	}

	private IEnumerator Fade(AudioSource track, float targetVolume) {
		float currentTime = 0;
		float start = track.volume;

		while (currentTime < _fadeTime) {
			currentTime += Time.deltaTime;
			track.volume = Mathf.Lerp(start, targetVolume, currentTime / _fadeTime);
			yield return null;
		}

		yield break;
	}
}
