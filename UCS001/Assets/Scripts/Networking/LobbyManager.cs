using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
	// Reference used for implementing Lobby with Relay
	// and for referencing the MaxNumberOfPlayers variable.
	[SerializeField] private RelayManager relayManager;
	public RelayManager RelayManager => relayManager;

	public Unity.Services.Lobbies.Models.Lobby CreatedOrJoinedLobby;

	private bool keepLobbyActive;
	[SerializeField] private float heartbeatIntervalInSeconds = 30.0f;
	private float lobbyUpdateTimer;
	[SerializeField] private float pollForLobbyUpdatesIntervalInSeconds = 3.0f;

	public List<Lobby> QueriedLobbies;

	public List<Player> PlayersInLobby {
		get {
				return CreatedOrJoinedLobby.Players; 
			}  
		}

	[SerializeField] private string AssignedPlayerName;
	private string[] PlayerColors;
	public string AssignedPlayerColor;
	private string PlayerId;

	private string LobbyName;

	[SerializeField] private SOEvent _OnHostExitGameEvent;
	[SerializeField] private SOEvent _OnClientExitGameEvent;

	// Methods
	// ///////////////////////////////////////////////////////
	#region methods

	private void Start()
	{
		// Index for player colors.
		PlayerColors = new string[] { "Red", "Blue", "Yellow", "Green" };
		LobbyRecordConfigurator.onJoinLobby += JoinLobbyById;
		QueriedLobbies = new List<Lobby>();
		// Assign Temporary Player Data - this will be assigned first 
		AssignedPlayerName = "TEMPPLAYER";
		AssignedPlayerColor = "Red";
		lobbyUpdateTimer = pollForLobbyUpdatesIntervalInSeconds;
	}

	private void Update()
	{
		LobbyPollForUpdates();
	}

	private void OnDestroy()
	{
		if (CreatedOrJoinedLobby != null)
		{
			LeaveLobby();
		}
	}

	public bool IsLobbyHost()
	{
		if (CreatedOrJoinedLobby != null)
		{
			Debug.Log($"!!!!!!!!!!!!!!!! Lobby is not null: {CreatedOrJoinedLobby != null}  HostId Is {CreatedOrJoinedLobby.HostId} Player Id is {AuthenticationService.Instance.PlayerId} returning { CreatedOrJoinedLobby != null && CreatedOrJoinedLobby.HostId == AuthenticationService.Instance.PlayerId}");
			return CreatedOrJoinedLobby != null && CreatedOrJoinedLobby.HostId == AuthenticationService.Instance.PlayerId;
		}
		return false;
	}

	public string GetCurrentLobbyName()
	{ 
		if (CreatedOrJoinedLobby != null)
		{
			Debug.Log($"{CreatedOrJoinedLobby.Name}");
			LobbyName = CreatedOrJoinedLobby.Name;
		}
		return LobbyName;
	}

	public string GetCurrentPlayerName()
	{
		Debug.Log($"{AssignedPlayerName}");
		return AssignedPlayerName;
	}

	public void StartGame()
	{
			if (IsLobbyHost())
			{
				NetworkManager.Singleton.StartHost();
			}
			else
			{
				NetworkManager.Singleton.StartClient();
			}
	}

	public void ExitGame()
	{
		var networkManager = NetworkManager.Singleton;

		if (networkManager.IsHost)
		{
			NetworkManager.Singleton.Shutdown();
			relayManager.JoinCode = "0";
			_OnHostExitGameEvent?.TriggerEvent();
		}
		else if (networkManager.IsClient)
		{
			NetworkManager.Singleton.OnClientStopped -= OnClientDisconnected;
			NetworkManager.Singleton.Shutdown();
			relayManager.JoinCode = "0";
			LeaveLobby();
			_OnClientExitGameEvent?.TriggerEvent();
		}
	}

	void OnClientDisconnected(bool bIsHost)
	{
		NetworkManager.Singleton.OnClientStopped -= OnClientDisconnected;
		
		//Host exited the game and the client was disconnected
		//Call _OnHostExitGameEvent to go to the PlayerList UI in the lobby 
		_OnHostExitGameEvent?.TriggerEvent();	
	}

	public async void CreateLobby(string lobbyName, string gameMode)
	{
		Debug.Log("TESTING CREATE LOBBBY!");
		try
		{
			// Player creating a lobby is always the first so assign them the data for player 0.
			AssignPlayerData(0);

			// Set Lobby Options to Setup Player Data.
			CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
			{
				Player = CreateNewPlayer(),
				Data = new Dictionary<string, DataObject>
				{
					{"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) },
					{"StartCode", new DataObject(DataObject.VisibilityOptions.Member, "0") }
				}
			};

			// Create a Lobby.
			LobbyName = lobbyName;
			CreatedOrJoinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, relayManager.MaxNumberOfPlayers, createLobbyOptions);

			// Set flag to instruct heartbeat Coroutine that it should continue to "ping" the Lobby every X seconds.  
			keepLobbyActive = true;

			// Lobby KeepAlive - Starts a Coroutine used to "ping" the Lobby every X seconds, otherwise the Lobby
			// will automatically become inactive and undiscoverable for future players.
			StartCoroutine(HeartbeatLobbyCoroutine(CreatedOrJoinedLobby.Id, heartbeatIntervalInSeconds));
			
			Debug.Log($"Created Lobby! {CreatedOrJoinedLobby.Name} {CreatedOrJoinedLobby.MaxPlayers}");
		}
		catch (LobbyServiceException lse)
		{
			lobbyName = null;
			Debug.Log(lse);
		}
	}

	public async void CreatePrivateLobby(string lobbyName, string gameMode)
	{
		try
		{
			// Player creating a lobby is always the first so assign them the data for player 0.
			AssignPlayerData(0);

			// Set Lobby Options to Setup Player Data and Create Lobby as Private.
			CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
			{
				IsPrivate = true,
				Player = CreateNewPlayer(),
				Data = new Dictionary<string, DataObject>
				{
					{"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) },
					{"StartCode", new DataObject(DataObject.VisibilityOptions.Member, "0") }
				}
			};

			// Create a Lobby, passing in the CreateLobbyOptions to set it to private..
			CreatedOrJoinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, relayManager.MaxNumberOfPlayers, createLobbyOptions);

			// Set flag to instruct heartbeat Coroutine that it should continue to "ping" the Lobby every X seconds.  
			keepLobbyActive = true;

			// Lobby KeepAlive - Starts a Coroutine used to "ping" the Lobby every X seconds, otherwise the Lobby
			// will automatically become inactive and undiscoverable for future players.
			StartCoroutine(HeartbeatLobbyCoroutine(CreatedOrJoinedLobby.Id, heartbeatIntervalInSeconds));

			Debug.Log($"Created Lobby! {CreatedOrJoinedLobby.Name} {CreatedOrJoinedLobby.MaxPlayers.ToString()} {CreatedOrJoinedLobby.LobbyCode}");
		}
		catch (LobbyServiceException lse)
		{
			Debug.Log(lse);
		}
	}



	// ContextMenu attributes used so the method can be tested through the inspector.
	[ContextMenu("ListLobbies")]
	public async Task ListLobbies()
	{
		try
		{
			// Query for all available lobbies.
			QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

			Debug.Log($"Lobbies found: {queryResponse.Results.Count} {queryResponse.Results} ");

			QueriedLobbies = queryResponse.Results;

			foreach (Lobby lobby in QueriedLobbies)
			{
				Debug.Log($"{lobby.Name} {lobby.MaxPlayers} {lobby.Data["GameMode"].Value} ");
			}

		}
		catch (LobbyServiceException lse)
		{
			Debug.Log(lse);
		}
	}

	[ContextMenu("ListLobbiesWithSlots")]
	private async Task ListLobbiesWithSlots()
	{
		try
		{
			QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
			{
				Filters = new List<QueryFilter>
				{
					new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
				},
				Order = new List<QueryOrder>
				{
					new QueryOrder(false, QueryOrder.FieldOptions.AvailableSlots)
				}
			};

			// Query for all available lobbies.
			QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

			Debug.Log($"Lobbies found: {queryResponse.Results.Count} {queryResponse.Results} ");

			QueriedLobbies = queryResponse.Results;

			foreach (Lobby lobby in queryResponse.Results)
			{
				Debug.Log($"{lobby.Name} {lobby.MaxPlayers} {lobby.AvailableSlots} {lobby.Data["GameMode"].Value}  ");
			}

		}
		catch (LobbyServiceException lse)
		{
			Debug.Log(lse);
		}
	}

	private async void JoinLobbyById(string lobbyId)
	{
		Debug.Log("Called Join Lobby");
		try
		{
			// Set Join Lobby Options to Setup Player Data.
			JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
			{
				Player = CreateNewPlayer()
			};

			CreatedOrJoinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyByIdOptions);
		}
		catch (LobbyServiceException lse)
		{
			Debug.LogError(lse);
		}
	}

	[ContextMenu("Join First Lobby From Search")]
	private async void JoinFirstLobbyFromSearch()
	{
		// Fetch all lobbies with slots ordered by most slots available descending.
		await ListLobbiesWithSlots();

		if (QueriedLobbies.Count > 0)
		{
			// Fetch the id for the lobby with the most available slots. 
			string lobbyIdToJoin = QueriedLobbies[0].Id;

			// Call join lobby passing the lobby id with the most slots.
			JoinLobbyById(lobbyIdToJoin);
		}
		else
		{
			Debug.Log($"Join First Lobby From Search Error - No Lobbies with slots available.");
		}
	}

	private async void JoinLobbyByCode(string lobbyCode)
	{
		try
		{
			// Set Join Lobby By Code Options to Setup Player Data.
			JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
			{
				Player = CreateNewPlayer()
			};


			CreatedOrJoinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
		}
		catch (LobbyServiceException lse)
		{
			Debug.Log(lse);
		}
	}

	[ContextMenu("Quick Join Lobby ")]
	private async void QuickJoinLobby()
	{
		try
		{
			// Set QuickJoin Lobby Options to Setup Player Data.
			QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
			{
				Player = CreateNewPlayer()
			};

			CreatedOrJoinedLobby = await Lobbies.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
			Debug.Log("LOBBY QUICK JOIN SUCCESSFUL!!");
		}
		catch (LobbyServiceException lse)
		{
			Debug.Log(lse);
		}
	}


	private void PrintPlayersInLobby(Lobby lobby)
	{
		Debug.Log($"{lobby.Players.Count} PLAYERS IN LOBBY {lobby.Name} GAMEMODE: {lobby.Data["GameMode"].Value}");
		foreach (Player player in PlayersInLobby)
		{
			Debug.Log($"{player.Id} - NAME: {player.Data["PlayerName"].Value}, COLOR: {player.Data["PlayerColor"].Value}.");
		}
	}

	public List<Player> GetPlayersInCurrentLobby()
	{
		return CreatedOrJoinedLobby.Players;
	}

	public async void UpdatePlayerNameAndColor(string newName, string newColor)
	{
		try
		{
			UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions
			{
				Data = new Dictionary<string, PlayerDataObject>
						{
							{"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newName)},
							{"PlayerColor", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newColor)},
						}
			};

			CreatedOrJoinedLobby = await LobbyService.Instance.UpdatePlayerAsync(CreatedOrJoinedLobby.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);
			AssignedPlayerColor = newColor;
			AssignedPlayerName = newName;
		}
		catch (LobbyServiceException lse)
		{
			Debug.Log(lse);
		}

	}

	public async void UpdateLobbyGameMode(string newGameMode)
	{
		try
		{
			UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
			{
				Data = new Dictionary<string, DataObject>
						{
							{"GameMode", new DataObject(DataObject.VisibilityOptions.Public, newGameMode)}
						}
			};

			CreatedOrJoinedLobby = await LobbyService.Instance.UpdateLobbyAsync(CreatedOrJoinedLobby.Id, updateLobbyOptions);
		}
		catch (LobbyServiceException lse)
		{
			Debug.Log(lse);
		}
	}

	// Coroutine to send KeepAlive heartbeat to prevent the lobby becoming inactive after 30 seconds.
	private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
	{
		var delay = new WaitForSecondsRealtime(waitTimeSeconds);
		while (keepLobbyActive)
		{
			Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
			Debug.Log("Lobby Heartbeat - BEEP!!!!");
			yield return delay;
		}
	}

	private async void LobbyPollForUpdates()
	{
		lobbyUpdateTimer -= Time.deltaTime;
		if (lobbyUpdateTimer < 0f)
		{
			lobbyUpdateTimer = pollForLobbyUpdatesIntervalInSeconds;

			if (CreatedOrJoinedLobby != null)
			{
				 Lobby lobby = await Lobbies.Instance.GetLobbyAsync(CreatedOrJoinedLobby.Id);
				 CreatedOrJoinedLobby = lobby; 
				Debug.Log("Lobby Poll For Updates - BIPP!!!!");

				if (CreatedOrJoinedLobby.Data.ContainsKey("StartCode"))
				{
					var joinCode = CreatedOrJoinedLobby.Data["StartCode"].Value;
					Debug.Log($"POLL ----- START CODE  {joinCode}");
					Debug.Log(
						$"POLL ----- IS START CODE CHANGED FROM ZERO?? {(joinCode != "0").ToString()}");

					Debug.Log($"POLL ----- JOIN CODE IN RELAY MGR  {relayManager.JoinCode}");

					bool hasJoinCode = (joinCode != "0" && relayManager.JoinCode != joinCode); 
					Debug.Log($"POLL ----- IS JOIN CODE IN RELAY SAME AS START CODE?? {hasJoinCode.ToString()}");

					if (hasJoinCode && !IsLobbyHost())  // Only run for joined clients automatically.
					{
						relayManager.JoinCode = joinCode;
						relayManager.StartClient();		// START AND JOIN THE GAME
						NetworkManager.Singleton.OnClientStopped += OnClientDisconnected;
					}
				}
			}

			await ListLobbies();
		}
	}

	[ContextMenu("LeaveLobby")]
	public async void LeaveLobby()
	{
		try
		{
			await LobbyService.Instance.RemovePlayerAsync(CreatedOrJoinedLobby.Id, AuthenticationService.Instance.PlayerId);
			CreatedOrJoinedLobby = null;
		}
		catch (LobbyServiceException lse)
		{
			Debug.Log(lse);
		}
	}


	public async void UpdateLobbyRelayJoinCode()
	{
		Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(CreatedOrJoinedLobby.Id, new UpdateLobbyOptions
		{
			Data = new Dictionary<string, DataObject>
			{
				{"StartCode", new DataObject(DataObject.VisibilityOptions.Member, relayManager.JoinCode) }
			}
		});

		CreatedOrJoinedLobby = lobby;
	}
	
	public async void StartRelayLobbyGame()
	{
		if (IsLobbyHost())
		{
			try
			{
				Debug.Log("Start Game");
				relayManager.StartHost();
			}
			catch (LobbyServiceException e)
			{
				Debug.LogError(e);
			}
		}
	}

	private async void KickPlayer(string playerId)
	{
		try
		{
			await LobbyService.Instance.RemovePlayerAsync(CreatedOrJoinedLobby.Id, playerId);
			if (AuthenticationService.Instance.PlayerId == playerId)
			{
				CreatedOrJoinedLobby = null;
			}
		}
		catch (LobbyServiceException lse)
		{
			Debug.Log(lse);
		}
	}

	public async void MigrateLobbyHost(string newHostId)
	{
		try
		{
			UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
			{
				HostId = newHostId
			};

			CreatedOrJoinedLobby = await LobbyService.Instance.UpdateLobbyAsync(CreatedOrJoinedLobby.Id, updateLobbyOptions);
		}
		catch (LobbyServiceException lse)
		{
			Debug.Log(lse);
		}
	}

	private async void DeleteLobby(string lobbyId)
	{
		try
		{
			await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
			if (CreatedOrJoinedLobby.Id == lobbyId)
			{
				CreatedOrJoinedLobby = null;
			}
		}
		catch (LobbyServiceException lse)
		{
			Debug.Log(lse);
		}
	}

	private Player CreateNewPlayer()
	{
		Debug.Log($"CREATING NEW PLAYER NAME{AssignedPlayerName} COLOR{AssignedPlayerColor}");
		return new Player
		{
			Data = new Dictionary<string, PlayerDataObject>
					{
						{"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AssignedPlayerName)},
						{"PlayerColor", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AssignedPlayerColor)},
					}
		};
	}

	public void AssignPlayerData(int playerIndex)
	{
		// Player creating a lobby is always the first so assign them the data for player 0.
		AssignedPlayerName = $"PLAYER{playerIndex.ToString()}";
		AssignedPlayerColor = PlayerColors[playerIndex];
	}

	public int GetLobbyIndexOfCurrentPlayer()
	{
		int playerIndex = -1;
		foreach (Player player in PlayersInLobby)
		{
			playerIndex++;
			// If this is matched the current player id return their index from the list of players.
			if (player.Id.Equals(AuthenticationService.Instance.PlayerId))
			{
				return playerIndex;
			}
		}
		return playerIndex;
	}
	#endregion

	// Context Menu - Proxy Test Functions
	// ///////////////////////////////////////////////////////
	#region Context Menu - Proxy Test Functions
	// ContextMenu attributes used so the method can be tested through the inspector.
	[ContextMenu("Test - CreateLobbyA")]
	public void TestCreateLobbyA()
	{
		CreateLobby("MyFirstLobby", "Royale");
	}

	[ContextMenu("Test - CreateLobbyB")]
	private void TestCreateLobbyB()
	{
		CreateLobby("MySecondLobby", "Capture The Flag");
	}

	[ContextMenu("Test - CreatePrivateLobby")]
	private void TestCreatePrivateLobby()
	{
		CreatePrivateLobby("MyPRIVATELobby", "Territory");
	}

	[ContextMenu("Test - Print Players Info")]
	private void PrintPlayersInfo()
	{
		this.PrintPlayersInLobby(CreatedOrJoinedLobby);
	}

	[ContextMenu("Test - Update Players Info")]
	private void TestUpdatePlayerNameAndColor()
	{
		// GET INDEX OF PLAYER
		int lobbyIndexOfPlayer = GetLobbyIndexOfCurrentPlayer();
		if (lobbyIndexOfPlayer == -1)
		{
			Debug.Log("Error - Unable to update player name and color. No index found for player in the lobby list");
		}
		else
		{
			AssignPlayerData(lobbyIndexOfPlayer);
			UpdatePlayerNameAndColor(AssignedPlayerName, AssignedPlayerColor);
		}
	}

	[ContextMenu("Test - Update Lobby Game Mode")]
	private void TestUpdateLobbyGameMode()
	{
		UpdateLobbyGameMode("Last Player Standing");
	}

	[ContextMenu("Test - Kick Player")]
	private void TestKickPlayer()
	{
		string playerToKick = CreatedOrJoinedLobby.Players[CreatedOrJoinedLobby.Players.Count - 1].Id;
		KickPlayer(playerToKick);
	}

	[ContextMenu("Test - Migrate Lobby Host")]
	private void TestMigrateLobbyHost()
	{
		string playerToKick = CreatedOrJoinedLobby.Players[CreatedOrJoinedLobby.Players.Count - 1].Id;
		MigrateLobbyHost(playerToKick);
	}

	[ContextMenu("Test - Delete Lobby")]
	private void TestDeleteLobby()
	{
		DeleteLobby(CreatedOrJoinedLobby.Id);
	}
	#endregion
}
