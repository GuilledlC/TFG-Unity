using UnityEngine;

namespace Guille_dlCH.TFG.Input {
	
	public struct CameraInput {
		public Vector2 Look;
	}
	
	public class InputManager : Singleton<InputManager> {

		public PlayerInputActions InputActions { get; private set; }

		protected override void Awake() {
			base.Awake();
			InputActions = new PlayerInputActions();
			InputActions.Enable();
		}

		private void OnDestroy() {
			InputActions.Disable();
			InputActions.Dispose();
		}
	}
	
}