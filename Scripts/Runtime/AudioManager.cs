using UnityEngine;
using System.Collections.Generic;

namespace Toybox {
	public class AudioManager : MonoBehaviour {
		public static AudioManager instance;

		public AudioSource musicSource, sfxSource;
		[Range(0f, 1f)]
		public float musicVolume = 0.5f, sfxVolume = 0.5f;
		public bool playMusicOnAwake;

		protected List<AudioSource> registeredSources;

		[System.Serializable]
		public class SoundBank {
			public string name = "";
			public AudioClip clip;
			[Range(0f, 1f)]
			public float volume = 1f;
		};
		
		public SoundBank[] soundBank;

		protected virtual void Awake() {
			if (AudioManager.instance) {
				Destroy(gameObject);
			}
			else {
				AudioManager.instance = this;
				DontDestroyOnLoad(gameObject);
			}

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

		// Method to introduce an Audio Source to the global volume control
		public void RegisterSource (AudioSource source) => registeredSources.Add(source);

		public virtual void PlayMusic(AudioClip clip) {
			musicSource.clip = clip;
			musicSource.Play();
		}

		// Plays an audio clip once
		public virtual void PlaySFX (AudioClip clip, float volume = 1f) => sfxSource.PlayOneShot(clip, volume);

		public virtual void StopMusic() {
			musicSource.Stop();
		}
	}
}
	