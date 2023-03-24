using UnityEngine;
using UnityEngine.Events;

namespace Toybox {
	[System.Serializable]
	public struct PlayerEvents {
		public UnityEvent OnWalk;
		public UnityEvent OnJump;
	}
	
	public class PlayerControllerCore : MonoBehaviour {
		
		public PlayerSettings settings;

		[Header("Features")]
		public bool enableMovement = true;
		public bool lockRotation = false;
		public bool headbobbing = true;
		public bool enableFootstep = true;

		public PlayerEvents events;
		
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

			if (!enableMovement) {
				movementDir = Vector3.zero;
			}
			else {
				sprinting = Input.GetButton("Fire3");
				float percievedMaxVel = sprinting ? settings.maxVelocity*settings.sprintMultiplier : settings.maxVelocity; 
				MovePlayer();
			}

			if (!lockRotation)
				RotateCam();

			if (headbobbing && !isFlying)
				BobHead();

			if (enableFootstep && !isFlying)
				Footstep();
		}

		void MovePlayer () {
			float movSpeed = sprinting ? settings.movementSpeed * settings.sprintMultiplier : settings.movementSpeed;
			float h = Input.GetAxisRaw("Horizontal");
			float v = Input.GetAxisRaw("Vertical");
			
			movementDir = new Vector3(h, 0, v).normalized * movSpeed;
			movementDir = transform.TransformDirection(movementDir);
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
			float mY = Input.GetAxis("Mouse Y");
			float mX = Input.GetAxis("Mouse X");

			vRot += mY * settings.sensivity;
			vRot = Mathf.Clamp(vRot, -60, 60);

			transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * settings.sensivity);
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