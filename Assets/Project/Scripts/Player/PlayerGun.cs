using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using Guille_dlCH.TFG.Projectiles;
using TMPro;

namespace Guille_dlCH.TFG.Player {
	
	public struct CharacterShotInput {
		public Quaternion Rotation;
		public bool Shoot;
	}

	public struct Shot {
		public Vector3 Origin;
		public Quaternion Direction;
		public NetworkObject Player;

		public Shot(Vector3 origin, Quaternion direction) {
			this.Origin = origin;
			this.Direction = direction;
			this.Player = null;
		}

		public void SetPlayer(NetworkObject networkObject) {
			this.Player = networkObject;
		}

		public NetworkObject GetPlayer() {
			return this.Player;
		}
	}
	
	public class PlayerGun : NetworkBehaviour {
		
		#region("Serialized Fields")

		[Header("Basic information")]
		[SerializeField] private Transform playerCamera;
		[SerializeField] private Transform muzzle;

		[Header("Sound")]
		[SerializeField] private AudioSource audioSource;
		[SerializeField] private AudioClip shotSound;
		[SerializeField] private AudioClip pingSound;
		[SerializeField] private AudioClip reloadSound;
		
		[Header("Bullet properties")]
		[SerializeField] private float caliber;
		[SerializeField] private float effectiveDistance;
		
		[Header("Weapon Properties")]
		[SerializeField] private float fireRate = 0.25f; // Time between shots in seconds
		
		private int score = 0;
		[SerializeField] private TMP_Text scoreText;

		#endregion
		
		#region("Private Variables")

		private readonly SyncVar<bool> canShoot = new SyncVar<bool>(true);
		private readonly List<Shot> visualShots = new List<Shot>(); // For gizmo drawing only
		private PlayerAnimator playerAnimator;
		private bool requestedShoot;

		#endregion
		
		#region("Event Methods")

		public override void OnStartClient() {
			if (!base.IsOwner) {
				scoreText.enabled = false;
			}
		}

		public void Initialize(PlayerAnimator animator) {
			playerAnimator = animator;
		}

		public void UpdateInput(CharacterShotInput shotInput) {
			requestedShoot = shotInput.Shoot;
		}

		private void Update() {
			if (!base.IsOwner) return;
	
			if (requestedShoot && canShoot.Value) {
				ClientPredictShot();
			}
		}

		#endregion

		#region("Client-Side Prediciton")

		private void ClientPredictShot() {
			Shot shot = new Shot(playerCamera.position, playerCamera.rotation);
			PlayShotEffects(shot);
			bool predictedHit = ShotHit(ref shot);
			if (predictedHit) {
				score++;
				scoreText.SetText(score.ToString());
				PlayHitEffects(shot);
				Debug.Log($"Predicted Hit {shot.Player.name}");
			}
			RequestServerShoot(shot, predictedHit);
		}		

		#endregion
		
		#region("Common Methods")

		private bool ShotHit(ref Shot shot) {
			Vector3 forward = shot.Direction * Vector3.forward;
			bool hit = Physics.SphereCast(shot.Origin, caliber / 2, forward, out RaycastHit hitInfo, effectiveDistance,
				LayerMask.GetMask("Player"));
			if (hit) {
				shot.SetPlayer(hitInfo.collider.gameObject.GetComponent<NetworkObject>());
			}
			return hit;
		}

		private void PlayShotEffects(Shot shot) {
			playerAnimator?.Shoot();
			if (shotSound != null) {
				audioSource?.PlayOneShot(shotSound);
			}
			//Debugging
			StartCoroutine(DrawShot(shot));
		}

		private void PlayHitEffects(Shot shot) {
			shot.GetPlayer().GetComponent<PlayerHit>()?.GetHit();
		}
		
		#endregion

		#region("Server Methods")

		private IEnumerator ShotCooldown() {
			canShoot.Value = false;
			yield return new WaitForSeconds(fireRate);
			canShoot.Value = true;
		}

		#endregion
		
		#region(RPCs)

		[ServerRpc]
		private void RequestServerShoot(Shot shot, bool predictedHit) {
			NetworkConnection conn = base.Owner;
			if (conn == null) return;

			if (!canShoot.Value) {				//If you can't even shoot
				if (predictedHit)				//But the client thinks it hit something
					TargetHitRejected(conn);	//Reject it
				return;							//Exit
			}

			// Shot accepted
			// Broadcast shot to all observers
			ObserverShoot(shot);

			bool serverHit = ShotHit(ref shot);
			if (serverHit) {
				ObserverHit(shot);
				ObserverLogHit(shot);
				if (!predictedHit) {
					TargetActuallyHit(conn, shot);
				}
			}
			else if (predictedHit)			//If you didn't hit anything but the client thinks it did
				TargetHitRejected(conn);	//Reject it

			StartCoroutine(ShotCooldown());
		}

		[TargetRpc]
		private void TargetHitRejected(NetworkConnection conn) {
			score--;
			scoreText.SetText(score.ToString());
			Debug.Log($"Didn't Hit");
		}
		
		[TargetRpc]
		private void TargetActuallyHit(NetworkConnection conn, Shot shot) {
			score++;
			scoreText.SetText(score.ToString());
			PlayHitEffects(shot);
			Debug.Log($"Server Actually Hit {shot.Player.name}");
		}

		[ObserversRpc(ExcludeOwner = true)]
		private void ObserverShoot(Shot shot) {
			PlayShotEffects(shot);
		}
			
		[ObserversRpc(ExcludeOwner = true)]
		private void ObserverHit(Shot shot) {
			PlayHitEffects(shot);
		}
		
		[ObserversRpc]
		private void ObserverLogHit(Shot shot) {
			Debug.Log($"Observer Hit {shot.Player.name}");
		}

		#endregion

		// Drawing shot trajectories for debugging
		IEnumerator DrawShot(Shot shot) {
			visualShots.Add(shot);
			yield return new WaitForSeconds(1.5f);
			visualShots.Remove(shot);
		}

		private void OnDrawGizmos() {
			foreach (var shot in visualShots) {
				Gizmos.color = Color.red;
				Gizmos.DrawLine(shot.Origin, shot.Origin + (shot.Direction * Vector3.forward * effectiveDistance));
			}
		}
		
	}
}