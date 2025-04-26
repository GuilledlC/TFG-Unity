using System;
using UnityEngine;
using FishNet.Object;
using Guille_dlCH.TFG.Projectiles;

namespace Guille_dlCH.TFG.Player {
	
	public struct CharacterItemInput {
		public Quaternion Rotation;
		public bool Shoot;
	}
	
	public class PlayerGun : NetworkBehaviour {
		
		[Header("Basic information")]
		[SerializeField] private Transform playerCamera;
		[SerializeField] private Transform muzzle;
		[SerializeField] private Projectile projectilePrefab;
		
		
		public Transform GetPlayerCamera() => playerCamera;

		private PlayerAnimator playerAnimator;
		private Quaternion _requestedRotation;
		private bool _requestedShoot;
		
		public void Initialize(PlayerAnimator animator/*Transform cameraTarget*/) {
			/*this.cameraTarget = cameraTarget;*/
			playerAnimator = animator;
		}

		public void UpdateInput(CharacterItemInput itemInput) {
			_requestedRotation = itemInput.Rotation;
			_requestedShoot = itemInput.Shoot;
		}

		void Update() {
			if (base.IsOwner) {
				if (_requestedShoot) {
					Shoot();
				}
			}
		}
		
		public void Shoot() {
			//EasyShootServer2(muzzle.position, transform.rotation, TimeManager.Tick);

			playerAnimator.Shoot();
			uint tick = TimeManager.Tick;
			SpawnBulletServerAuth(muzzle.position, transform.rotation, tick);
		}

		[ServerRpc]
		private void SpawnBulletServerAuth(Vector3 muzzlePosition, Quaternion rotation, uint startTick) {
			uint currentTick = TimeManager.Tick;
			uint timeDiff = currentTick - startTick;
			uint timeInSeconds = timeDiff / TimeManager.TickRate;
			Vector3 forward = rotation * Vector3.forward;
			Vector3 startPosition = playerCamera.position + (forward * (projectilePrefab.GetSpeed() * timeInSeconds));
			Projectile bullet = Instantiate(projectilePrefab, startPosition, rotation);
			ServerManager.Spawn(bullet.gameObject);
			bullet.Initialize(playerCamera.position, startPosition);
			SpawnTracerForAllClients(muzzlePosition, rotation, bullet);
		}

		[ObserversRpc]
		private void SpawnTracerForAllClients(Vector3 muzzlePosition, Quaternion rotation, Projectile bullet) {
			Tracer tracer = Instantiate(bullet.GetTracer(), muzzlePosition, rotation);
			tracer.Initialize(bullet);
		}

		
	}
}