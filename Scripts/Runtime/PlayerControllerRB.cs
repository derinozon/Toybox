using UnityEngine;
using UnityEngine.Events;

namespace Toybox {
	
	[RequireComponent(typeof(Rigidbody))]
	public class PlayerControllerRB : MonoBehaviour {
		
		public PlayerSettings settings;

		[Header("Features")]
		public bool active = true;
		public bool lockRotation = false;
		public bool headbobbing = true;
		public bool enableFootstep = true;

		public PlayerEvents events;
		
		bool isFlying;
		float jumpA;
		float vRot;
		bool sprinting;
		bool grounded;
		float footstepTimer;
		float bobTimer = Mathf.PI / 2;
		Vector3 headRestPosition;

		Rigidbody rb;
		Transform camT;
		Vector3 movementDir;
		CapsuleCollider col;

		Vector3 BottomPoint {
			get {
				return transform.position + col.center + Vector3.down * col.height/2;
			}
		}

		void Awake () {
			rb = GetComponent<Rigidbody>();
			camT = GetComponentInChildren<Camera>().transform;
			col = GetComponent<CapsuleCollider>();

			rb.freezeRotation = true;
			rb.useGravity = false;

			if (!settings) {
				settings = ScriptableObject.CreateInstance<PlayerSettings>();
			}

			headRestPosition = camT.localPosition;
		}
		
		void Update () {

			if (!active) {
				movementDir = Vector3.zero;
				return;
			}
			sprinting = Input.GetKey(KeyCode.LeftShift);
			float percievedMaxVel = sprinting ? settings.maxVelocity*settings.sprintMultiplier : settings.maxVelocity; 

			MovePlayer();
			Jump();

			if (!lockRotation)
				RotateCam();

			if (headbobbing && !isFlying)
				BobHead();

			if (enableFootstep && !isFlying)
				Footstep();

			// Speed Limiting
			if (rb.linearVelocity.magnitude > percievedMaxVel) {
				Vector3 newVelocity = rb.linearVelocity.normalized;
				newVelocity *= percievedMaxVel;
				rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);
			}
			
			// Limit free fall speed
			if (rb.linearVelocity.y < settings.freeFallLimit) {
				rb.linearVelocity = new Vector3(rb.linearVelocity.x, settings.freeFallLimit, rb.linearVelocity.z);
			}
		}

		void FixedUpdate () {
			// Has HL1 bugs
			//rb.MovePosition(rb.position + movementDir * Time.fixedDeltaTime);

			// Accelerate
			rb.AddForce(500*movementDir * Time.fixedDeltaTime);

			// De-accelerate
			if (movementDir == Vector3.zero && !isFlying) {
				rb.linearVelocity = new Vector3(rb.linearVelocity.x*0.9f, rb.linearVelocity.y, rb.linearVelocity.z*0.9f);
			}

			// Gravity
        	rb.AddForce(-9.81f * settings.gravityScale * Vector3.up, ForceMode.Acceleration);
		}

		void Jump () {
			if (GroundCheck()) {
				if (Input.GetKey(KeyCode.Space)) {
					rb.linearVelocity = new Vector3(rb.linearVelocity.x, settings.jumpForce, rb.linearVelocity.z);
					if (!isFlying) {
						events.OnJump.Invoke();
					}
					isFlying = true;
				}
				if (isFlying && movementDir.y < 0) {
					isFlying = false;
				}
			}
		}

		void MovePlayer () {
			float movSpeed = sprinting ? settings.movementSpeed * settings.sprintMultiplier : settings.movementSpeed;
			float h = Input.GetAxisRaw("Horizontal");
			float v = Input.GetAxisRaw("Vertical");
			
			movementDir = new Vector3(h, 0, v).normalized * movSpeed;
			movementDir = transform.TransformDirection(movementDir);

			Ray ray = new Ray(transform.position, movementDir);
			//LayerMask lm = new LayerMask();
			//bool stick = Physics.Raycast(ray, col.radius + settings.raycastError, lm, QueryTriggerInteraction.Ignore);
			bool stick = Physics.Raycast(ray, col.radius + settings.raycastError);

			if (stick) {
				movementDir = Vector3.zero;
			}
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
			vRot += Input.GetAxis("Mouse Y") * settings.sensivity;
			vRot = Mathf.Clamp(vRot, -60, 60);

			transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * settings.sensivity);
			camT.localEulerAngles = Vector3.left * vRot;

			if (transform.eulerAngles.x != 0) {
				camT.SetParent(null, true);
				transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
				camT.SetParent(transform, true);
			}
		}

		bool GroundCheck () {
			Ray ray = new Ray(BottomPoint + Vector3.up*col.radius + Vector3.up*settings.raycastError, Vector3.down);
			return Physics.SphereCast(ray, col.radius, settings.raycastError*2);
		}
		
		void OnDrawGizmosSelected() {
			col = GetComponent<CapsuleCollider>();
			
			if (col && settings) {
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(BottomPoint, BottomPoint + Vector3.down * settings.raycastError);
				Gizmos.DrawWireSphere(BottomPoint + Vector3.up*col.radius, col.radius);
			}
    	}

		void OnCollisionEnter (Collision other) {
			isFlying = false;
		}
	}
}