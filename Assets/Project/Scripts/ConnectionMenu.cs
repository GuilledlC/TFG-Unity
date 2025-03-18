using UnityEngine;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using TMPro;
using UnityEngine.UI;

namespace Guille_dlCH.TFG {

	public class ConnectionMenu : ShowableMenu {

		[SerializeField] private TMP_InputField ipAddress;
		[SerializeField] private Button hostButton;
		[SerializeField] private Button clientButton;

		protected override void Awake() {
			base.Awake();
			ipAddress.text = "localhost";
			hostButton.onClick.AddListener(() => {
				InstanceFinder.TransportManager.Transport.SetClientAddress("localhost");
				InstanceFinder.ServerManager.StartConnection();
				JoinServer();

			});
			clientButton.onClick.AddListener(() => {
				InstanceFinder.TransportManager.Transport.SetClientAddress(ipAddress.text);
				JoinServer();
			});
		}

		private void JoinServer() {
			InstanceFinder.ClientManager.StartConnection();
			Hide();
		}
	}
}
