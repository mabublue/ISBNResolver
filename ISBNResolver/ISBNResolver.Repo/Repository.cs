using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using ISBNResolver.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ISBNResolver.Repository
{
    public interface IRepository
    {
        Task<Book> GetBook(string ISBN, CancellationToken cancellationToken);
        Task SaveBook(Book book, CancellationToken cancellationToken);
    }

    public class Repository : IRepository
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;
        private readonly Table _dynamoDbTable;
        private readonly ILogger<Repository> _logger;

        public Repository(ILogger<Repository> logger)
        {
            AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ToLower() == "development")
            {
                // Local DynamoDb Install:
                //   https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/CodeSamples.DotNet.html#CodeSamples.DotNet.RegionAndEndpoint
                clientConfig.ServiceURL = "http://localhost:8000";
            }
            else
            {
                clientConfig.RegionEndpoint = RegionEndpoint.APSoutheast2;
            }

            _dynamoDbClient = new AmazonDynamoDBClient(clientConfig);
            _dynamoDbTable = Table.LoadTable(_dynamoDbClient, "BookList");
            _logger = logger;
        }


        public async Task<Book> GetBook(string ISBN, CancellationToken cancellationToken)
        {
            var searchISBN = CleanISBN(ISBN);

            var document = await _dynamoDbTable.GetItemAsync(searchISBN);

            if (document is null)
                return null;
            return JsonSerializer.Deserialize<Book>(document["Book"]);
        }

        public async Task SaveBook(Book book, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(book.isbn)) {
                var item = new Document();
                item["Id"] = CleanISBN(book.isbn);
                item["Book"] = JsonSerializer.Serialize(book);
                await _dynamoDbTable.UpdateItemAsync(item, cancellationToken);
            }

            if (!string.IsNullOrEmpty(book.isbn13))
            {
                var item = new Document();
                item["Id"] = CleanISBN(book.isbn13);
                item["Book"] = JsonSerializer.Serialize(book);
                await _dynamoDbTable.UpdateItemAsync(item, cancellationToken);
            }
        }

        private string CleanISBN(string isbn)
        {
            return new string(isbn.Where(c => char.IsLetterOrDigit(c)).ToArray());
        }
    }

}