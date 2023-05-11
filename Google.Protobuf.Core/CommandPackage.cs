using Google.Protobuf;
using SuperSocket.ProtoBase;

namespace Core;

public sealed partial class CommandPackage : IKeyedPackageInfo<string>
{
    internal const int HeaderSize = sizeof(short);

    public CommandPackage CreateInfo<TPackage>(string key, TPackage package)
        where TPackage : IMessage
    {
        return CreateInfo(key, package.ToByteString());
    }

    public CommandPackage CreateInfo(string key, ByteString value)
    {
        Key = key;
        ErrorCode = default;
        ErrorMessage = string.Empty;
        Content = value;
        return this;
    }

    public CommandPackage CreateError(string key, ErrorCode code, string message)
    {
        Key = key;
        ErrorCode = code;
        ErrorMessage = message;
        Content = ByteString.Empty;
        return this;
    }

    public CommandPackage CreateRpcError(string rpcKey, ErrorCode code, string message)
    {
        Key = CommandType.RpcReply;
        RpcKey = rpcKey;
        ErrorCode = code;
        ErrorMessage = message;
        Content = ByteString.Empty;
        return this;
    }

    public CommandPackage CreateRpcInfo(string rpcKey, ByteString value)
    {
        Key = CommandType.RpcReply;
        RpcKey = rpcKey;
        ErrorCode = default;
        ErrorMessage = string.Empty;
        Content = value;
        return this;
    }
}