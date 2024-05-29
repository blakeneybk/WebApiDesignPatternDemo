using Fora.Service.CompanyService;
using Microsoft.AspNetCore.Mvc;

namespace Fora.WebApi.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CompanyController(ICompanyService companyService, ILogger<CompanyController> logger) : ControllerBase
{
    [HttpGet("fundinginfo")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompanyFundingDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> GetCompanyFundingInfos([FromQuery] string startsWith)
    {
        try
        {
            var companies = startsWith == null ? 
                (await companyService.GetCompanies())?.ToList() : 
                (await companyService.GetCompaniesByPrefix(startsWith))?.ToList();

            if (companies == null || !companies.Any())
            {
                return NotFound();
            }

            return Ok(await companyService.GetCompanyFunding(companies));
        }
        catch (Exception ex)
        {
            var errorMessage = $"An error occurred while processing the request: {ex.Message}";
            logger.LogError(ex, errorMessage);
            return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
        }
    }
}