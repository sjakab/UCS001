using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.PlayerAccounts;
using Unity.Services.Core;

public class UCSAuthenticationManager : MonoBehaviour
{
    public SOEvent UserAuthenticatedAndSignedIn;
    public SOEvent UserAuthenticationFailed;
    public SOEvent UserAuthenticationExpired;
    public SOEvent UserSignedOut;
    public SOEvent GoToEnterGame;

    // Start is called before the first frame update
     async void Start()
    {
        await Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async Task Initialize()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += handleSignIn;
        AuthenticationService.Instance.SignInFailed += handleSignInFailure;
        AuthenticationService.Instance.SignedOut += handleSignOut;
        AuthenticationService.Instance.Expired += handleAuthExpired;
    }

    public async void OnSignInAnon()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void OnSignInUnity()
    {
        await PlayerAccountService.Instance.StartSignInAsync();
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
