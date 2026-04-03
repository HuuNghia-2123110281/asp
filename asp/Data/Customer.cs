using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace asp.Data
{
    public class Customer
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Phone")]
        public string Phone { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Demand")]
        public string Demand { get; set; }
    }
}