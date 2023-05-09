using Core;
using Microsoft.AspNetCore.Mvc;
using Server;
using SuperSocket;

namespace Google.Protobuf.Server.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/[controller]/[action]")]
public sealed class SessionController : ControllerBase
{
    private ILogger<SessionController> _logger;
    private readonly ISessionContainer _sessionContainer;

    public SessionController(
        ILogger<SessionController> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _sessionContainer = serviceProvider.GetRequiredService<ISessionContainer>();
    }

    [HttpGet]
    [Route("All")]
    public IActionResult All()
    {
        var sessions = _sessionContainer.GetSessions();

        return Ok(sessions);
    }

    [HttpGet]
    [Route("Order/Add")]
    public async ValueTask<IActionResult> OrderAddAsync(string id, CancellationToken cancellationToken)
    {
        var session = _sessionContainer.GetSessionByID(id) as CommandSession;

        if (session == null)
            return NotFound("客户端不在线");

        var addOrderPackage = new CommandPackage
        {
            Key = CommandType.AddOrder,
            Content = new CommandOrder
            {
                Content = Guid.NewGuid().ToString()
            }.ToByteString()
        };

        var resp = await session.GetResponsePacketAsync<CommandOrderReply>(addOrderPackage, cancellationToken);

        return Ok(resp);
    }
}