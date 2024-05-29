using System;
using System.Text.Json.Serialization;

namespace Fora.Service.CompanyService;

public class CompanyFundingDto : IEquatable<CompanyFundingDto>
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("standardFundableAmount")]
    public decimal StandardFundableAmount { get; set; }

    [JsonPropertyName("specialFundableAmount")]
    public decimal SpecialFundableAmount { get; set; }

    public bool Equals(CompanyFundingDto other)
    {
        if (other is null)
            return false;

        return this.Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        if (obj is CompanyFundingDto other)
            return Equals(other);

        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}