using System;

public interface IPeerLink
{
    void Send(byte[] message);
    event Action<byte[]> Received;
}
