using Microsoft.AspNetCore.Mvc;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserNotificationsController : ControllerBase
    {
        private readonly IUserNotificationService _service;

        public UserNotificationsController(IUserNotificationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userNotif = await _service.GetByIdAsync(id);
            return userNotif == null ? NotFound() : Ok(userNotif);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserNotification userNotif)
        {
            var created = await _service.CreateAsync(userNotif);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UserNotification userNotif)
        {
            if (id != userNotif.Id) return BadRequest("ID mismatch");

            var updated = await _service.UpdateAsync(userNotif);
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
