using MongoDB.Driver;
using asp.Data;

var builder = WebApplication.CreateBuilder(args);


var mongoConnectionString = builder.Configuration.GetSection("MongoDB:ConnectionString").Value;
var mongoDatabaseName = builder.Configuration.GetSection("MongoDB:DatabaseName").Value;


var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(mongoDatabaseName);

builder.Services.AddSingleton(mongoDatabase);
builder.Services.AddSingleton<IMongoClient>(mongoClient);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "NguyenHuuNghia_2123110281",
        Version = "v1",
        Description = "Hệ thống API Bất Động Sản của Nguyễn Hữu Nghĩa"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();


app.UseDefaultFiles();
app.UseStaticFiles();

// Kích hoạt CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();