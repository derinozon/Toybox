using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Toybox {
	[CustomEditor(typeof(AudioManager))]
	public class AudioManagerEditor : Editor {
		AudioManager root;
		SerializedProperty wave;

		ReorderableList list;

		static int soundPlayingIndex = -1;
		static float clipEndTime = -1;
		static AudioSource temp;

		void OnEnable () {
			root = (target as AudioManager);
			wave = serializedObject.FindProperty("soundBank");

			list = new ReorderableList(serializedObject, wave, true, true, true, true);
			list.drawElementCallback = DrawListItems;
			list.drawHeaderCallback = DrawHeader;

			EditorApplication.update += SoundIndexTimer;

			var tempobj = new GameObject("TEMP");
			tempobj.hideFlags = HideFlags.HideInHierarchy;
			temp = tempobj.AddComponent<AudioSource>();
		}

		void OnDisable () {
			root = null;

			StopAllClips();
			
			EditorApplication.update -= SoundIndexTimer;
			DestroyImmediate(temp.gameObject);

			soundPlayingIndex = -1;
			clipEndTime = -1;
		}

		EditorApplication.CallbackFunction SoundIndexTimer = () => {
			if (clipEndTime > 0 && Time.realtimeSinceStartup > clipEndTime) {
				ResetSoundIndex();
				clipEndTime = -1;
			}
		};

		public static void PlayClip (AudioClip clip, float volume) {
			temp.PlayOneShot(clip, volume);
		}

		public static void StopAllClips () {
			temp.Stop();
		}

		void DrawListItems(Rect rect, int index, bool isActive, bool isFocused) {
			SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
			
			EditorGUI.PropertyField(
				new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), 
				element.FindPropertyRelative("clip"),
				GUIContent.none
			);

			EditorGUI.PropertyField(
				new Rect(rect.x + 220, rect.y, 200, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("volume"),
				GUIContent.none
			);

			var btncontent = soundPlayingIndex != index ? new GUIContent("Play") : new GUIContent("Stop");
			var btnClicked = GUI.Button(
				new Rect(rect.x + 440, rect.y, 160, EditorGUIUtility.singleLineHeight),
				btncontent
			);

			if (btnClicked) {
				if (soundPlayingIndex > -1) {
					StopAllClips();
				}
				if (soundPlayingIndex != index) {
					var clip = root.soundBank[index].clip;
					PlayClip(clip, root.soundBank[index].volume);
					soundPlayingIndex = index;
					clipEndTime = Time.realtimeSinceStartup + clip.length;
				}
				else {
					ResetSoundIndex();
				}
			}
		}

		static void ResetSoundIndex () => soundPlayingIndex = -1;

		void DrawHeader(Rect rect) {
			EditorGUI.LabelField(rect, "Sound Bank");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			list.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}
	}
}


////////////////////////////////////////////
// Old play audio functions for referance //
////////////////////////////////////////////
// using System;
// using System.Reflection;
// public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false) {
// 	Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

// 	Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
// 	MethodInfo method = audioUtilClass.GetMethod(
// 		"PlayPreviewClip",
// 		BindingFlags.Static | BindingFlags.Public,
// 		null,
// 		new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
// 		null
// 	);

// 	method.Invoke(
// 		null,
// 		new object[] { clip, startSample, loop }
// 	);
// }
// public static void StopAllClips() {
// 	Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

// 	Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
// 	MethodInfo method = audioUtilClass.GetMethod(
// 		"StopAllPreviewClips",
// 		BindingFlags.Static | BindingFlags.Public,
// 		null,
// 		new Type[] { },
// 		null
// 	);

// 	method.Invoke(
// 		null,
// 		new object[] { }
// 	);
// }