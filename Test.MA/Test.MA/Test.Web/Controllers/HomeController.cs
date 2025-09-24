using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using Test.Application.DTOs;
using Test.Application.Services;
using Test.Core.Interfaces;

namespace Test.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IExcelProcessingService _excelService;
        private readonly ITurnoverService _turnoverService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HomeController(
            IExcelProcessingService excelService,
            ITurnoverService turnoverService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _excelService = excelService;
            _turnoverService = turnoverService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required");

            using var stream = file.OpenReadStream();
            var turnoversDto = await _excelService.ProcessUploadedFileAsync(stream, file.FileName);

            // При желании можно сохранить список Turnover в БД через UnitOfWork
            // Пример сохранения (опционально):
            foreach (var tDto in turnoversDto)
            {
                var entity = _mapper.Map<Test.Core.Models.Turnover>(tDto);
                await _unitOfWork.TurnoverRepository.AddAsync(entity);
            }
            await _unitOfWork.SaveChangesAsync();

            var processed = _turnoverService.ProcessTurnoverData(turnoversDto);
            return Ok(processed);
        }

        [HttpGet("files/{fileId}")]
        public async Task<IActionResult> GetFileData(int fileId)
        {
            var file = await _unitOfWork.FileRepository.GetByIdAsync(fileId);
            if (file == null) return NotFound();
            var dto = _mapper.Map<FileDto>(file);
            return Ok(dto);
        }
    }
}
