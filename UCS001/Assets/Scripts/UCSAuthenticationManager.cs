using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.PlayerAccounts;
using Unity.Services.Core;

public class UCSAuthenticationManager : MonoBehaviour
{
    public Event UserAuthenticated;
    public Event UserAuthenticationFailed;
    public Event UserAuthenticationExpired;
    public Event UserSignedOut;
    public Event GoToEnterGame;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async void Initialize()
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


}
