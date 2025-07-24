using UnityEngine;

namespace Guille_dlCH.TFG.UI {
	
	[RequireComponent(typeof(CanvasGroup))]	
	public class ShowableUI : MonoBehaviour {
		
		[SerializeField] protected CanvasGroup canvasGroup;
		protected bool hidden = false;

		protected virtual void Awake() {
			// Auto-get the component if not assigned
			if (!TryGetComponent(out canvasGroup))
				canvasGroup = gameObject.AddComponent<CanvasGroup>();
			else
				canvasGroup = GetComponent<CanvasGroup>();
		}
		
		public virtual void Show() {
			//Show the canvas using the CanvasGroup
			canvasGroup.alpha = 1;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
			hidden = false;
		}
		
		public virtual void Hide() {
			//Hide the canvas using the CanvasGroup
			canvasGroup.alpha = 0;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
			hidden = true;
		}

		public bool IsHidden() => hidden;
	}
}