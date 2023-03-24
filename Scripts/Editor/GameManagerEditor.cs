using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace Toybox {
	[CustomEditor(typeof(GameManager))]
	public class GameManagerEditor : Editor {
		GameManager root;

		private void OnEnable () {
			root = (target as GameManager);

			SceneView.duringSceneGui += OnScene;
		}

		private void OnDisable () {
			root = null;

			SceneView.duringSceneGui -= OnScene;
		}

		SerializedProperty wave;  
    
    	// The Reorderable List we will be working with 
    	ReorderableList list; 
		List<string> list2 = new List<string>();
		public override void OnInspectorGUI () {
			DrawDefaultInspector();
			
			// if (root.showFPS) {

			// }
			
			// var reorderableList = new ReorderableList(list2, typeof(string), false, true, true, true);
			// reorderableList.DoLayoutList();
		}

		void OnScene (SceneView sceneView) {
			Handles.BeginGUI();
			
			Handles.EndGUI();
		}
	}
}