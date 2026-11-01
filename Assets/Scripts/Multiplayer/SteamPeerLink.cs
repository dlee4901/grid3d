using System;
using System.Runtime.InteropServices;
using Steamworks;

public sealed class SteamPeerLink : IPeerLink, IDisposable
{
    private const int Channel = 0;
    private const int MaxMessagesPerPoll = 32;

    private readonly IntPtr[] _incoming = new IntPtr[MaxMessagesPerPoll];
    private readonly Callback<SteamNetworkingMessagesSessionRequest_t> _sessionRequest;
    private readonly Callback<SteamNetworkingMessagesSessionFailed_t> _sessionFailed;
    private SteamNetworkingIdentity _remote;

    public CSteamID Remote { get; }

    public event Action<byte[]> Received;
    public event Action<string> Disconnected;

    public SteamPeerLink(CSteamID remote)
    {
        Remote = remote;
        _remote = new SteamNetworkingIdentity();
        _remote.SetSteamID(remote);
        _sessionRequest = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnSessionRequest);
        _sessionFailed = Callback<SteamNetworkingMessagesSessionFailed_t>.Create(OnSessionFailed);
    }

    public void Send(byte[] message)
    {
        var handle = GCHandle.Alloc(message, GCHandleType.Pinned);
        try
        {
            var result = SteamNetworkingMessages.SendMessageToUser(ref _remote, handle.AddrOfPinnedObject(),
                (uint)message.Length, Constants.k_nSteamNetworkingSend_Reliable, Channel);
            if (result != EResult.k_EResultOK) Disconnected?.Invoke($"send failed: {result}");
        }
        finally
        {
            handle.Free();
        }
    }

    public void Poll()
    {
        int count;
        while ((count = SteamNetworkingMessages.ReceiveMessagesOnChannel(Channel, _incoming, MaxMessagesPerPoll)) > 0)
        {
            for (var i = 0; i < count; i++)
            {
                var message = SteamNetworkingMessage_t.FromIntPtr(_incoming[i]);
                byte[] bytes = null;
                if (message.m_identityPeer.GetSteamID() == Remote)
                {
                    bytes = new byte[message.m_cbSize];
                    Marshal.Copy(message.m_pData, bytes, 0, message.m_cbSize);
                }
                SteamNetworkingMessage_t.Release(_incoming[i]);
                if (bytes != null) Received?.Invoke(bytes);
            }
        }
    }

    public void Dispose()
    {
        _sessionRequest.Dispose();
        _sessionFailed.Dispose();
        SteamNetworkingMessages.CloseSessionWithUser(ref _remote);
    }

    private void OnSessionRequest(SteamNetworkingMessagesSessionRequest_t request)
    {
        if (request.m_identityRemote.GetSteamID() != Remote) return;
        SteamNetworkingMessages.AcceptSessionWithUser(ref request.m_identityRemote);
    }

    private void OnSessionFailed(SteamNetworkingMessagesSessionFailed_t failure)
    {
        if (failure.m_info.m_identityRemote.GetSteamID() == Remote) Disconnected?.Invoke("connection to opponent failed");
    }
}
