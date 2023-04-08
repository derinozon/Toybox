using UnityEngine;
using UnityEngine.Events;

namespace Toybox {
	[System.Serializable]
	public struct PlayerEvents {
		public UnityEvent OnWalk;
		public UnityEvent OnJump;
	}

	[System.Serializable]
	public struct InputData {
		public float moveX, moveY, lookX, lookY;
		public bool sprint;

		public InputData (float moveX, float moveY, float lookX, float lookY, bool sprint) {
			this.moveX = moveX;
			this.moveY = moveY;
			this.lookX = lookX;
			this.lookY = lookY;
			this.sprint = sprint;
		}
	}
	
	public class PlayerControllerCore : MonoBehaviour {
		
		public PlayerSettings settings;

		[Header("Features")]
		public bool enableMovement = true;
		public bool lockRotation = false;
		public bool headbobbing = true;
		public bool enableFootstep = true;
		public bool manualInput = false;

		public PlayerEvents events;
		
		InputData input;
		bool isFlying;
		float vRot;
		bool sprinting;
		float footstepTimer;
		float bobTimer = Mathf.PI / 2;
		Vector3 headRestPosition;

		protected Transform camT;
		protected Vector3 movementDir;

		virtual protected void Awake () {
			camT = GetComponentInChildren<Camera>().transform;

			if (!settings) {
				settings = ScriptableObject.CreateInstance<PlayerSettings>();
			}

			headRestPosition = camT.localPosition;
		}
		
		virtual protected void Update () {
			if (!manualInput) {
				input = new InputData(
					Input.GetAxisRaw(settings.moveX),
					Input.GetAxisRaw(settings.moveY),
					Input.GetAxis(settings.lookX),
					Input.GetAxis(settings.lookY),
					Input.GetButton(settings.sprint)
				);
			}

			movementDir = enableMovement ? CalculateMovement() : Vector3.zero;

			if (!lockRotation)
				RotateCam();

			if (!isFlying) {
				if (headbobbing)
					BobHead();

				if (enableFootstep)
					Footstep();
			}
		}

		public void PollInput (InputData input) {
			this.input = input;
		}

		Vector3 CalculateMovement () {
			float movSpeed = sprinting ? settings.movementSpeed * settings.sprintMultiplier : settings.movementSpeed;
			Vector3 movDir = new Vector3(input.moveX, 0, input.moveY).normalized * movSpeed;
			return transform.TransformDirection(movDir);
		}

		void Footstep () {
			if (movementDir != Vector3.zero) {
				footstepTimer += Time.deltaTime * (sprinting ? settings.sprintMultiplier : 1);

				if (footstepTimer > settings.footstepFrequency) {
					events.OnWalk.Invoke();
					footstepTimer = 0;
				}
			}
			else {
				footstepTimer = 0.9f;
			}
		}

		void BobHead() {
			if (movementDir != Vector3.zero) {
				bobTimer += settings.bobSpeed * Time.deltaTime * (sprinting ? settings.sprintMultiplier : 1);

				// Absolute value of y for a parabolic path
				camT.localPosition = new Vector3(Mathf.Cos(bobTimer) * settings.bobAmount, headRestPosition.y + Mathf.Abs((Mathf.Sin(bobTimer) * settings.bobAmount)), headRestPosition.z);
			}
			else {
				bobTimer = Mathf.PI / 2;
				camT.localPosition = Vector3.Lerp(camT.localPosition, headRestPosition, settings.headRestTransitionSpeed * Time.deltaTime);
			}

			// Completed a full cycle on the unit circle. Reset to 0 to avoid bloated values.
			if (bobTimer > Mathf.PI * 2)
				bobTimer = 0;
		}

		void RotateCam () {

			vRot += input.lookY * settings.sensivity;
			vRot = Mathf.Clamp(vRot, -60, 60);

			transform.Rotate(Vector3.up * input.lookX * settings.sensivity);
			camT.localEulerAngles = Vector3.left * vRot;

			if (transform.eulerAngles.x != 0) {
				camT.SetParent(null, true);
				transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
				camT.SetParent(transform, true);
			}
		}

		void OnCollisionEnter (Collision other) {
			isFlying = false;
		}
	}
}