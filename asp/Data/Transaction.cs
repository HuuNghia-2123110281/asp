using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace asp.Data
{
    public class Transaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("PropertyId")]
        public string PropertyId { get; set; }

        [BsonElement("CustomerId")]
        public string CustomerId { get; set; }

        [BsonElement("Type")]
        public string Type { get; set; }

        [BsonElement("Price")]
        public decimal Price { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; }

        [BsonElement("Date")]
        public DateTime Date { get; set; } = DateTime.Now;
    }
}