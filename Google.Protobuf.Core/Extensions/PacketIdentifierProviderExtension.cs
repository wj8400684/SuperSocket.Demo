using Google.Protobuf;

namespace Core.Extensions;

public static class PacketIdentifierProviderExtension
{
    public static CommandPackage GetRpcCommandPackage(this PacketIdentifierProvider packetIdentifierProvider, CommandType rpcKey, ByteString content)
    {
        var identifier = packetIdentifierProvider.GetNextPacketIdentifier();

        return new CommandPackage
        {
            Key = CommandType.Rpc,
            RpcKey = rpcKey,
            Identifier = identifier,
            Content = content
        };
    }

    public static CommandPackage GetRpcCommandPackage<TContentPackage>(this PacketIdentifierProvider packetIdentifierProvider, CommandType rpcKey, TContentPackage content) 
        where TContentPackage : IMessage<TContentPackage>
    {
        return packetIdentifierProvider.GetRpcCommandPackage(rpcKey, content.ToByteString());
    }
}
