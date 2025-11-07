using Microsoft.AspNetCore.Mvc;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;

        public AttachmentsController(IAttachmentService attachmentService)
        {
            _attachmentService = attachmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _attachmentService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _attachmentService.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Attachment attachment)
        {
            var created = await _attachmentService.CreateAsync(attachment);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, Attachment attachment)
        {
            if (id != attachment.Id) return BadRequest();

            var updated = await _attachmentService.UpdateAsync(attachment);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _attachmentService.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
