using UnityEngine;

namespace Guille_dlCH.TFG.UI {
	
	public class ShowableMenu : ShowableUI {
		
		[SerializeField] protected bool hideOnStart = true;
		[SerializeField] protected ShowableMenu parent;
		
		protected override void Awake() {
			base.Awake();
			
			GameEvents.OnMenuBack += GoBack;
			if(hideOnStart)
				Hide();
		}
		
		public override void Show() {
			base.Show();
			GameEvents.OnMenuBack += GoBack;
		}
		
		public override void Hide() {
			base.Hide();
			GameEvents.OnMenuBack -= GoBack;
		}

		public virtual void GoBack() {
			parent?.Show();
			Hide();
		}
		
	}
}