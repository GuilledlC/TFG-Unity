using UnityEngine;

namespace Guille_dlCH.TFG.Projectiles {
	
	public class Tracer : MonoBehaviour {
		
		[SerializeField] private Projectile bullet;
		
		private float speed;
		private Vector3 startPos;
		
		private float distanceTraveled = 0f;
		private float catchupDistance = 0.2f;
	
		private void Update() {
			Move(Time.deltaTime);
		}

		public void Initialize(Projectile bulletToFollow) {
			bullet = bulletToFollow;
			speed = bullet.GetSpeed();
			startPos = transform.position;
		}
		
		private void Move(float deltaTime) {
			try {
				distanceTraveled += speed * deltaTime;
				float t = Mathf.Clamp01(distanceTraveled / catchupDistance);
            
				transform.position = Vector3.Lerp(startPos, bullet.transform.position, t);
            
			} catch (MissingReferenceException) {
				Destroy(gameObject);
			}
		}
		
	}
	
}