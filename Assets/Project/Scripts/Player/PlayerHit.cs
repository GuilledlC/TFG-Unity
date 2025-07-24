using UnityEngine;

namespace Guille_dlCH.TFG.Player {
	public class PlayerHit : MonoBehaviour {
		
		[Header("Sound")]
		[SerializeField] private AudioSource audioSource;
		[SerializeField] private AudioClip hitSound;

		public void GetHit() {
			if (audioSource != null && hitSound != null) {
				audioSource.PlayOneShot(hitSound);
			}
		}

	}
}