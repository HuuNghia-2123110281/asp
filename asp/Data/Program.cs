using MongoDB.Driver;
using asp.Data; // Đảm bảo đúng namespace chứa class BatDongSan của bạn

var builder = WebApplication.CreateBuilder(args);

// 1. Lấy thông tin cấu hình từ appsettings.json
var mongoConnectionString = builder.Configuration.GetSection("MongoDB:ConnectionString").Value;
var mongoDatabaseName = builder.Configuration.GetSection("MongoDB:DatabaseName").Value;

// 2. Kết nối và Đăng ký MongoDB (Thay thế hoàn toàn SQL Server)
var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(mongoDatabaseName);

// Đăng ký IMongoDatabase dưới dạng Singleton để các Controller có thể sử dụng
builder.Services.AddSingleton(mongoDatabase);

// 3. Cấu hình CORS (Giữ nguyên để giao diện HTML gọi được API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. Thêm dịch vụ Controller và Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. Cấu hình HTTP request pipeline (Giữ nguyên các file tĩnh và Swagger)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Cho phép chạy file index.html từ thư mục wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Kích hoạt CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();