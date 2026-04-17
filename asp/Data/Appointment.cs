using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace asp.Data
{
    public class Appointment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CustomerId { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string PropertyId { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }

        [BsonElement("AppointmentDate")]
        public DateTime AppointmentDate { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; } = "Chờ xác nhận"; // Cập nhật logic thực tế
    }
}