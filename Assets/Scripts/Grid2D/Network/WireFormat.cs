using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

internal static class WireFormat
{
    public const int MaxIdBytes = 128;

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static void WriteId(BinaryWriter writer, string id)
    {
        var bytes = Encoding.UTF8.GetBytes(id ?? string.Empty);
        if (bytes.Length > MaxIdBytes) throw new ArgumentException($"id '{id}' exceeds {MaxIdBytes} bytes");
        writer.Write((byte)bytes.Length);
        writer.Write(bytes);
    }

    public static string ReadId(BinaryReader reader)
    {
        var length = reader.ReadByte();
        if (length > MaxIdBytes) return null;
        var bytes = reader.ReadBytes(length);
        if (bytes.Length != length) throw new EndOfStreamException();
        return StrictUtf8.GetString(bytes);
    }

    public const int MaxUnitsPerTeam = 32;

    public static void WriteTeam(BinaryWriter writer, TeamData team)
    {
        WriteId(writer, team.Id);
        writer.Write((byte)team.UnitStartPositions.Count);
        foreach (var unit in team.UnitStartPositions)
        {
            writer.Write(unit.Position);
            WriteId(writer, unit.UnitId);
        }
    }

    public static TeamData ReadTeam(BinaryReader reader, string mapId)
    {
        var teamId = ReadId(reader);
        if (teamId == null) return null;
        var unitCount = reader.ReadByte();
        if (unitCount > MaxUnitsPerTeam) return null;
        var units = new List<(int, string)>();
        for (var i = 0; i < unitCount; i++)
        {
            var position = reader.ReadInt32();
            var unitId = ReadId(reader);
            if (unitId == null) return null;
            units.Add((position, unitId));
        }
        return new TeamData(teamId, mapId, units);
    }

    public static bool TryRead<T>(byte[] bytes, Func<BinaryReader, T> read, out T value) where T : class
    {
        value = null;
        if (bytes == null) return false;
        try
        {
            using var reader = new BinaryReader(new MemoryStream(bytes, writable: false), Encoding.UTF8);
            value = read(reader);
            if (reader.BaseStream.Position != bytes.Length) value = null;
        }
        catch (EndOfStreamException)
        {
            value = null;
        }
        catch (DecoderFallbackException)
        {
            value = null;
        }
        return value != null;
    }
}
