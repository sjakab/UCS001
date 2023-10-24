using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using WebSocketSharp;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private TextMeshProUGUI displayJoinCode;
    [SerializeField] private TMP_InputField inputJoinCode;
    
    public SOEvent HostJoinGame;
    public SOEvent BackToAuthentication;
    
    public int MaxNumberOfPlayers = 4;

    static public string RelayJoinCode;
    static public Action OnGameServerStarted;
    static public Action OnGameClientStarted;


    private void Start()
    {
        networkManager.OnServerStarted += onStartedRelayGame;
        networkManager.OnClientStarted += OnClientJoinedRelayGame;
    }

    private void OnDestroy()
    {
        networkManager.OnServerStarted -= onStartedRelayGame;
        networkManager.OnClientStarted -= OnClientJoinedRelayGame;

        if (networkManager.IsListening || networkManager.IsClient)
        {
            networkManager.Shutdown();
        }
        
    }

    void onStartedRelayGame()
    {
        OnGameServerStarted?.Invoke();
    }

    void OnClientJoinedRelayGame()
    {
        OnGameClientStarted?.Invoke();
    }

    public void StartHost()
    {
        HostJoinGame?.TriggerEvent();
        StartCoroutine(ConfigureTransportAndStartNgoAsHost());
    }

    public static async Task<RelayServerData> AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
    {
        Allocation allocation;
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay create allocation request failed {e.Message}");
            throw;
        }

        Debug.Log($"server: {allocation.ConnectionData[0].ToString()} {allocation.ConnectionData[1].ToString()}");
        Debug.Log($"server: {allocation.AllocationId.ToString()}");

        try
        {
            RelayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }
        return new RelayServerData(allocation, "dtls");
    }

    public void DisplayJoinCode()
    {
        displayJoinCode.text = $"JoinCode: {RelayJoinCode}";
    }
    
    IEnumerator ConfigureTransportAndStartNgoAsHost()
    {
        var serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(MaxNumberOfPlayers);
        while (!serverRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }
        if (serverRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to start Relay Server. Server not started. Exception: " + serverRelayUtilityTask.Exception.Message);
            yield break;
        }

        var relayServerData = serverRelayUtilityTask.Result;

        // Display the joinCode to the user.
        DisplayJoinCode();
        
        networkManager.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        networkManager.StartHost();
        yield return null;
    }

    public string JoinCode
    {
        set { inputJoinCode.text = RelayJoinCode = value; }
        get
        {
            if (!inputJoinCode.text.IsNullOrEmpty() && 
                (RelayJoinCode.IsNullOrEmpty() || RelayJoinCode == "0"))
            {
                RelayJoinCode = inputJoinCode.text;
            }
            return RelayJoinCode;   
        }
    }
    
    public void StartClient()
    {
        HostJoinGame?.TriggerEvent();
        RelayJoinCode = inputJoinCode.text;
        StartCoroutine(ConfigureTransportAndStartNgoAsConnectingPlayer());
    }

    public static async Task<RelayServerData> JoinRelayServerFromJoinCode(string joinCode)
    {
        JoinAllocation allocation;
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }

        Debug.Log($"client: {allocation.ConnectionData[0].ToString()} {allocation.ConnectionData[1].ToString()}");
        Debug.Log($"host: {allocation.HostConnectionData[0].ToString()} {allocation.HostConnectionData[1].ToString()}");
        Debug.Log($"client: {allocation.AllocationId.ToString()}");

        return new RelayServerData(allocation, "dtls");
    }
    
    IEnumerator ConfigureTransportAndStartNgoAsConnectingPlayer()
    {
        // Populate RelayJoinCode beforehand through the UI
        var clientRelayUtilityTask = JoinRelayServerFromJoinCode(RelayJoinCode);

        while (!clientRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }

        if (clientRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to connect to Relay Server. Exception: " + clientRelayUtilityTask.Exception.Message);
            yield break;
        }

        var relayServerData = clientRelayUtilityTask.Result;

        networkManager.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        networkManager.StartClient();
        yield return null;
    }
    
    public void GoBack()
    {
        BackToAuthentication?.TriggerEvent();
    }
}
