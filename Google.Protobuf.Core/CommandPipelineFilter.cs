using System.Buffers;
using SuperSocket.ProtoBase;

namespace Core;

public sealed class CommandPipelineFilter : FixedHeaderPipelineFilter<CommandPackage>
{
    public CommandPipelineFilter() : base(CommandPackage.HeaderSize)
    {
    }

    protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);

        reader.TryReadLittleEndian(out short bodyLength);

        return bodyLength;
    }
}