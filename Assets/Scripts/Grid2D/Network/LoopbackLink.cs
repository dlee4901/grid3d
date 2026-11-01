using System;
using System.Collections.Generic;

public sealed class LoopbackLink : IPeerLink
{
    private readonly Queue<byte[]> _inbox = new();
    private LoopbackLink _partner;

    public event Action<byte[]> Received;

    public static (LoopbackLink, LoopbackLink) CreatePair()
    {
        var a = new LoopbackLink();
        var b = new LoopbackLink();
        a._partner = b;
        b._partner = a;
        return (a, b);
    }

    public bool HasPending => _inbox.Count > 0;

    public void Send(byte[] message) => _partner._inbox.Enqueue((byte[])message.Clone());

    public void DeliverPending()
    {
        var count = _inbox.Count;
        for (var i = 0; i < count; i++)
        {
            var message = _inbox.Dequeue();
            Received?.Invoke(message);
        }
    }
}
