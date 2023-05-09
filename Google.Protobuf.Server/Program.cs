using Core;
using Server;
using Server.Command;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.IOCPTcpChannelCreatorFactory;
using SuperSocket.ProtoBase;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AsSuperSocketHostBuilder<CommandPackage, CommandPipelineFilter>()
    .UseCommand(options => options.AddCommandAssembly(typeof(LoginCommand).Assembly))
    .UsePackageDecoder<CommandPackageDecoder>()
    .UseHostedService<CommandServer>()
    .UseSession<CommandSession>()
    .UseClearIdleSession()
    .UseInProcSessionContainer()
    .UseIOCPTcpChannelCreatorFactory()
    .AsMinimalApiHostBuilder()
    .ConfigureHostBuilder();

builder.Services.AddSingleton<IPackageEncoder<CommandPackage>, CommandPackageEncoder>();
builder.Services.AddHostedService<PackageHostServer>();

var app = builder.Build();

app.Run();