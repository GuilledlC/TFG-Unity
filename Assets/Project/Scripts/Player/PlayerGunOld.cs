using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using Guille_dlCH.TFG.Projectiles;
using UnityEngine.Serialization;

namespace Guille_dlCH.TFG.Player {
	
	public class PlayerGunOld : NetworkBehaviour {
		
		[Header("Basic information")]
		[SerializeField] private Transform actualMuzzle;

		[Header("Sound")]
		[SerializeField] private AudioSource audioSource;
		[SerializeField] private AudioClip shotSound;
		[SerializeField] private AudioClip pingSound;
		[SerializeField] private AudioClip reloadSound;
		
		[Header("Bullet properties")]
		[SerializeField] private float caliber;
		[SerializeField] private float effectiveDistance;
		
		
		private PlayerAnimator playerAnimator;
		private Quaternion _requestedRotation;
		private bool _requestedShoot;
		
		public void Initialize(PlayerAnimator animator) {
			playerAnimator = animator;
		}

		public void UpdateInput(CharacterShotInput shotInput) {
			_requestedRotation = shotInput.Rotation;
			_requestedShoot = shotInput.Shoot;
		}

		void Update() {
			if (base.IsOwner) {
				if (_requestedShoot) {
					Shoot();
				}
			}
		}

		private List<Shot> shots = new List<Shot>();
		private void OnDrawGizmos() {
			foreach (var shot in shots) {
				Gizmos.color = Color.red;
				Gizmos.DrawLine(shot.Origin, shot.Origin + (shot.Direction* Vector3.forward * effectiveDistance));
			}
		}

		IEnumerator DrawShot(Shot shot) {
			shots.Add(shot);
			yield return new WaitForSeconds(1.5f);
			shots.Remove(shot);
		}

		public void Shoot() {
			playerAnimator.Shoot(); //Synced by the NetworkAnimator
			audioSource.PlayOneShot(shotSound); //Our shot

			Shot shot = new Shot(actualMuzzle.position, actualMuzzle.rotation);
			StartCoroutine(DrawShot(shot));
			ServerAuthShoot(shot);
		}

		[ServerRpc]
		private void ServerAuthShoot(Shot shot) {
			uint currentTick = TimeManager.Tick;
			Vector3 forward = shot.Direction * Vector3.forward;
			//Play sound
			ObserverShoot(shot);

			//TODO
			if(Physics.SphereCast(shot.Origin, caliber/2, forward, out RaycastHit hitInfo, effectiveDistance,
				~LayerMask.GetMask("Player"))) {
				ObserverHit(hitInfo.collider.gameObject.name);
			}
		}

		// Audio-visual queues
		[ObserversRpc(ExcludeOwner = true)]
		private void ObserverShoot(Shot shot) {
			audioSource.PlayOneShot(shotSound);
			StartCoroutine(DrawShot(shot));
		}
		
		// Actual hit (take points of damage, etc)
		[ObserversRpc]
		private void ObserverHit(string name) {
			Debug.Log("I shot " + name);
		}
		
	}
}