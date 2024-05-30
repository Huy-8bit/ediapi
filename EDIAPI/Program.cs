using Microsoft.EntityFrameworkCore;
using EDIAPI.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using EDIAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Thêm các dịch vụ vào container
builder.Services.AddControllers()
    .AddNewtonsoftJson(); // Sử dụng NewtonsoftJson
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Kiểm tra kết nối SQL Server
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        if (dbContext.Database.CanConnect())
        {
            Console.WriteLine("Kết nối tới SQL Server thành công!");
        }
        else
        {
            Console.WriteLine("Không thể kết nối tới SQL Server.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Lỗi kết nối SQL Server: {ex.Message}");
    }
}

// Cấu hình middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

[JsonSerializable(typeof(ResponseMessage))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
