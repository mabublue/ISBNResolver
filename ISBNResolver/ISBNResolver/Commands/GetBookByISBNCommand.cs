using ISBNResolver.ISBNDb;
using ISBNResolver.Models;
using ISBNResolver.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ISBNResolver.Commands
{
    public class GetBookByISBNCommand : IRequest<GetBookByISBNCommandResponse>
    {
        public string ISBN { get; set; }
    }

    public class GetBookByISBNCommandResponse : CommandResponse
    {
        public Book book { get; set; }
    }

    public class GetBookByISBNCommandHandler : IRequestHandler<GetBookByISBNCommand, GetBookByISBNCommandResponse>
    {
        private readonly ILogger<GetBookByISBNCommandHandler> _logger;
        private readonly IISBNDb _isbnDb;
        private readonly IRepository _repository;

        public GetBookByISBNCommandHandler(ILogger<GetBookByISBNCommandHandler> logger, IISBNDb isbnDb, IRepository repository)
        {
            _logger = logger;
            _isbnDb = isbnDb;
            _repository = repository;
        }

        public async Task<GetBookByISBNCommandResponse> Handle(GetBookByISBNCommand request, CancellationToken cancellationToken)
        {
            var commandResponse = new GetBookByISBNCommandResponse();

            Book book;

            // Check Dynamo for book
            try
            {
                book = await _repository.GetBook(request.ISBN, cancellationToken);
            }
            catch (Exception e)
            {
                commandResponse.Errors.Append($"Error retreiving Book from Db by ISBN : {e.Message}");
                _logger.LogWarning($"Error retreiving Book from Db by ISBN : {e.Message}");
                return commandResponse;
            }

            // If not in Dynamo, call out to ISBNDb
            if (book is null)
            try
            {
                book = await _isbnDb.CallApiForBookByISBN(request.ISBN, cancellationToken);
                await _repository.SaveBook(book, cancellationToken);
            }
            catch (Exception e)
            {
                commandResponse.Errors.Append($"Error retreiving Book from Api by ISBN : {e.Message}");
                _logger.LogWarning($"Error retreiving Book from Api by ISBN : {e.Message}");
                return commandResponse;
            }

            commandResponse.book = book;

            return commandResponse;
        }
    }
}
