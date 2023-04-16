using UnityEngine;
using System.Collections.Generic;

namespace Toybox {
	public class AudioManager : Singleton<AudioManager> {
		public static AudioManager instance;

		public AudioSource musicSource, sfxSource;
		[Range(0f, 1f)]
		public float musicVolume = 0.5f, sfxVolume = 0.5f;
		public bool playMusicOnAwake;

		protected List<AudioSource> registeredSources = new List<AudioSource>();

		[System.Serializable]
		public class SoundBank {
			public AudioClip clip;
			[Range(0f, 1f)]
			public float volume = 1f;
		};
		
		[HideInInspector]
		public SoundBank[] soundBank;

		protected override void Awake() {
			base.Awake();

			if (!musicSource) {
				musicSource = gameObject.AddComponent<AudioSource>();
			}
			musicSource.playOnAwake = playMusicOnAwake;

			if (!sfxSource) {
				sfxSource = gameObject.AddComponent<AudioSource>();
				sfxSource.playOnAwake = false;
			}
		}

		void Update () {
			musicSource.volume = musicVolume;
			sfxSource.volume = sfxVolume;

			foreach (var source in registeredSources) {
				source.volume = sfxVolume;
			}
		}

		SoundBank GetSoundFromBank (string name) {
			foreach (var item in soundBank) {
				if (item.clip.name == name)
					return item;
			}
			return null;
		}

		// Method to introduce an Audio Source to the global volume control
		public void RegisterSource (AudioSource source) => registeredSources.Add(source);

		// Plays the music
		public virtual void PlayMusic(AudioClip clip) {
			musicSource.clip = clip;
			musicSource.Play();
		}

		// Plays an audio clip once
		public virtual void PlaySFX (AudioClip clip, float volume = 1f) => sfxSource.PlayOneShot(clip, volume);

		public virtual void PlaySFX (string name) {
			SoundBank sound = GetSoundFromBank(name);
			if (sound != null) {
				sfxSource.PlayOneShot(sound.clip, sfxVolume * sound.volume);
			}
			else {
				Debug.LogWarning("No such Audio Clip as " + name + " in Sound Bank");
			}
		}
		// Stops the music
		public virtual void StopMusic() => musicSource.Stop();
	}
}
	