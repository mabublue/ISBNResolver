using Amazon.DynamoDBv2.DataModel;

namespace ISBNResolver.Models
{
    public class Book
    {
        public string id { get; set; }
        public string publisher { get; set; }
        public string title { get; set; }
        public string title_long { get; set; }
        public string isbn { get; set; }
        public string isbn13 { get; set; }
        public string dewey_decimal { get; set; }
        public string binding { get; set; }
        public string language { get; set; }
        public string date_published { get; set; }
        public string edition { get; set; }
        public int pages { get; set; }
        public string dimensions { get; set; }
        public string overview { get; set; }
        public string image { get; set; }
        public string msrp { get; set; }
        public string excerpt { get; set; }
        public string synopsys { get; set; }
        public string[] authors { get; set; }
        public string[] subjects { get; set; }
        public string[] reviews { get; set; }
    }
}
