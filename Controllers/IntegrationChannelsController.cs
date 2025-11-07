using Microsoft.AspNetCore.Mvc;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IntegrationChannelsController : ControllerBase
    {
        private readonly IIntegrationChannelService _service;

        public IntegrationChannelsController(IIntegrationChannelService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var channel = await _service.GetByIdAsync(id);
            return channel == null ? NotFound() : Ok(channel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(IntegrationChannel channel)
        {
            var created = await _service.CreateAsync(channel);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, IntegrationChannel channel)
        {
            if (id != channel.Id) return BadRequest("ID mismatch");

            var updated = await _service.UpdateAsync(channel);
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
