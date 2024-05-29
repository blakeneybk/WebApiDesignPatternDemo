using System.Text.Json.Serialization;
using Fora.ImportService.JsonConverters;

namespace Fora.ImportService.Models;

public class EdgarCompanyInfo
{
    [JsonConverter(typeof(StringOrNumberConverter))]
    public int Cik { get; set; }

    public string EntityName { get; set; }

    public InfoFact Facts { get; set; }

    public class InfoFact
    {
        [JsonPropertyName("us-gaap")]
        public InfoFactUsGaap UsGaap { get; set; }
    }

    public class InfoFactUsGaap
    {
        public InfoFactUsGaapNetIncomeLoss NetIncomeLoss { get; set; }
    }

    public class InfoFactUsGaapNetIncomeLoss
    {
        public InfoFactUsGaapIncomeLossUnits Units { get; set; }
    }

    public class InfoFactUsGaapIncomeLossUnits
    {
        public InfoFactUsGaapIncomeLossUnitsUsd[] Usd { get; set; }
    }

    public class InfoFactUsGaapIncomeLossUnitsUsd
    {
        public string Form { get; set; }
        public string Frame { get; set; }
        public decimal Val { get; set; }
    }
}