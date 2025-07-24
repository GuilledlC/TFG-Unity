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
			Vector3 delta = transform.forward * (speed * deltaTime);
			if (Physics.SphereCast(transform.position, caliber, transform.position - delta, out RaycastHit hit, delta.magnitude)) {
				Debug.Log(hit.transform.name);
				Despawn(this.gameObject);
				Despawn(tracer.gameObject);
			}
			transform.position += delta;
		}

		public float GetSpeed() {
			return speed;
		}

		public Tracer GetTracer() {
			return tracer;
		}
		
	}
	
}