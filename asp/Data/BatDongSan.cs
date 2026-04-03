using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace asp.Data
{
    public class BatDongSan
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string TieuDe { get; set; }
        public string DiaChi { get; set; }
        public decimal Gia { get; set; }
        public string LoaiHinh { get; set; }
    }
}