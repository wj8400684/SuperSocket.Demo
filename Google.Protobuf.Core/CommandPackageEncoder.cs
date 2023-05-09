using System.Buffers;
using Google.Protobuf;
using SuperSocket.ProtoBase;

namespace Core;

public sealed class CommandPackageEncoder : IPackageEncoder<CommandPackage>
{
    public int Encode(IBufferWriter<byte> writer, CommandPackage pack)
    {
        var bodyLength = pack.CalculateSize();

        var length = writer.WriteBigEndian((short)bodyLength);

        pack.WriteTo(writer);

        return bodyLength + length;
    }
}