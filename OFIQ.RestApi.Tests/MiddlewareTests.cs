using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OFIQ.RestApi.Middleware;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace OFIQ.RestApi.Tests
{
    public class MiddlewareTests
    {
        [Fact]
        public async Task ExceptionHandlingMiddleware_CatchesException_AndReturnsJson()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (HttpContext hc) => throw new Exception("Test Exception");
            var middleware = new ExceptionHandlingMiddleware(next, loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.Equal("application/json", context.Response.ContentType);
            Assert.Equal(500, context.Response.StatusCode);
            Assert.Contains("Test Exception", responseBody);
        }
    }
}
