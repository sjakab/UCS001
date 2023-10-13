using UnityEngine;

public class EnableForLobbyHost : MonoBehaviour
{
	[SerializeField] private LobbyManager lobbyMgr;
	[SerializeField] private GameObject[] TargetGameObjects;

	[SerializeField] private float lobbyUpdateTimer;
	[SerializeField] private float pollForLobbyUpdatesIntervalInSeconds = 30f;

	// Start is called before the first frame update
	void Start()
    {
		foreach(GameObject go in TargetGameObjects)
		{
			go.SetActive(lobbyMgr.IsLobbyHost());
		}
	}

    // Update is called once per frame
    void Update()
    {
		lobbyUpdateTimer -= Time.deltaTime;
		if (lobbyUpdateTimer < 0f)
		{
			lobbyUpdateTimer = pollForLobbyUpdatesIntervalInSeconds;
			foreach (GameObject go in TargetGameObjects)
			{
				go.SetActive(lobbyMgr.IsLobbyHost());
			}
		}
	}
}
