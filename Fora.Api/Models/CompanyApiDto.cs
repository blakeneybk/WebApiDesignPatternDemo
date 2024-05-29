namespace Fora.Api.Models;

public class CompanyApiDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal StandardFundableAmount { get; set; }
    public decimal SpecialFundableAmount { get; set; }
}