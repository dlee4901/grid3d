using System;
using Steamworks;

public sealed class SteamMatchService : IDisposable
{
    private const int MaxMembers = 2;

    private readonly CallResult<LobbyCreated_t> _created;
    private readonly CallResult<LobbyEnter_t> _entered;
    private readonly Callback<LobbyChatUpdate_t> _memberChanged;
    private readonly Callback<GameLobbyJoinRequested_t> _joinRequested;
    private CSteamID _lobby;

    public string LocalPlayerId => SteamUser.GetSteamID().m_SteamID.ToString();
    public string LobbyCode => _lobby.IsValid() ? _lobby.m_SteamID.ToString() : null;
    public bool IsHost { get; private set; }

    public event Action<string> LobbyReady;
    public event Action<SteamPeerLink, bool> OpponentConnected;
    public event Action<string> OpponentLeft;
    public event Action<string> Error;

    public SteamMatchService()
    {
        _created = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
        _entered = CallResult<LobbyEnter_t>.Create(OnLobbyEntered);
        _memberChanged = Callback<LobbyChatUpdate_t>.Create(OnMemberChanged);
        _joinRequested = Callback<GameLobbyJoinRequested_t>.Create(request => Join(request.m_steamIDLobby));
    }

    public void Host()
    {
        IsHost = true;
        _created.Set(SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, MaxMembers));
    }

    public void Join(string code)
    {
        if (ulong.TryParse(code?.Trim(), out var id)) Join(new CSteamID(id));
        else Error?.Invoke($"'{code}' is not a lobby code");
    }

    public void InviteFriend()
    {
        if (_lobby.IsValid()) SteamFriends.ActivateGameOverlayInviteDialog(_lobby);
    }

    public void Dispose()
    {
        if (_lobby.IsValid()) SteamMatchmaking.LeaveLobby(_lobby);
        _created.Dispose();
        _entered.Dispose();
        _memberChanged.Dispose();
        _joinRequested.Dispose();
    }

    private void Join(CSteamID lobby)
    {
        IsHost = false;
        _entered.Set(SteamMatchmaking.JoinLobby(lobby));
    }

    private void OnLobbyCreated(LobbyCreated_t result, bool ioFailure)
    {
        if (ioFailure || result.m_eResult != EResult.k_EResultOK)
        {
            Error?.Invoke($"could not create lobby ({result.m_eResult})");
            return;
        }
        _lobby = new CSteamID(result.m_ulSteamIDLobby);
        LobbyReady?.Invoke(LobbyCode);
    }

    private void OnLobbyEntered(LobbyEnter_t result, bool ioFailure)
    {
        if (ioFailure || result.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            Error?.Invoke("could not join lobby");
            return;
        }
        _lobby = new CSteamID(result.m_ulSteamIDLobby);
        OpponentConnected?.Invoke(new SteamPeerLink(SteamMatchmaking.GetLobbyOwner(_lobby)), false);
    }

    private void OnMemberChanged(LobbyChatUpdate_t update)
    {
        if (update.m_ulSteamIDLobby != _lobby.m_SteamID) return;
        var member = new CSteamID(update.m_ulSteamIDUserChanged);
        if (member == SteamUser.GetSteamID()) return;
        var change = (EChatMemberStateChange)update.m_rgfChatMemberStateChange;
        if (IsHost && change.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeEntered))
            OpponentConnected?.Invoke(new SteamPeerLink(member), true);
        else if (!change.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeEntered))
            OpponentLeft?.Invoke(SteamFriends.GetFriendPersonaName(member));
    }
}
