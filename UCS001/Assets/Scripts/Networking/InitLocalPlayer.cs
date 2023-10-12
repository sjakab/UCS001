using UnityEngine;
using Unity.Netcode;

public class InitLocalPlayer : MonoBehaviour
{
    [SerializeField] private NetworkObject networkObj;
    [SerializeField] private CharacterController charController;
    [SerializeField] private GameObject[] localPlayerGOsToEnable;
    [SerializeField] private MonoBehaviour[] localPlayerComponentsToEnable;
    [SerializeField] private GameObject[] localPlayerGOsToDisable;
    [SerializeField] private MonoBehaviour[] localPlayerComponentsToDisable;

    void Start()
    {
        // Disable GOs
        if (networkObj.IsLocalPlayer)
        {
            charController.enabled = true;

            if (localPlayerGOsToEnable.Length > 0)
            {
                for (int i = 0; i < localPlayerGOsToEnable.Length; i++)
                {
                    localPlayerGOsToEnable[i].SetActive(true);
                }
            }
            // Disable Components
            if (localPlayerComponentsToEnable.Length > 0)
            {
                for (int i = 0; i < localPlayerComponentsToEnable.Length; i++)
                {
                    localPlayerComponentsToEnable[i].enabled = true;
                }
            }
            // Disable GOs
            if (localPlayerGOsToDisable.Length > 0)
            {
                for (int i = 0; i < localPlayerGOsToDisable.Length; i++)
                {
                    localPlayerGOsToDisable[i].SetActive(false);
                }
            }
            // Disable Components
            if (localPlayerComponentsToDisable.Length > 0)
            {
                for (int i = 0; i < localPlayerComponentsToDisable.Length; i++)
                {
                    localPlayerComponentsToDisable[i].enabled = false;
                }
            }
        }        
    }
}
