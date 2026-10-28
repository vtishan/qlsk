using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using QLSK.Common.Auth; using QLSK.Common.Models;
namespace QLSK.Features.Ve;
[ApiController,Route("api/tickets"),Authorize] public class VeController(VeService service):ControllerBase
{
 [HttpGet("me")] public async Task<IActionResult> Me()=>Ok(ApiResponse.Ok(await service.CuaToi(User.LayId())));
 [HttpGet("{id:int}")] public async Task<IActionResult> Lay(int id)=>Ok(ApiResponse.Ok(await service.Lay(User.LayId(),id,User.IsInRole("QuanLySuKien")||User.IsInRole("QuanTriVien"))));
 [HttpGet("by-code/{code}")] public async Task<IActionResult> Ma(string code)=>Ok(ApiResponse.Ok(await service.Ma(User.LayId(),code,User.IsInRole("QuanLySuKien")||User.IsInRole("QuanTriVien"))));
}
[ApiController,Route("api/check-in"),Authorize(Roles="QuanLySuKien,QuanTriVien")] public class CheckInController(VeService service):ControllerBase
{
 public class CheckInDto{[Required] public string Ma{get;set;}="";}
 [HttpPost] public async Task<IActionResult> Post(CheckInDto dto)=>Ok(ApiResponse.Ok(await service.CheckIn(dto.Ma.Trim())));
}
