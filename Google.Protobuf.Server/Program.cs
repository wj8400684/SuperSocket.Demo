using Core;
using Server.Server;
using SuperSocket;
using SuperSocket.IOCPTcpChannelCreatorFactory;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AsSuperSocketHostBuilder<CommandPackage, CommandPipelineFilter>()
    // .UseCommand(options => options.AddCommandAssembly(typeof(ADD).Assembly))
    .UsePackageDecoder<CommandPackageDecoder>()
    .UseHostedService<CommandServer>()
    .UseSession<CommandSession>()
    .UseClearIdleSession()
    .UseInProcSessionContainer()
    .UseIOCPTcpChannelCreatorFactory()
    .AsMinimalApiHostBuilder()
    .ConfigureHostBuilder();

var app = builder.Build();

app.Run();