using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class EnableForLobbyHost : MonoBehaviour
{
	[SerializeField] private LobbyManager lobbyManager;
	[SerializeField] private GameObject[] targetGameObjects;
	
	private void OnEnable()
	{
		StartCoroutine(SetLobbyHost());
	}

	IEnumerator SetLobbyHost()
	{
		while (gameObject.activeSelf)
		{
			bool isHost = lobbyManager.IsLobbyHost();
			if (isHost)
			{
				foreach (GameObject go in targetGameObjects)
				{
					go.SetActive(isHost);
				}
				break;
			}
			yield return null;
		}
	}
}
