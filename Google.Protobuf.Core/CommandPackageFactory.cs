using Google.Protobuf;

namespace Core;

public static class CommandPackageFactory
{
    private static readonly Dictionary<Type, CommandType> _commands = new();

    static CommandPackageFactory()
    {
        RegisterCommandType<CommandOrder>(CommandType.AddOrder);
        RegisterCommandType<CommandOrderReply>(CommandType.AddOrderReply);

        RegisterCommandType<CommandHeartBeat>(CommandType.HeartBeat);
        RegisterCommandType<CommandHeartBeatReply>(CommandType.HeartBeatReply);

        RegisterCommandType<CommandRegister>(CommandType.Register);
        RegisterCommandType<CommandRegisterReply>(CommandType.RegisterReply);

        RegisterCommandType<CommandLogin>(CommandType.Login);
        RegisterCommandType<CommandLoginReply>(CommandType.LoginReply);

        RegisterCommandType<CommandHello>(CommandType.Hello);
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
        if (!_commands.TryGetValue(typeof(TPackageContent), out var key))
            throw new KeyNotFoundException("请注册key");

        return new CommandPackage
        {
            Key = CommandType.Rpc,
            RpcKey = key,
            Content = package.ToByteString(),
        };
    }
}
