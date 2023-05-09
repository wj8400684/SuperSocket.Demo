using Google.Protobuf;
using SuperSocket.ProtoBase;

namespace Core;

public sealed partial class CommandPackage : IKeyedPackageInfo<CommandType>
{
    internal const int HeaderSize = sizeof(short);

    public CommandPackage CreateInfo(ByteString value)
    {
        return new CommandPackage
        {
            RequestId = RequestId,
            Content = value,
        };
    }
    
    public CommandPackage CreateError(int code, string message)
    {
        return new CommandPackage
        {
            RequestId = RequestId,
            ErrorMessage = message
        };
    }
}