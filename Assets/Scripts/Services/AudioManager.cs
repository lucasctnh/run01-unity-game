using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
	public static AudioManager Instance;

	[SerializeField] private List<Sound> _sounds = new List<Sound>();

	[Tooltip("The tracks available to play sounds. By default, the 1st is for BGM and the 2nd is for SFX.")]
	[SerializeField] private List<AudioSource> _tracks = new List<AudioSource>();

	private void OnEnable() => GameManager.OnUpdateVolume += (track, volume) => ChangeTrackVolume(track, volume);

	private void OnDisable() => GameManager.OnUpdateVolume -= (track, volume) => ChangeTrackVolume(track, volume);

	private void Awake() {
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	public void PlaySound(Sound.Type soundType, int trackNumber) {
		AudioSource audioSource = GetTrack(trackNumber);
		audioSource.clip = GetAudioClip(soundType);
		audioSource.loop = true;

		if (!audioSource.isPlaying)
			audioSource.Play();
	}

	public void StopSound(int trackNumber) {
		AudioSource audioSource = GetTrack(trackNumber);
		if (audioSource != null && audioSource.isPlaying)
			audioSource.Stop();
	}

	public void PlaySoundOneShot(Sound.Type soundType, int trackNumber) {
		AudioSource audioSource = GetTrack(trackNumber);
		if (audioSource != null)
			audioSource.PlayOneShot(GetAudioClip(soundType));
	}

	public float GetTrackVolume(int trackNumber) {
		return GetTrack(trackNumber).volume;
	}

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
}
