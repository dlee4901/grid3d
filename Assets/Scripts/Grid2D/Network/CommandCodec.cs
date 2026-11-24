using System;
using System.IO;
using System.Text;

public static class CommandCodec
{
    public const byte Version = 3;
    public const int MaxTargets = 64;

    private const byte AbilityTag = 1;
    private const byte ForfeitTag = 3;

    public static byte[] Encode(Command command)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(Version);
            switch (command)
            {
                case AbilityCommand ability:
                    writer.Write(AbilityTag);
                    WireFormat.WriteId(writer, ability.AbilityId);
                    writer.Write(ability.SourcePosition);
                    writer.Write(ability.TargetPositions.Length);
                    foreach (var target in ability.TargetPositions) writer.Write(target);
                    var path = ability.Path ?? System.Array.Empty<int>();
                    writer.Write(path.Length);
                    foreach (var position in path) writer.Write(position);
                    writer.Write(ability.ElapsedMs);
                    break;
                case ForfeitCommand:
                    writer.Write(ForfeitTag);
                    break;
                default:
                    throw new NotSupportedException($"{command.GetType().Name} has no wire format");
            }
        }
        return stream.ToArray();
    }

    public static bool TryDecode(byte[] bytes, int issuer, out Command command)
        => WireFormat.TryRead(bytes, reader => Read(reader, issuer), out command);

    private static Command Read(BinaryReader reader, int issuer)
    {
        if (reader.ReadByte() != Version) return null;
        return reader.ReadByte() switch
        {
            AbilityTag => ReadAbility(reader, issuer),
            ForfeitTag => new ForfeitCommand(issuer),
            _ => null
        };
    }

    private static Command ReadAbility(BinaryReader reader, int issuer)
    {
        var abilityId = WireFormat.ReadId(reader);
        if (abilityId == null) return null;
        var source = reader.ReadInt32();
        var count = reader.ReadInt32();
        if (count < 0 || count > MaxTargets) return null;
        var targets = new int[count];
        for (var i = 0; i < count; i++) targets[i] = reader.ReadInt32();
        var pathCount = reader.ReadInt32();
        if (pathCount < 0 || pathCount > MaxTargets) return null;
        var path = new int[pathCount];
        for (var i = 0; i < pathCount; i++) path[i] = reader.ReadInt32();
        return new AbilityCommand(issuer, abilityId, source, targets, path, reader.ReadInt32());
    }
}
