using System.Collections;
using UnityEngine;
// using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace Toybox {
    public class GameManager : Singleton<GameManager> {
		public delegate void BoolEvent(bool value);
		public delegate void IntEvent(int value);

		// public PostProcessProfile playerProfile;

		public Cursor cursorClick;
		public Cursor cursorRest;

		public BoolEvent OnPause;
		public IntEvent OnLevelLoaded;

		public bool showFPS = true;
		public FPSInfo fps;
		
		[HideInInspector]
		public PCInfo sysInfo;

		protected override void Awake () {
			base.Awake();

			OnPause += (a) => {};
			OnLevelLoaded += (a) => {};
			OnLevelLoaded += (int value) => {};

			//ComputeSystem();
		}

		void OnGUI () {
			if (showFPS)
				fps.FPSDisplay();
		}

		void Update() {
			fps.UpdateFPS();
		}

		public IEnumerator LoadScene (int sceneID) {
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneID);
			Application.backgroundLoadingPriority = ThreadPriority.Low;

			while (!asyncLoad.isDone) {
				yield return null;
			}
			OnLevelLoaded(sceneID);
		}

		public void ChangeQuality (int index) {
			QualitySettings.SetQualityLevel(index);
			// playerProfile.GetSetting<Bloom>().active = index > 1;
			// playerProfile.GetSetting<MotionBlur>().active = index > 2;
			// playerProfile.GetSetting<AmbientOcclusion>().active = index > 2;
		}

		public void SetResolution (int index) {
			int mw = sysInfo.maxResolution.width;
			int mh = sysInfo.maxResolution.height;

			if (index == 0) Screen.SetResolution(mw, mh, true);
			if (index == 1) Screen.SetResolution(mw/2, mh/2, true);
			if (index == 2) Screen.SetResolution(mw/4, mh/4, true);
		}

		void ComputeSystem () {
			sysInfo = new PCInfo();

			if (sysInfo.vram > 2048 && sysInfo.cores > 8 && sysInfo.cpu > 3000) {
				ChangeQuality(3);
			}
			else if (sysInfo.vram > 1024 && sysInfo.cores > 4 && sysInfo.cpu > 2600) {
				ChangeQuality(2);
			}
			else {
				ChangeQuality(1);
			}
		}
    }

	public class PCInfo {
		public int cpu, cores, ram, vram;
		public Resolution maxResolution;

		public PCInfo () {
			maxResolution = Screen.resolutions[Screen.resolutions.Length-1];

			vram = SystemInfo.graphicsMemorySize;
			cores = SystemInfo.processorCount;
			cpu = SystemInfo.processorFrequency;
			ram = SystemInfo.systemMemorySize;

			if (cpu == 0) cpu = 2800;
		}
	}

	[System.Serializable]
	public class FPSInfo {

		public Color color = new Color (1.0f, 1.0f, 1.0f, 1.0f);

		[HideInInspector]
		public float fps = 60;
		float deltaTime = 0.0f;

		public void UpdateFPS () {
			deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
			fps = 1.0f / deltaTime;
		}

		public void FPSDisplay () {
			int w = Screen.width, h = Screen.height;
	
			GUIStyle style = new GUIStyle();
	
			Rect rect = new Rect(10, 10, w, h * 2 / 100);
			style.alignment = TextAnchor.UpperLeft;
			style.fontSize = h * 2 / 80;
			style.normal.textColor = color;
			float msec = deltaTime * 1000.0f;
			string text = string.Format("{1:0.} FPS", msec, fps);
			GUI.Label(rect, text, style);
		}
	}
}