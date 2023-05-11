using Google.Protobuf;

namespace Core;

public static class CommandPackageFactory
{
    private static readonly Dictionary<Type, string> _commands = new();

    static CommandPackageFactory()
    {
    }

    private static void RegisterCommandType<TPackageContent>(CommandType key)
    {
        _commands.Add(typeof(TPackageContent), key);
    }

    public static CommandPackage CreateRequest<TPackageContent>(CommandType key, TPackageContent package)
        where TPackageContent : IMessage
    {
        return new CommandPackage().CreateInfo(key, package);
    }

    public static CommandPackage CreateRequest<TPackageContent>(TPackageContent package)
        where TPackageContent : IMessage
    {
        if (!_commands.TryGetValue(typeof(TPackageContent), out var key))
            throw new KeyNotFoundException("请注册key");

        return new CommandPackage().CreateInfo(key, package);
    }

    public static CommandPackage CreateRpcRequest<TPackageContent>(TPackageContent package)
        where TPackageContent : IMessage
    {
        const string rpckey = nameof(CommandRpc);

        return new CommandPackage
        {
            Key = rpckey,
            RpcKey = typeof(TPackageContent).Name,
            Content = package.ToByteString(),
        };
    }
}
