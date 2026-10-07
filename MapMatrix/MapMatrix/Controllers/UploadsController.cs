using MapMatrix.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MapMatrix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        private readonly FileUploadService _uploadService;
        public UploadsController(FileUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        // POST : api/uploads/image (multipart/form-data, champ "file")
        [Authorize(Roles = "guide,admin")]
        [HttpPost("image")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> uploadImage(IFormFile file)
        {
            try
            {
                var url = await _uploadService.saveImageAsync(file);
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                return Ok(new
                {
                    url = url,
                    fullUrl = baseUrl + url
                });
            }
            catch(ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
