using UnityEngine;
using FishNet.Object;

namespace Guille_dlCH.TFG.Projectiles {
	
	[RequireComponent(typeof(DestroyAfter))]
	public class Projectile : NetworkBehaviour {
		
		[SerializeField] private float speed;
		[SerializeField] private float caliber;
		[SerializeField] private Tracer tracer;
		
		private void Update() {
			Move(Time.deltaTime);
		}

		public void Initialize(Vector3 cameraPosition, Vector3 startPosition) {
			Vector3 direction = startPosition - cameraPosition;
			bool hit = Physics.SphereCast(cameraPosition, caliber, direction.normalized, out RaycastHit deathZone, direction.magnitude);
			if (hit) {
				//Kill player or harm prop
			}
		}
		
		private void Move(float deltaTime) {
			transform.position += transform.forward * (speed * deltaTime);
		}

		public float GetSpeed() {
			return speed;
		}

		public Tracer GetTracer() {
			return tracer;
		}
		
	}
	
}