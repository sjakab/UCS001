using UnityEngine;
using Unity.Netcode;

public class NetworkingManager : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager; 

    public SOEvent HostJoinGame;
    public SOEvent BackToAuthentication;
    
    public void StartHost()
    {
        HostJoinGame?.TriggerEvent();
        _networkManager.StartHost();
    }

    public void StartClient()
    {
        HostJoinGame?.TriggerEvent();
        _networkManager.StartClient();
    }

    public void GoBack()
    {
        BackToAuthentication?.TriggerEvent();
    }
}
