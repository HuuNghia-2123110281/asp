using Microsoft.AspNetCore.Http;

namespace asp.Data
{
    public class BatDongSanRequest
    {
        public string TieuDe { get; set; } = null!;
        public string? LoaiGiaoDich { get; set; }
        public string LoaiHinh { get; set; } = null!;
        public string? ProjectId { get; set; }
        public string? OwnerId { get; set; }
        public double DienTich { get; set; }
        public int PhongNgu { get; set; }
        public double Gia { get; set; }
        public string DiaChi { get; set; } = null!;
        public string? MoTa { get; set; }
        public IFormFile? HinhAnhFile { get; set; }

    }
}