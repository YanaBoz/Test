using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using Test.Application.DTOs;
using Test.Core.Interfaces;
using Test.Core.Models;

namespace Test.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GroupsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroups()
        {
            var groups = await _unitOfWork.GroupRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<GroupDto>>(groups));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupDto>> GetGroup(int id)
        {
            var group = await _unitOfWork.GroupRepository.GetByIdAsync(id);
            if (group == null) return NotFound();
            return Ok(_mapper.Map<GroupDto>(group));
        }

        [HttpPost]
        public async Task<ActionResult<GroupDto>> CreateGroup(GroupDto dto)
        {
            var entity = _mapper.Map<Group>(dto);
            await _unitOfWork.GroupRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGroup), new { id = entity.Id }, _mapper.Map<GroupDto>(entity));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(int id, GroupDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var entity = _mapper.Map<Group>(dto);
            await _unitOfWork.GroupRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var group = await _unitOfWork.GroupRepository.GetByIdAsync(id);
            if (group == null) return NotFound();
            await _unitOfWork.GroupRepository.DeleteAsync(group);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }
    }
}
