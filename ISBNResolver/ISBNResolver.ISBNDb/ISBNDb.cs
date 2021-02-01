using ISBNResolver.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ISBNResolver.ISBNDb
{
    public interface IISBNDb
    {
        Task<Book> CallApiForBookByISBN(string ISBN, CancellationToken cancellationToken);

    }
    public class ISBNDb : IISBNDb
    {
        private readonly ILogger<ISBNDb> _logger;
        private readonly IApiClient _apiClient;

        public ISBNDb(ILogger<ISBNDb> logger, IApiClient apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        public async Task<Book> CallApiForBookByISBN(string ISBN, CancellationToken cancellationToken)
        {
            var rawJson = await _apiClient.CallApiForBookByISBN(ISBN, cancellationToken);

            var bookWrapper = JsonSerializer.Deserialize<BookWrapper>(rawJson);

            if (bookWrapper is null)
                throw new Exception("Error Deserializing Book");

            return bookWrapper.book;
        }
    }
}
