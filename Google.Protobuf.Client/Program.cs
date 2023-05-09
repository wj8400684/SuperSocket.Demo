using System.Diagnostics;
using System.Net;
using SuperSocket.Client;
using Core;
using Google.Protobuf;

var client = new EasyClient<CommandPackage, CommandPackage>(new CommandPipelineFilter
{
    Decoder = new
        CommandPackageDecoder()
}, new CommandPackageEncoder()).AsClient();

await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 4040));

var watch = new Stopwatch();
watch.Start();

Console.WriteLine("请输入发送次数，不输入默认为100w次按enter ");

var count = 1000 * 1000;

var input = Console.ReadLine();

if (!string.IsNullOrWhiteSpace(input))
    _ = int.TryParse(input, out count);

Console.WriteLine($"开始执行");

for (int i = 0; i < count; i++)
{
    var loginCommand = new CommandPackage
    {
        Key = CommandType.Login,
        Content = new CommandLogin
        {
            Email = "8400684@..qomc",
            Username = "fefsfs",
            Password = "ssssss"
        }.ToByteString()
    };
    
    await client.SendAsync(loginCommand);

    var reply = await client.ReceiveAsync();
}

watch.Stop();
Console.WriteLine($"执行完成{watch.ElapsedMilliseconds/1000}秒");

Console.ReadKey();