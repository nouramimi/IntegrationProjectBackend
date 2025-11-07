using Microsoft.AspNetCore.Mvc;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryAttemptsController : ControllerBase
    {
        private readonly IDeliveryAttemptService _service;

        public DeliveryAttemptsController(IDeliveryAttemptService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DeliveryAttempt attempt)
        {
            var created = await _service.CreateAsync(attempt);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, DeliveryAttempt attempt)
        {
            if (id != attempt.Id) return BadRequest("ID mismatch");

            var updated = await _service.UpdateAsync(attempt);
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
