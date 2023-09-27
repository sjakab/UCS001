using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;


public class UCSAuthenticationManager : MonoBehaviour
{
	enum ESignInMethod : int
	{
		Anonymous = 0,
		UnityAccount
	}
	
	public SOEvent UserAuthenticatedAndSignedIn	;
	public SOEvent UserAuthenticationFailed;
	public SOEvent UserAuthenticationExpired;
	public SOEvent UserSignedOut;

	public async void InitializeAndAuthenticateAsync(int method)
	{
		await InitializeAsync();
		
		switch((ESignInMethod)method)
		{
		case ESignInMethod.Anonymous:
			await SignInAnonymouslyAsync();
			break;
		case ESignInMethod.UnityAccount:
			await SignInUnityAccountAsync();
			break;
		default:
			break;
		}
	}

	// Async task to initialize Unity 
	public async Task InitializeAsync()
	{
		try
		{
			await UnityServices.InitializeAsync();
			SetupEvents();
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}

	async Task SignInAnonymouslyAsync()
	{
		try
		{
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
			Debug.Log("Sign in anonymously succeeded!");

			// Shows how to get the playerID
			Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

		}
		catch (AuthenticationException ex)
		{
			// Compare error code to AuthenticationErrorCodes
			// Notify the player with the proper error message
			Debug.LogException(ex);
		}
		catch (RequestFailedException ex)
		{
			// Compare error code to CommonErrorCodes
			// Notify the player with the proper error message
			Debug.LogException(ex);
		}
	}

	async Task SignInUnityAccountAsync()
	{
		try
		{
			
		}
	}

	// Setup authentication event handlers if desired
	void SetupEvents()
	{
		AuthenticationService.Instance.SignedIn += handleSignIn;

		AuthenticationService.Instance.SignInFailed += handleSignInFailure;

		AuthenticationService.Instance.SignedOut += handleSignOut;

		AuthenticationService.Instance.Expired += handleAuthExpired;
	}

	private void handleSignIn()
	{
		// Fire event for user authenticated.
		UserAuthenticatedAndSignedIn?.TriggerEvent();

		// Shows how to get a playerID
		Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

		// Shows how to get an access token
		Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
	}

	private void handleSignInFailure(RequestFailedException err)
	{
		UserAuthenticationFailed?.TriggerEvent();
		Debug.LogError(err);
	}

	private void handleSignOut()
	{
		UserSignedOut?.TriggerEvent();
	}

	private void handleAuthExpired()
	{
		UserAuthenticationExpired?.TriggerEvent();
	}
}
