using System.IO;
using HarmonyLib;
using NetSerializer;

namespace Content.Server._BRatbite.Security;

public static class NetSerializerGuard
{
    private const string HarmonyId = "netserializer-guard";

    private static bool _applied;

    public static void Apply(ISawmill log)
    {
        if (_applied)
            return;

        var harmony = new Harmony(HarmonyId);

        var readString = AccessTools.Method(typeof(Primitives), nameof(Primitives.ReadPrimitive),
            new[] { typeof(Stream), typeof(string).MakeByRefType() });
        var readBytes = AccessTools.Method(typeof(Primitives), nameof(Primitives.ReadPrimitive),
            new[] { typeof(Stream), typeof(byte[]).MakeByRefType() });

        if (readString == null || readBytes == null)
        {
            log.Error("NetSerializerGuard: could not find NetSerializer read methods, exploit guard NOT applied.");
            return;
        }

        harmony.Patch(readString, prefix: new HarmonyMethod(typeof(NetSerializerGuard), nameof(StringPrefix)));
        harmony.Patch(readBytes, prefix: new HarmonyMethod(typeof(NetSerializerGuard), nameof(BytesPrefix)));

        _applied = true;
        log.Info("NetSerializerGuard: length checks applied to NetSerializer string/byte[] reads.");
    }

    public static bool StringPrefix(Stream stream)
    {
        if (!stream.CanSeek)
            return true;

        var start = stream.Position;
        Primitives.ReadPrimitive(stream, out uint totalBytes);

        if (totalBytes > 1)
        {
            totalBytes -= 1;
            Primitives.ReadPrimitive(stream, out uint totalChars);

            if (totalChars > totalBytes)
                throw new InvalidDataException($"String claims {totalChars} chars from {totalBytes} bytes.");

            CheckRemaining(stream, totalBytes);
        }

        stream.Position = start;
        return true;
    }

    public static bool BytesPrefix(Stream stream)
    {
        if (!stream.CanSeek)
            return true;

        var start = stream.Position;
        Primitives.ReadPrimitive(stream, out uint len);

        if (len > 1)
            CheckRemaining(stream, len - 1);

        stream.Position = start;
        return true;
    }

    private static void CheckRemaining(Stream stream, uint length)
    {
        var remaining = stream.Length - stream.Position;
        if (length > remaining)
            throw new InvalidDataException($"Length prefix {length} exceeds remaining {remaining} bytes.");
    }
}
