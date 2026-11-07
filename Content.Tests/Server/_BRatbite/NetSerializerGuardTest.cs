using System.IO;
using Content.Server._BRatbite.Security;
using NetSerializer;
using NUnit.Framework;

namespace Content.Tests.Server._BRatbite;

[TestFixture]
[TestOf(typeof(NetSerializerGuard))]
public sealed class NetSerializerGuardTest
{
    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("héllo wörld ☢")]
    public void ValidStringPasses(string value)
    {
        var stream = new MemoryStream();
        Primitives.WritePrimitive(stream, value);
        stream.Position = 0;

        Assert.That(NetSerializerGuard.StringPrefix(stream), Is.True);
        Assert.That(stream.Position, Is.Zero);

        Primitives.ReadPrimitive(stream, out string back);
        Assert.That(back, Is.EqualTo(value));
    }

    [Test]
    public void ValidBytesPass()
    {
        var stream = new MemoryStream();
        Primitives.WritePrimitive(stream, new byte[] { 1, 2, 3 });
        stream.Position = 0;

        Assert.That(NetSerializerGuard.BytesPrefix(stream), Is.True);
        Assert.That(stream.Position, Is.Zero);
    }

    [Test]
    public void OversizedStringLengthIsRejected()
    {
        var stream = new MemoryStream();
        Primitives.WritePrimitive(stream, 0x0FFFFFF0u);
        Primitives.WritePrimitive(stream, 0x0FFFFFE0u);
        stream.WriteByte((byte) 'A');
        stream.Position = 0;

        Assert.Throws<InvalidDataException>(() => NetSerializerGuard.StringPrefix(stream));
    }

    [Test]
    public void StringWithMoreCharsThanBytesIsRejected()
    {
        var stream = new MemoryStream();
        Primitives.WritePrimitive(stream, 3u);
        Primitives.WritePrimitive(stream, 0x0FFFFFE0u);
        stream.WriteByte((byte) 'A');
        stream.WriteByte((byte) 'A');
        stream.Position = 0;

        Assert.Throws<InvalidDataException>(() => NetSerializerGuard.StringPrefix(stream));
    }

    [Test]
    public void OversizedByteArrayLengthIsRejected()
    {
        var stream = new MemoryStream();
        Primitives.WritePrimitive(stream, 0x0FFFFFF0u);
        stream.WriteByte(1);
        stream.Position = 0;

        Assert.Throws<InvalidDataException>(() => NetSerializerGuard.BytesPrefix(stream));
    }
}
