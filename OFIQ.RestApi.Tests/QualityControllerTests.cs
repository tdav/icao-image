using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OFIQ.RestApi.Controllers;
using OFIQ.RestApi.Services;
using OFIQ.RestApi.Models;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace OFIQ.RestApi.Tests
{
    public class QualityControllerTests
    {
        private readonly Mock<IOFIQService> _ofiqServiceMock;
        private readonly QualityController _controller;

        public QualityControllerTests()
        {
            _ofiqServiceMock = new Mock<IOFIQService>();
            // This will fail to compile initially because QualityController doesn't exist
            _controller = new QualityController(_ofiqServiceMock.Object);
        }

        [Fact]
        public async Task GetScalarQuality_ReturnsOkWithScore()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "fake image content";
            var fileName = "test.jpg";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);

            _ofiqServiceMock.Setup(s => s.GetScalarQualityAsync(It.IsAny<Stream>()))
                .ReturnsAsync(85.5);

            // Act
            var result = await _controller.GetScalarQuality(fileMock.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ScalarQualityResponse>(okResult.Value);
            Assert.Equal(85.5, response.ScalarQuality);
        }
    }
}
