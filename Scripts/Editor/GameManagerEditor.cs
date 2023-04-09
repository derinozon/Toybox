using UnityEditor;

namespace Toybox {
	[CustomEditor(typeof(GameManager))]
	public class GameManagerEditor : Editor {
		GameManager root;

		void OnEnable () {
			root = (target as GameManager);

			SceneView.duringSceneGui += OnScene;
		}

		void OnDisable () {
			root = null;

			SceneView.duringSceneGui -= OnScene;
		}

		public override void OnInspectorGUI () {
			DrawDefaultInspector();
		}

		void OnScene (SceneView sceneView) {
			Handles.BeginGUI();
			Handles.EndGUI();
		}
	}
}