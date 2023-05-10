using Core;
using Microsoft.AspNetCore.Mvc;
using Server;
using SuperSocket;

namespace Google.Protobuf.Server.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/[controller]")]
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
        if (_sessionContainer.GetSessionByID(id) is not CommandSession session)
            return NotFound("客户端不在线");

        var resp = await session.InvokerRpcAsync<CommandOrder, CommandOrderReply>(
            cancellationToken: cancellationToken,
            content: new CommandOrder
            {
                Content = Guid.NewGuid().ToString()
            });

        return Ok(resp);
    }
}