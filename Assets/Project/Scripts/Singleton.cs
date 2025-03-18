using UnityEngine;

namespace Guille_dlCH.TFG {
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {

		protected static T _instance;

		public static bool HasInstance => _instance != null;

		public static T Instance {
			get {
				if (_instance == null) {
					_instance = FindFirstObjectByType<T>();
					if (_instance == null) {
						GameObject go = new GameObject(typeof(T).Name);
						_instance = go.AddComponent<T>();
					}
				}
				return _instance;
			}
		}

		protected virtual void Awake() {
			if (_instance != null && _instance != this) {
				Destroy(gameObject);
				return;
			}
			_instance = this as T;
		}
	}
}