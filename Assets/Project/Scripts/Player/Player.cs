using System.Collections;
using UnityEngine;
using FishNet.Object;
using Guille_dlCH.TFG.Input;
using Guille_dlCH.TFG.Player;
using KinematicCharacterController;

namespace Guille_dlCH.TFG.Player {

	public class Player : NetworkBehaviour {

		public static string PlayerName;
		
		[SerializeField] private PlayerCamera playerCamera;
		[SerializeField] private PlayerCharacter playerCharacter;
		[SerializeField] private PlayerGun playerGun;
		[SerializeField] private PlayerAnimator playerAnimator;

		private Camera mainCamera;

		public override void OnStartClient() {
			base.OnStartClient();

			if (base.IsOwner) {
				SetPlayerName(PlayerName);
				StartCoroutine(SetupPlayer());
			}
			else {
				playerCamera.enabled = false;
				playerCharacter.enabled = false;
				playerCharacter.gameObject.GetComponent<KinematicCharacterMotor>().enabled = false;
				playerGun.enabled = false;
				playerAnimator.Disable();
			}
		}

		IEnumerator SetupPlayer() {
			yield return new WaitForEndOfFrame();

			mainCamera = Camera.main;
			if (mainCamera != null) {
				mainCamera.transform.SetParent(playerCamera.transform);
				mainCamera.transform.localPosition = Vector3.zero;
				mainCamera.transform.localRotation = Quaternion.identity;
			}
			else {
				Debug.LogError("Camera is still null!");
			}

			playerCharacter.Initialize();
			playerAnimator.Initialize(playerCharacter);
			playerGun.Initialize(playerAnimator);

			//Utils.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Player"));
		}

		private void Start() {
			Cursor.lockState = CursorLockMode.Locked;
		}

		private void Update() {
			if (base.IsOwner) {
				var deltaTime = Time.deltaTime;

				PlayerInputActions.PlayerActions playerActions = InputManager.Instance.InputActions.Player;

				//Camera input
				var cameraInput = new CameraInput { Look = playerActions.Look.ReadValue<Vector2>() };
				playerCamera.UpdateRotation(cameraInput);

				//Character input
				var characterMovementInput = new CharacterMovementInput {
					Rotation = playerCamera.transform.rotation,
					Move = playerActions.Move.ReadValue<Vector2>(),
					Jump = playerActions.Jump.WasPressedThisFrame(),
					JumpSustain = playerActions.Jump.IsPressed()
				};
				playerCharacter.UpdateInput(characterMovementInput);
				
				//Get character item input and update it
				var characterItemInput = new CharacterShotInput {
					Rotation = playerCamera.transform.rotation,
					Shoot = playerActions.Shoot.WasPressedThisFrame()
				};
				playerGun.UpdateInput(characterItemInput);
			}
		}

		[ServerRpc]
		private void SetPlayerName(string _name) {
			SetPlayerNameObserver(_name);
		}
		
		[ObserversRpc(BufferLast=true)]
		private void SetPlayerNameObserver(string _name) {
			gameObject.name = _name;
		}
		
		private void LateUpdate() {
			//This needs to be here for all players so the PlayerCamera.cs's gameobject
			//always moves towards the camera target. It's also the reason why Player.cs
			//and PlayerCharacter.cs aren't disabled.
			//playerCamera.UpdatePosition(cameraTarget);
		}
	}

}