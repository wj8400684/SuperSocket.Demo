using Google.Protobuf;
using SuperSocket.ProtoBase;

namespace Core;

public sealed partial class CommandPackage : IKeyedPackageInfo<CommandType>
{
    internal const int HeaderSize = sizeof(short);

    public CommandPackage CreateInfo<TPackage>(CommandType key, TPackage package)
        where TPackage : IMessage
    {
        return CreateInfo(key, package.ToByteString());
    }

    public CommandPackage CreateInfo(CommandType key, ByteString value)
    {
        Key = key;
        ErrorCode = default;
        ErrorMessage = string.Empty;
        Content = value;
        return this;
    }

    public CommandPackage CreateError(CommandType key, ErrorCode code, string message)
    {
        Key = key;
        ErrorCode = code;
        ErrorMessage = message;
        Content = ByteString.Empty;
        return this;
    }
}