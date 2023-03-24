using UnityEngine;

namespace Toybox {
	public class AudioManager : MonoBehaviour {
		public static AudioManager instance;

		public AudioSource musicSource, sfxSource;
		[Range(0,1)]
		public float musicVolume = 0.5f, sfxVolume = 0.5f;

		void Awake() {
			if (AudioManager.instance) {
				Destroy(gameObject);
			}
			else {
				AudioManager.instance = this;
				DontDestroyOnLoad(gameObject);
			}

			if (!musicSource) {
				musicSource = gameObject.AddComponent<AudioSource>();
				musicSource.playOnAwake = false;
			}

			if (!sfxSource) {
				sfxSource = gameObject.AddComponent<AudioSource>();
				sfxSource.playOnAwake = false;
			}
		}

		void Update () {
			musicSource.volume = musicVolume;
			sfxSource.volume = sfxVolume;
		}

		public void PlayMusic(AudioClip clip) {
			musicSource.clip = clip;
			musicSource.Play();
		}

		public void PlaySFX (AudioClip clip) {
			sfxSource.PlayOneShot(clip);
		}

		public void StopMusic() {
			musicSource.Stop();
		}
	}
}
	