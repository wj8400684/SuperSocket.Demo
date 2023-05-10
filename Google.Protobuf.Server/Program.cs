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
    .UsePackageHandlingScheduler<CommandRpcPackageHandlingScheduler>()
    .UseClearIdleSession()
    .UseInProcSessionContainer()
    .UseIOCPTcpChannelCreatorFactory()
    .AsMinimalApiHostBuilder()
    .ConfigureHostBuilder();

builder.Services.AddSingleton<IPackageEncoder<CommandPackage>, CommandPackageEncoder>();

builder.Services.AddHostedService<PackageHostServer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();