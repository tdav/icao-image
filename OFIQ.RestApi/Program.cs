using OFIQ.RestApi.Services;
using OFIQ.RestApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IOFIQService, OFIQService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
