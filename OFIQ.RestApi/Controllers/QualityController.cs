using Microsoft.AspNetCore.Mvc;
using OFIQ.RestApi.Services;
using OFIQ.RestApi.Models;

namespace OFIQ.RestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QualityController : ControllerBase
    {
        private readonly IOFIQService _ofiqService;

        public QualityController(IOFIQService ofiqService)
        {
            _ofiqService = ofiqService;
        }

        [HttpPost("scalar")]
        public async Task<IActionResult> GetScalarQuality(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                double score = await _ofiqService.GetScalarQualityAsync(stream);
                return Ok(new ScalarQualityResponse { ScalarQuality = score });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
