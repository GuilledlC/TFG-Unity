using Guille_dlCH.TFG.Input;
using UnityEngine;

namespace Guille_dlCH.TFG.Player {
	public class PlayerCamera : MonoBehaviour {
		
		[SerializeField] private float sensitivity = 0.2f;
		private Vector3 _eulerAngles;

		public void UpdateRotation(CameraInput input) {
		
			_eulerAngles += new Vector3(-input.Look.y, input.Look.x) * sensitivity;
		
			//Clamp rotation
			_eulerAngles = new Vector3(Mathf.Clamp(_eulerAngles.x, -90f, 90f), _eulerAngles.y, _eulerAngles.z);
		
			transform.eulerAngles = _eulerAngles;
		}
		
	}
}