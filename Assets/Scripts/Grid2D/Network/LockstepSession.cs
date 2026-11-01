using System;
using System.Linq;

public sealed class LockstepSession
{
    public enum SessionPhase { Idle, Handshake, Playing, Failed }

    private readonly IPeerLink _link;
    private readonly string _localPlayerId;
    private readonly bool _isHost;
    private readonly string _hostMapId;
    private readonly Func<string, GridDefinition> _findMap;
    private readonly TeamData _localTeam;

    private string _remotePlayerId;
    private TeamData _remoteTeam;
    private MatchSetup _setup;
    private TurnExecutor _executor;
    private bool _applyingRemote;

    public SessionPhase Phase { get; private set; } = SessionPhase.Idle;
    public string FailureReason { get; private set; }
    public int LocalSeat { get; private set; }
    public int RemoteSeat { get; private set; }

    public event Action<MatchSetup> MatchStarted;
    public event Action<string> Failed;

    public LockstepSession(IPeerLink link, string localPlayerId, bool isHost, string hostMapId,
                           Func<string, GridDefinition> findMap, TeamData localTeam)
    {
        _link = link;
        _localPlayerId = localPlayerId;
        _isHost = isHost;
        _hostMapId = hostMapId;
        _findMap = findMap;
        _localTeam = localTeam;
        _link.Received += OnReceived;
    }

    public void Start()
    {
        Phase = SessionPhase.Handshake;
        Send(NetMessage.Hello(_localPlayerId));
    }

    public void OpponentDisconnected(string reason) => Fail($"opponent disconnected: {reason}", opponentForfeits: true);

    public void Attach(TurnExecutor executor)
    {
        _executor = executor;
        _executor.CommandApplied += OnCommandApplied;
        Phase = SessionPhase.Playing;
    }

    private void OnReceived(byte[] bytes)
    {
        if (Phase == SessionPhase.Failed) return;
        if (NetMessage.IsProtocolMismatch(bytes))
        {
            Fail("protocol version mismatch");
            return;
        }
        if (!NetMessage.TryDecode(bytes, out var message))
        {
            Fail("malformed message");
            return;
        }
        switch (message.Type)
        {
            case NetMessageType.Hello: OnHello(message.PlayerId); break;
            case NetMessageType.Team: OnTeam(message.Team); break;
            case NetMessageType.Setup: OnSetup(message.Setup); break;
            case NetMessageType.Command: OnRemoteCommand(message); break;
        }
    }

    private void OnHello(string playerId)
    {
        if (_remotePlayerId != null || playerId == _localPlayerId)
        {
            Fail("unexpected hello");
            return;
        }
        _remotePlayerId = playerId;
        if (!_isHost) Send(NetMessage.ForTeam(_localTeam));
    }

    private void OnTeam(TeamData team)
    {
        if (!_isHost || _remotePlayerId == null || _remoteTeam != null)
        {
            Fail("unexpected team");
            return;
        }
        _remoteTeam = team;
        var setup = new MatchSetup(_hostMapId, new[] { _localPlayerId, _remotePlayerId }, new[] { _localTeam, team });
        var issue = setup.Validate(_findMap(_hostMapId));
        if (issue != null)
        {
            Fail($"invalid setup: {issue}");
            return;
        }
        Send(NetMessage.ForSetup(setup));
        BeginMatch(setup);
    }

    private void OnSetup(MatchSetup setup)
    {
        if (_isHost || _setup != null)
        {
            Fail("unexpected setup");
            return;
        }
        var issue = setup.Validate(_findMap(setup.MapId));
        if (issue == null && (setup.SeatOf(_remotePlayerId) != 1 || setup.SeatOf(_localPlayerId) == 0))
            issue = "seats do not match the handshake";
        if (issue == null && !SamePlacements(setup.Teams[setup.SeatOf(_localPlayerId) - 1], _localTeam))
            issue = "host changed this player's team";
        if (issue != null)
        {
            Fail($"invalid setup: {issue}");
            return;
        }
        BeginMatch(setup);
    }

    private void BeginMatch(MatchSetup setup)
    {
        _setup = setup;
        LocalSeat = setup.SeatOf(_localPlayerId);
        RemoteSeat = setup.SeatOf(_remotePlayerId);
        MatchStarted?.Invoke(setup);
    }

    private void OnCommandApplied(Command command)
    {
        if (_applyingRemote || Phase != SessionPhase.Playing || command.IssuingPlayer != LocalSeat) return;
        var sequence = _executor.Log.Count - 1;
        Send(NetMessage.ForCommand(sequence, _executor.StateHash, CommandCodec.Encode(command)));
    }

    private void OnRemoteCommand(NetMessage message)
    {
        if (Phase != SessionPhase.Playing)
        {
            Fail("command before match start");
            return;
        }
        if (message.Sequence != _executor.Log.Count)
        {
            Fail($"expected command #{_executor.Log.Count}, got #{message.Sequence}", opponentForfeits: true);
            return;
        }
        if (!CommandCodec.TryDecode(message.CommandBytes, RemoteSeat, out var command))
        {
            Fail("malformed command", opponentForfeits: true);
            return;
        }
        _applyingRemote = true;
        var applied = _executor.Apply(command);
        _applyingRemote = false;
        if (!applied) Fail($"peer command #{message.Sequence} was rejected", opponentForfeits: true);
        else if (_executor.StateHash != message.StateHash) Fail($"desync after command #{message.Sequence}");
    }

    private void Send(NetMessage message) => _link.Send(message.Encode());

    private void Fail(string reason, bool opponentForfeits = false)
    {
        if (Phase == SessionPhase.Failed) return;
        var wasPlaying = Phase == SessionPhase.Playing;
        Phase = SessionPhase.Failed;
        if (opponentForfeits && wasPlaying)
        {
            _applyingRemote = true;
            _executor.Apply(new ForfeitCommand(RemoteSeat));
            _applyingRemote = false;
        }
        FailureReason = reason;
        GridLog.Warning($"[LockstepSession] {reason}");
        Failed?.Invoke(reason);
    }

    private static bool SamePlacements(TeamData a, TeamData b)
        => a.UnitStartPositions.Select(u => (u.Position, u.UnitId))
            .SequenceEqual(b.UnitStartPositions.Select(u => (u.Position, u.UnitId)));
}
