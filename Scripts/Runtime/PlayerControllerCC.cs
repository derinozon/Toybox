using UnityEngine;
using UnityEngine.Events;

namespace Toybox {
	
	[RequireComponent(typeof(CharacterController))]
	public class PlayerControllerCC : PlayerControllerCore {
		protected CharacterController controller;
		
		override protected void Awake () {
			base.Awake();
			controller = GetComponent<CharacterController>();
		}

		override protected void Update () {
			base.Update();
			if (enableMovement) {
				controller.SimpleMove(movementDir);
			}
		}
	}
		
}