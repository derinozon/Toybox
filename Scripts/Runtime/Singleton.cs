using UnityEngine;

namespace Toybox {
	public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> {
		private static T instance;

		public static T Instance {
			get {
				if (instance == null) {
					Debug.LogWarning("Singleton is null!");
				}
				return instance;
			}
		}

		protected virtual void Awake () {
			if (instance == null) {
				instance = (T)this;
				DontDestroyOnLoad(gameObject);
			}
			else {
				Destroy(gameObject);
			}
		}
	}
}