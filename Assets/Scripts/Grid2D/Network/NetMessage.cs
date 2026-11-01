using System.IO;
using System.Text;

public enum NetMessageType : byte { Hello = 1, Team = 2, Setup = 3, Command = 4 }

public sealed class NetMessage
{
    public const byte ProtocolVersion = 1;

    public NetMessageType Type { get; private set; }
    public string PlayerId { get; private set; }
    public TeamData Team { get; private set; }
    public MatchSetup Setup { get; private set; }
    public int Sequence { get; private set; }
    public ulong StateHash { get; private set; }
    public byte[] CommandBytes { get; private set; }

    private NetMessage() { }

    public static NetMessage Hello(string playerId) => new() { Type = NetMessageType.Hello, PlayerId = playerId };
    public static NetMessage ForTeam(TeamData team) => new() { Type = NetMessageType.Team, Team = team };
    public static NetMessage ForSetup(MatchSetup setup) => new() { Type = NetMessageType.Setup, Setup = setup };

    public static NetMessage ForCommand(int sequence, ulong stateHash, byte[] commandBytes)
        => new() { Type = NetMessageType.Command, Sequence = sequence, StateHash = stateHash, CommandBytes = commandBytes };

    public byte[] Encode()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(ProtocolVersion);
            writer.Write((byte)Type);
            switch (Type)
            {
                case NetMessageType.Hello:
                    WireFormat.WriteId(writer, PlayerId);
                    break;
                case NetMessageType.Team:
                    WireFormat.WriteId(writer, Team.MapId);
                    WireFormat.WriteTeam(writer, Team);
                    break;
                case NetMessageType.Setup:
                    WriteBlob(writer, Setup.Encode());
                    break;
                case NetMessageType.Command:
                    writer.Write(Sequence);
                    writer.Write(StateHash);
                    WriteBlob(writer, CommandBytes);
                    break;
            }
        }
        return stream.ToArray();
    }

    public static bool TryDecode(byte[] bytes, out NetMessage message)
        => WireFormat.TryRead(bytes, Read, out message);

    public static bool IsProtocolMismatch(byte[] bytes)
        => bytes != null && bytes.Length > 0 && bytes[0] != ProtocolVersion;

    private static NetMessage Read(BinaryReader reader)
    {
        if (reader.ReadByte() != ProtocolVersion) return null;
        switch ((NetMessageType)reader.ReadByte())
        {
            case NetMessageType.Hello:
                var playerId = WireFormat.ReadId(reader);
                return playerId == null ? null : Hello(playerId);
            case NetMessageType.Team:
                var mapId = WireFormat.ReadId(reader);
                if (mapId == null) return null;
                var team = WireFormat.ReadTeam(reader, mapId);
                return team == null ? null : ForTeam(team);
            case NetMessageType.Setup:
                var setupBytes = ReadBlob(reader);
                return setupBytes != null && MatchSetup.TryDecode(setupBytes, out var setup) ? ForSetup(setup) : null;
            case NetMessageType.Command:
                var sequence = reader.ReadInt32();
                var hash = reader.ReadUInt64();
                var commandBytes = ReadBlob(reader);
                return commandBytes == null ? null : ForCommand(sequence, hash, commandBytes);
            default:
                return null;
        }
    }

    private const int MaxBlobBytes = 4096;

    private static void WriteBlob(BinaryWriter writer, byte[] blob)
    {
        writer.Write((ushort)blob.Length);
        writer.Write(blob);
    }

    private static byte[] ReadBlob(BinaryReader reader)
    {
        var length = reader.ReadUInt16();
        if (length > MaxBlobBytes) return null;
        var blob = reader.ReadBytes(length);
        if (blob.Length != length) throw new EndOfStreamException();
        return blob;
    }
}
