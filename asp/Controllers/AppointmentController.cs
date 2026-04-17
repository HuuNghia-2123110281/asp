using asp.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IMongoCollection<Appointment> _appointmentCollection;

        public AppointmentController(IMongoDatabase database)
        {
            _appointmentCollection = database.GetCollection<Appointment>("Appointments");
        }

        // 1. Lấy toàn bộ danh sách lịch hẹn
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAll()
        {
            var appointments = await _appointmentCollection.Find(_ => true)
                .SortByDescending(a => a.AppointmentDate)
                .ToListAsync();
            return Ok(appointments);
        }

        // 2. Tạo lịch hẹn mới
        [HttpPost]
        public async Task<ActionResult> Create(Appointment appointment)
        {
            await _appointmentCollection.InsertOneAsync(appointment);
            return Ok(new { message = "Tạo lịch hẹn thành công!", data = appointment });
        }

        // 3. Cập nhật trạng thái lịch hẹn 
        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateStatus(string id, [FromBody] string newStatus)
        {
            var filter = Builders<Appointment>.Filter.Eq(a => a.Id, id);
            var update = Builders<Appointment>.Update.Set(a => a.Status, newStatus);

            var result = await _appointmentCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0) return NotFound(new { message = "Không tìm thấy lịch hẹn" });

            return Ok(new { message = "Cập nhật trạng thái thành công!" });
        }

        // 4. Xóa lịch hẹn
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var result = await _appointmentCollection.DeleteOneAsync(a => a.Id == id);
            if (result.DeletedCount == 0) return NotFound();
            return Ok(new { message = "Xóa lịch hẹn thành công!" });
        }
    }
}