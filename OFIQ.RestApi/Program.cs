using OFIQ.RestApi.Services;
using OFIQ.RestApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IOFIQService, OFIQService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
