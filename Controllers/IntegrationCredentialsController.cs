using Microsoft.AspNetCore.Mvc;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IntegrationCredentialsController : ControllerBase
    {
        private readonly IIntegrationCredentialService _service;

        public IntegrationCredentialsController(IIntegrationCredentialService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var credential = await _service.GetByIdAsync(id);
            return credential == null ? NotFound() : Ok(credential);
        }

        [HttpPost]
        public async Task<IActionResult> Create(IntegrationCredential credential)
        {
            var created = await _service.CreateAsync(credential);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, IntegrationCredential credential)
        {
            if (id != credential.Id) return BadRequest("ID mismatch");

            var updated = await _service.UpdateAsync(credential);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
