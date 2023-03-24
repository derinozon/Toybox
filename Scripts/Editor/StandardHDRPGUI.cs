using UnityEngine;
using UnityEditor;

namespace Toybox {

	[CanEditMultipleObjects]
	public class StandardHDRPGUI : ShaderGUI {
		Color color, outlineColor;
		Texture albedoMap, normalMap, maskMap, emissionMap, heightMap, detailMap;
		float emissionStr, outlineWidth;

		float metallicStr = 0f;
		float smoothnessStr = 0.5f;
		Vector2 tiling = Vector2.one;
		Vector2 offset = Vector2.zero;

		bool showEmission, showOutline;

		enum RenderType {
			Opaque, Transparent
		};

		RenderType renderType;

		[ColorUsage(false, true)]
		Color emissionColor;
		MaterialGlobalIlluminationFlags giFlags = MaterialGlobalIlluminationFlags.BakedEmissive;

		private static Texture TextureField(string name, Texture texture) {
			GUILayout.BeginVertical();
			var style = new GUIStyle(GUI.skin.label);
			style.alignment = TextAnchor.UpperCenter;
			style.fixedWidth = 70;
			GUILayout.Label(name, style);
			var result = (Texture)EditorGUILayout.ObjectField(texture, typeof(Texture), false, GUILayout.Width(70), GUILayout.Height(70));
			GUILayout.EndVertical();
			return result;
		}

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
			//base.OnGUI(materialEditor, properties);
			Material targetMat = materialEditor.target as Material;


			// --- Get Material Properties --- //
			color = targetMat.GetColor("_Color");

			albedoMap = targetMat.GetTexture("_MainTex");
			normalMap = targetMat.GetTexture("_Normal");
			maskMap = targetMat.GetTexture("_Mask");
			emissionMap = targetMat.GetTexture("_Emission");
			heightMap = targetMat.GetTexture("_HeightMap");

			emissionStr = targetMat.GetFloat("_EmissionStr");
			emissionColor = targetMat.GetColor("_EmissionColor");
			
			smoothnessStr = targetMat.GetFloat("_Smoothness");
			metallicStr = targetMat.GetFloat("_Metalness");

			tiling = targetMat.GetTextureScale("_MainTex");
			offset = targetMat.GetTextureOffset("_MainTex");
			
			showOutline = targetMat.IsKeywordEnabled("OUTLINE_ON");
			outlineColor = targetMat.GetColor("_OutlineColor");
			outlineWidth = targetMat.GetFloat("_OutlineWidth");

			renderType = targetMat.renderQueue < 3000 ? RenderType.Opaque : RenderType.Transparent;
			giFlags = targetMat.globalIlluminationFlags;
			

			// --- Render UI --- //
			EditorGUI.BeginChangeCheck();

			renderType = (RenderType)EditorGUILayout.EnumPopup("Render Type", renderType);

			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal();
			albedoMap = TextureField("Albedo", albedoMap);
			normalMap = TextureField("Normal", normalMap);
			maskMap = TextureField("Mask", maskMap);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();
			EditorGUILayout.BeginHorizontal();
			emissionMap = TextureField("Emission", emissionMap);
			heightMap = TextureField("Height", heightMap);
			detailMap = TextureField("Detail", detailMap);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			tiling = EditorGUILayout.Vector2Field("Tiling", tiling);
			offset = EditorGUILayout.Vector2Field("Offset", offset);

			EditorGUILayout.Separator();

			
			color = EditorGUILayout.ColorField("Color", color);
			
			if (maskMap == null) {
				metallicStr = EditorGUILayout.Slider("Metallic", metallicStr, 0, 1);
				smoothnessStr = EditorGUILayout.Slider("Smoothness", smoothnessStr, 0, 1);
			}

			emissionColor = EditorGUILayout.ColorField(new GUIContent("Emission Color"), emissionColor, true, false, true);
			if (emissionMap != null || emissionColor != Color.black) {
				//emissionStr = EditorGUILayout.Slider("Emission Strength", emissionStr, 0, 10);
				giFlags = (MaterialGlobalIlluminationFlags) EditorGUILayout.EnumPopup("Emission GI", giFlags);
			}
			

			

			if (renderType == RenderType.Opaque) {
				showOutline = EditorGUILayout.Toggle("Display Outline", showOutline);
				if (showOutline) {
					outlineColor = EditorGUILayout.ColorField("Outline Color", outlineColor);
					outlineWidth = EditorGUILayout.Slider("Outline Width", outlineWidth, 1, 1.1f);
				}
			}
			
			// --- Set Values If Changed --- //
			if (EditorGUI.EndChangeCheck()) {
				targetMat.SetColor("_Color", color);

				targetMat.SetTexture("_MainTex", albedoMap);
				targetMat.SetTexture("_Normal", normalMap);
				targetMat.SetTexture("_Mask", maskMap);
				targetMat.SetTexture("_Emission", emissionMap);
				targetMat.SetTexture("_HeightMap", heightMap);

				targetMat.SetFloat("_EmissionStr", emissionStr);
				targetMat.SetColor("_EmissionColor", emissionColor);

				targetMat.globalIlluminationFlags = giFlags;

				// Tiling and Offset //
				targetMat.SetTextureScale("_MainTex", tiling);
				targetMat.SetTextureScale("_Normal", tiling);
				targetMat.SetTextureScale("_Mask", tiling);
				targetMat.SetTextureScale("_Emission", tiling);
				targetMat.SetTextureScale("_HeightMap", tiling);

				targetMat.SetTextureOffset("_MainTex", offset);
				targetMat.SetTextureOffset("_Normal", offset);
				targetMat.SetTextureOffset("_Mask", offset);
				targetMat.SetTextureOffset("_Emission", offset);
				targetMat.SetTextureOffset("_HeightMap", offset);

				if (maskMap == null) {
					targetMat.DisableKeyword("MASK_ON");

					targetMat.SetFloat("_Smoothness", smoothnessStr);
					targetMat.SetFloat("_Metalness", metallicStr);
				}
				else {
					targetMat.EnableKeyword("MASK_ON");
				}

				

				if (renderType == RenderType.Transparent) {
					targetMat.SetOverrideTag("RenderType", "Transparent");
					targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
					targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					targetMat.SetInt("_ZWrite", 0);
					// targetMat.EnableKeyword("TPP");
					targetMat.renderQueue = 3000;
				}
				else {
					targetMat.SetOverrideTag("RenderType", "Geometry");
					targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
					targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
					targetMat.SetInt("_ZWrite", 1);
					//targetMat.DisableKeyword("TPP");
					targetMat.renderQueue = 2000;


					if (showOutline) {
						targetMat.EnableKeyword("OUTLINE_ON");
						targetMat.renderQueue = 2999;
						targetMat.SetShaderPassEnabled("Always", true);

						targetMat.SetColor("_OutlineColor", outlineColor);
						targetMat.SetFloat("_OutlineWidth", outlineWidth);
					}
					else {
						targetMat.DisableKeyword("OUTLINE_ON");
						targetMat.renderQueue = 2000;
						targetMat.SetShaderPassEnabled("Always", false);
					}
				}

				
			}
		}
	}
}