using Steamworks;
using UnityEngine;

public class SteamMatchController : LoggableBehaviour
{
    private GridManager _gridManager;
    private GridDefinition _map;
    private TeamData _hostTeam;
    private TeamData _guestTeam;

    private SteamMatchService _match;
    private SteamPeerLink _link;
    private LockstepSession _session;
    private bool _steamReady;
    private string _status = "";
    private string _joinCode = "";

    public void Init(GridManager gridManager, GridDefinition map, TeamData hostTeam, TeamData guestTeam)
    {
        _gridManager = gridManager;
        _map = map;
        _hostTeam = hostTeam;
        _guestTeam = guestTeam;

        if (SteamAPI.InitEx(out var error) != ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
        {
            _status = $"Steam init failed: {error}";
            LogError(_status);
            return;
        }
        _steamReady = true;
        SteamNetworkingUtils.InitRelayNetworkAccess();

        _match = new SteamMatchService();
        _match.LobbyReady += code => SetStatus($"Lobby {code}: waiting for opponent");
        _match.OpponentConnected += OnOpponentConnected;
        _match.OpponentLeft += name => OnOpponentGone($"{name} left the lobby");
        _match.Error += SetStatus;
        SetStatus($"Signed in as {SteamFriends.GetPersonaName()}");

        var launchLobby = LaunchLobbyCode();
        if (launchLobby != null) _match.Join(launchLobby);
    }

    private static string LaunchLobbyCode()
    {
        var args = System.Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++)
            if (args[i] == "+connect_lobby") return args[i + 1];
        return null;
    }

    private void OnOpponentConnected(SteamPeerLink link, bool isHost)
    {
        if (_session != null) return;
        _link = link;
        _link.Disconnected += OnOpponentGone;
        _session = new LockstepSession(link, _match.LocalPlayerId, isHost, isHost ? _map.Id : null, FindMap,
                                       isHost ? _hostTeam : _guestTeam);
        _session.MatchStarted += OnMatchStarted;
        _session.Failed += reason => SetStatus($"Match stopped: {reason}");
        _session.Start();
        SetStatus("Connecting to opponent...");
    }

    private void OnOpponentGone(string reason)
    {
        SetStatus($"Opponent gone: {reason}");
        _session?.OpponentDisconnected(reason);
    }

    private void OnMatchStarted(MatchSetup setup)
    {
        _gridManager.StartGame(FindMap(setup.MapId), setup.SeedCommands(), _session.LocalSeat);
        _gridManager.AttachSession(_session);
        SetStatus($"Playing as seat {_session.LocalSeat}");
    }

    private static GridDefinition FindMap(string id) => IdRegistry<GridDefinition>.TryGet(id, out var map) ? map : null;

    private void SetStatus(string status)
    {
        _status = status;
        Log(status);
    }

    private void Update()
    {
        if (!_steamReady) return;
        SteamAPI.RunCallbacks();
        _link?.Poll();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 320, 160), GUI.skin.box);
        GUILayout.Label(_status);
        if (_steamReady && _session == null)
        {
            if (_match.LobbyCode == null)
            {
                if (GUILayout.Button("Host")) _match.Host();
                _joinCode = GUILayout.TextField(_joinCode);
                if (GUILayout.Button("Join")) _match.Join(_joinCode);
            }
            else if (_match.IsHost)
            {
                if (GUILayout.Button("Invite friend")) _match.InviteFriend();
                if (GUILayout.Button("Copy lobby code")) GUIUtility.systemCopyBuffer = _match.LobbyCode;
            }
        }
        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        _link?.Dispose();
        _match?.Dispose();
        if (_steamReady) SteamAPI.Shutdown();
    }
}
