namespace Guille_dlCH.TFG {
	
	public static class GameEvents {

		public delegate void GameStateChange();
		public static event GameStateChange OnGamePaused;
		public static event GameStateChange OnMenuBack;
		public static event GameStateChange OnGameResumed;

		public static void TriggerGamePaused()
			=> OnGamePaused?.Invoke();
		
		public static void TriggerMenuBack()
			=> OnMenuBack?.Invoke();
		
		public static void TriggerGameResumed()
			=> OnGameResumed?.Invoke();

	}
	
}