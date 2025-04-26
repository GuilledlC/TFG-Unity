using UnityEngine;

namespace Guille_dlCH.TFG {
	
	public static class Utils {
		
		public static void SetLayerRecursively(GameObject obj, int newLayer) {
			// Update the layer of the object itself
			obj.layer = newLayer;

			// Update all children
			foreach (Transform child in obj.transform)
				SetLayerRecursively(child.gameObject, newLayer);
		}
		
	}
	
}