using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreateLobbyUIManager : MonoBehaviour
{
	[SerializeField] private LobbyManager lobbyManager;
	[SerializeField] private Button createLobbyBtn;
	[SerializeField] private TMP_InputField lobbyNameInputField;
	[SerializeField] private TMP_InputField gameModeInputField;

	// Start is called before the first frame update
	void Start()
    {
		createLobbyBtn.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
		CheckForPopulatedTextField();
    }

	public void CheckForPopulatedTextField()
	{
		if (string.IsNullOrWhiteSpace(lobbyNameInputField.text) || string.IsNullOrWhiteSpace(gameModeInputField.text) )
		{
			createLobbyBtn.interactable = false;
		}
		else
		{
			createLobbyBtn.interactable = true;
		}
	}

	public void CreateNewLobby()
	{
		createLobbyBtn.interactable = false;
		lobbyManager.CreateLobby(lobbyNameInputField.text, gameModeInputField.text);
		lobbyNameInputField.text = "";
		gameModeInputField.text = "";
	}
}
