using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using EDIAPI.Models;

var builder = WebApplication.CreateSlimBuilder(args);

// Thêm các dịch vụ vào container
builder.Services.AddControllers()
    .AddNewtonsoftJson(); // Sử dụng NewtonsoftJson
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

// Cấu hình middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:5155");

[JsonSerializable(typeof(ResponseMessage))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
