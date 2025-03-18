using System.Collections;
using UnityEngine;
using FishNet.Object;
using Guille_dlCH.TFG.Input;

public class Player : NetworkBehaviour {
	
	private Camera mainCamera;
	
	public override void OnStartClient() {
		base.OnStartClient();
		
		if (base.IsOwner) {
			StartCoroutine(SetupPlayer());
		}
		else {
			
		}
	}
	
	IEnumerator SetupPlayer() {
		yield return new WaitForEndOfFrame();
		
		mainCamera = Camera.main;
		if (mainCamera != null) {
			mainCamera.transform.SetParent(this.transform);
			mainCamera.transform.localPosition = Vector3.up * 1.8f;  //DO A CAMERA POINT
			mainCamera.transform.localRotation = Quaternion.identity;
		} else {
			Debug.LogError("Camera or CameraSpring is still null!");
		}
	}

	private void Update() {
		if (base.IsOwner) {
			var deltaTime = Time.deltaTime;

			PlayerInputActions.PlayerActions playerActions = InputManager.Instance.InputActions.Player;
			var cameraInput = new CameraInput { Look = playerActions.Look.ReadValue<Vector2>() };
		}
	}
}
