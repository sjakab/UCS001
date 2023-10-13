using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerRecordConfigurator : MonoBehaviour
{
	//public delegate void OnKickPlayerUI(string lobbyId);
	//public static OnKickPlayerUI onKickPlayer;
	//[SerializeField] private Button kickPlayerButton;
	[SerializeField] private TMPro.TMP_Text playerName;
	[SerializeField] private TMPro.TMP_Text playerColor;
	private string playerId;

	public TMP_Text PlayerName { get => playerName; set => playerName = value; }
	public TMP_Text PlayerColor { get => playerColor; set => playerColor = value; }
	public string PlayerId { get => playerId; set => playerId = value; }

	//private void Start()
	//{
	//	joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);
	//}

	//private void OnJoinLobbyClicked()
	//{
	//	Debug.Log("ON JOIN LOBBY CLICKED");
	//	onJoinLobby.Invoke(lobbyId);
	//}
}
