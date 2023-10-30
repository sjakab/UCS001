using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.PlayerAccounts;
using Unity.Services.Core;

public class UCSAuthenticationManager : MonoBehaviour
{
    enum ESignInMethod : int
    {
        Anonymous = 0,
        UnityAccount
    }

    public SOEvent UserAuthenticatedAndSignedIn;
    public SOEvent UserAuthenticationFailed;
    public SOEvent UserAuthenticationExpired;
    public SOEvent UserSignedOut;
    public SOEvent ContinueToEnterGame;

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

    public async void AuthenticateAsync(int method)
    {
        switch ((ESignInMethod)method)
        {
            case ESignInMethod.Anonymous:
                await OnSignInAnon();
                break;
            case ESignInMethod.UnityAccount:
                await OnSignInUnity();
                break;
            default:
                break;
        }
    }

    public async Task OnSignInAnon()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async Task OnSignInUnity()
    {
        await PlayerAccountService.Instance.StartSignInAsync();
    }

    public void GoToEnterGame()
    {
        ContinueToEnterGame?.TriggerEvent();
    }

    public void SignOut()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut();
        }

        if (PlayerAccountService.Instance.IsSignedIn)
        {
            PlayerAccountService.Instance.SignOut();
        }
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
