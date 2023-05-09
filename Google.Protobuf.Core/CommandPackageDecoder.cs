using System.Buffers;
using SuperSocket.ProtoBase;

namespace Core;

public sealed class CommandPackageDecoder : IPackageDecoder<CommandPackage>
{
    public CommandPackage Decode(ref ReadOnlySequence<byte> buffer, object context)
    {
        return CommandPackage.Parser.ParseFrom(buffer.Slice(CommandPackage.HeaderSize));
    }
}