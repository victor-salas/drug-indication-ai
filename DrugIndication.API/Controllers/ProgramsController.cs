using DrugIndication.Domain.Entities;
using DrugIndication.Domain.Models;
using DrugIndication.Infrastructure;
using DrugIndication.Parsing.Transformers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrugIndication.API.Controllers
{
    [ApiController]
    [Route("programs")]
    public class ProgramController : ControllerBase
    {
        private readonly ProgramRepository _repository;
        private readonly ProgramTransformer _transformer;

        public ProgramController(ProgramRepository repository, ProgramTransformer transformer)
        {
            _repository = repository;
            _transformer = transformer;
        }

        /// <summary>
        /// Creates a new program using raw input enriched with AI and rules.
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ProgramDto>> CreateProgram([FromBody] RawProgramInput input)
        {
            if (input == null)
                return BadRequest("Invalid input");

            var transformed = await _transformer.TransformAsync(input);
            await _repository.CreateAsync(transformed);

            return CreatedAtAction(nameof(GetProgramById), new { id = transformed.ProgramID }, transformed);
        }

        /// <summary>
        /// Returns a program by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProgramDto>> GetProgramById(int id)
        {
            var program = await _repository.GetByIdAsync(id);
            if (program == null)
                return NotFound();
            return Ok(program);
        }

        /// <summary>
        /// Lists all stored programs
        /// </summary>
        [HttpGet]
        [HttpGet]
        public async Task<ActionResult<List<ProgramDto>>> GetAllPrograms([FromQuery] string? search)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var results = await _repository.SearchByNameAsync(search);
                return Ok(results);
            }

            var all = await _repository.GetAllAsync();
            return Ok(all);
        }

        /// <summary>
        /// Updates an existing program by ID
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProgram(int id, [FromBody] ProgramDto updated)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            updated.ProgramID = id;
            await _repository.UpdateAsync(updated);
            return NoContent();
        }

        /// <summary>
        /// Deletes a program by ID
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgram(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
