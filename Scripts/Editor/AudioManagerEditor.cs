using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Toybox {
	[CustomEditor(typeof(AudioManager))]
	public class AudioManagerEditor : Editor {
		AudioManager root;
		SerializedProperty soundBankProp;

		ReorderableList list;

		static int soundPlayingIndex = -1;
		static float clipEndTime = -1;
		static AudioSource temp;

		void OnEnable () {
			root = (target as AudioManager);
			soundBankProp = serializedObject.FindProperty("soundBank");

			list = new ReorderableList(serializedObject, soundBankProp, true, true, true, true);
			list.drawElementCallback = DrawListItems;
			list.drawHeaderCallback = DrawHeader;

			EditorApplication.update += SoundIndexTimer;
		}

		void OnDisable () {
			root = null;

			StopAllClips();
			
			EditorApplication.update -= SoundIndexTimer;

			soundPlayingIndex = -1;
			clipEndTime = -1;
		}

		void CreateTemp () {
			var tempobj = new GameObject("TEMP");
			tempobj.hideFlags = HideFlags.HideInHierarchy;
			temp = tempobj.AddComponent<AudioSource>();
		}

		static void DestroyTemp () {
			temp.Stop();
			DestroyImmediate(temp.gameObject);
			temp = null;
		}

		EditorApplication.CallbackFunction SoundIndexTimer = () => {
			if (clipEndTime > 0 && Time.realtimeSinceStartup > clipEndTime) {
				ResetSoundIndex();
				clipEndTime = -1;
				StopAllClips();
			}
		};

		void PlayClip (AudioClip clip, float volume) {
			if (!temp) {
				CreateTemp();
			}
			temp.PlayOneShot(clip, volume);
		}

		static void StopAllClips () {
			if (temp) {
				DestroyTemp();
			}
		}

		void DrawListItems(Rect rect, int index, bool isActive, bool isFocused) {
			SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
			int margin = 40;
			int spacing = 0;

			EditorGUI.PropertyField(
				new Rect(rect.x, rect.y, 175, EditorGUIUtility.singleLineHeight), 
				element.FindPropertyRelative("clip"),
				GUIContent.none
			);
			spacing += 175;

			EditorGUI.PropertyField(
				new Rect(rect.x + spacing + margin, rect.y, 175, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("volume"),
				GUIContent.none
			);
			spacing += 175;

			var btnClicked = GUI.Button(
				new Rect(rect.x + spacing + margin, rect.y, 60, EditorGUIUtility.singleLineHeight),
				new GUIContent("Copy")
			);
			spacing += 60;

			if (btnClicked) {
				GUIUtility.systemCopyBuffer = root.soundBank[index].clip.name;
			}

			var btncontent = soundPlayingIndex != index ? new GUIContent("Play") : new GUIContent("Stop");
			btnClicked = GUI.Button(
				new Rect(rect.x + spacing + margin, rect.y, 60, EditorGUIUtility.singleLineHeight),
				btncontent
			);
			spacing += 60;

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
			string label = "Sound Bank";
			if (soundPlayingIndex != -1) {
				label += " - Playing " + root.soundBank[soundPlayingIndex].clip.name;
			}
			EditorGUI.LabelField(rect, label);
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
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