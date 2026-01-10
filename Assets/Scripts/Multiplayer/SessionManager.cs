using UnityEngine;
using Unity.Services.Core;
using System;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

public class SessionManager : MonoBehaviour
{
    public event EventHandler<OnCreateJoinCodeArgs> OnCreateJoinCode;

    public class OnCreateJoinCodeArgs : EventArgs
    {
        public string JoinCode;
    }

    private void CreateJoinCode(string joinCode)
    {
        OnCreateJoinCode?.Invoke(this, new OnCreateJoinCodeArgs { JoinCode = joinCode });
    }

    private async void Start()
    {

        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async void StartSessionAsHost()
    {
        try
        {
            var options = new SessionOptions
            {
                MaxPlayers = 2
            }.WithRelayNetwork(); // or WithDistributedAuthorityNetwork() to use Distributed Authority instead of Relay
            var session = await MultiplayerService.Instance.CreateSessionAsync(options);
            Debug.Log($"Session {session.Id} created! Join code: {session.Code}");
            CreateJoinCode(session.Code);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async void JoinSessionAsClient(string joinCode)
    {
        try
        {
            await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}