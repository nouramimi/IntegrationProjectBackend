using Microsoft.AspNetCore.Mvc;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IntegrationsController : ControllerBase
    {
        private readonly IIntegrationService _service;

        public IntegrationsController(IIntegrationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var integration = await _service.GetByIdAsync(id);
            return integration == null ? NotFound() : Ok(integration);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Integration integration)
        {
            var created = await _service.CreateAsync(integration);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, Integration integration)
        {
            if (id != integration.Id) return BadRequest("ID mismatch");

            var updated = await _service.UpdateAsync(integration);
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
