using System.Collections;
using FishNet.Component.Animating;
using UnityEngine;
using FishNet.Object;
using UnityEngine.Rendering;

namespace Guille_dlCH.TFG.Player {
	
	public class PlayerAnimator : NetworkBehaviour {

		[SerializeField] private Animator playerAnimator;
		[SerializeField] private NetworkAnimator networkAnimator;
		[SerializeField] private SkinnedMeshRenderer playerModel;
		[SerializeField] private Animator gunAnimator;
		[SerializeField] private SkinnedMeshRenderer gunModel;

		private PlayerCharacter playerCharacter;

		private static readonly int Speed = Animator.StringToHash("Speed");
		private static readonly int Shooting = Animator.StringToHash("Shooting");
		
		//For when base.IsOwner()
		public void Initialize(PlayerCharacter pc) {
			playerCharacter = pc;
			//So we don't show the player model to the owner
			playerModel.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
		}

		//For when !base.IsOwner()
		public void Disable() {
			gunModel.enabled = false;
			this.enabled = false;
		}
		
		private void Update() {
			playerAnimator.SetFloat(Speed, playerCharacter.GetSpeed());
			//gunAnimator.SetFloat(Speed, playerCharacter.GetSpeed());
		}

		public void Shoot() {
			networkAnimator.SetTrigger(Shooting);
			//playerAnimator.SetTrigger(Shooting);
			gunAnimator.SetTrigger(Shooting);
		}
		
	}
	
}