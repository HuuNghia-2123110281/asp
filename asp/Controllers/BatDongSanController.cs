using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatDongSanController : ControllerBase
    {
        // GET: api/<CategoryController1>
        [HttpGet]
        public IEnumerable<BatDongSan> Get()
        {
            return new List<BatDongSan>()
    {
        new BatDongSan {
            Id = 1,
            TieuDe = "Căn hộ cao cấp Vinhomes",
            DiaChi = "Bình Thạnh, TP.HCM",
            Gia = 4500000000,
            LoaiHinh = "Căn hộ"
        },
        new BatDongSan {
            Id = 2,
            TieuDe = "Đất nền sổ đỏ chính chủ",
            DiaChi = "Đức Hòa, Long An",
            Gia = 1200000000,   
            LoaiHinh = "Đất nền"
        }
    };
        }

        // GET api/<CategoryController1>/5
        [HttpGet("{id}")]
        public BatDongSan Get(int id)
        {
            return new BatDongSan { Id = id, TieuDe = "Căn hộ mẫu", DiaChi = "Địa chỉ mẫu", Gia = 0 };
        }

        // POST api/<CategoryController1>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<CategoryController1>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CategoryController1>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
