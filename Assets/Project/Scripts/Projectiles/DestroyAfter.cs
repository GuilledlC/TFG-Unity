using System.Collections;
using UnityEngine;
using FishNet.Object;

namespace Guille_dlCH.TFG.Projectiles {

	public class DestroyAfter : NetworkBehaviour {

		[Header("Parameters")] [Tooltip("How much time to destroy this object after it is dropped")] [SerializeField]
		private float destroyAfter = 90f;

		[Tooltip("Whether to destroy it by default")] [SerializeField] private bool destroyByDefault = true;
		private Coroutine destructionCoroutine;
		private float timer;
		private bool isPaused = false;

		public float GetTimer() => timer;
		public bool GetPaused() => isPaused;

		private void Start() {
			if (destroyByDefault)
				StartDestruction();
		}

		public void StartDestruction() {
			if (destructionCoroutine != null)
				StopCoroutine(destructionCoroutine);

			timer = 0f;
			isPaused = false;
			destructionCoroutine = StartCoroutine(DestroyAfterDelayCoroutine());
		}

		public void StopDestruction() {
			isPaused = true;
			StopCoroutine(destructionCoroutine);
		}

		private IEnumerator DestroyAfterDelayCoroutine() {
			while (timer < destroyAfter) {
				if (!isPaused) {
					timer += Time.deltaTime;
				}

				yield return null;
			}

#if UNITY_EDITOR
			//Debug.Log(TimeManager.Tick);
#endif
			Destroy(gameObject);
		}

	}

}
