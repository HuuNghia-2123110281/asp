using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace asp.Data
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string TransactionId { get; set; } = null!;

        public double Amount { get; set; }

        public string Method { get; set; } = "Chuyển khoản";

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public string? Note { get; set; }
    }
}