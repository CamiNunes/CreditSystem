using CreditSystem.Application.Interfaces;
using CreditSystem.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CreditSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditController : ControllerBase
{
    private readonly ICreditService _creditService;

    public CreditController(ICreditService creditService)
    {
        _creditService = creditService;
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestCredit([FromBody] CreditRequestDto requestDto)
    {
        try
        {
            var request = await _creditService.RequestCreditAsync(
                requestDto.ApplicantName,
                requestDto.ApplicantEmail,
                requestDto.RequestedAmount);

            return Ok(new CreditRequestResponseDto
            {
                RequestId = request.Id,
                Status = request.Status.ToString()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("evaluate/{requestId}")]
    public async Task<IActionResult> EvaluateCredit(int requestId)
    {
        try
        {
            await _creditService.EvaluateCreditRequestAsync(requestId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRequests()
    {
        var requests = await _creditService.GetAllCreditRequestsAsync();
        return Ok(requests);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRequestById(int id)
    {
        var request = await _creditService.GetCreditRequestByIdAsync(id);
        if (request == null)
            return NotFound();

        return Ok(request);
    }
}