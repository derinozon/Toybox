using UnityEngine;

namespace Toybox {
	[CreateAssetMenu(fileName = "DefaultPlayerSetting", menuName = "DK/PlayerSettings", order = 1)]
	public class PlayerSettings : ScriptableObject {
		[Header("Movement Settings")]
		public float sprintMultiplier = 2;
		public float movementSpeed = 2;
		public float jumpForce = 2.5f;
		public float maxVelocity = 15;
		public float freeFallLimit = -50;
		public float footstepFrequency = 0.4f;

		[Header("Rotation Settings")]
		public float sensivity = 3;

		[Header("Bobbing Settings")]
		public float headRestTransitionSpeed = 20f;
		public float bobSpeed = 5f;
		public float bobAmount = 0.05f; 

		[Header("Other Settings")]
		public float gravityScale = 1;

		[Header("Advanced Settings")]
		public float raycastError = 0.02f;
	}
}