using ISBNResolver.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ISBNResolver.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ISBNController : ControllerBase
    {
        private readonly ILogger<ISBNController> _logger;
        private readonly IMediator _mediator;

        public ISBNController(ILogger<ISBNController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ObjectResult> Get(string ISBN, CancellationToken cancellationToken)
        {
            var command = new GetBookByISBNCommand { ISBN = ISBN };

            var response = await _mediator.Send(command);

            if (!response.Success || response.book is null)
                return new NotFoundObjectResult(response.book);
            return new OkObjectResult(response.book);
        }
    }
}
