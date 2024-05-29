namespace Fora.Data.Models;

public class CompanyInfo : IDataModel
{
    public int Id => Cik;
    public int Cik { get; set; }
    public string EntityName { get; set; }
    public InfoFact Facts { get; set; }
}

public class InfoFact
{
    public InfoFactUsGaap UsGaap { get; set; }
}

public class InfoFactUsGaap
{
    public InfoFactUsGaapNetIncomeLoss NetIncomeLoss { get; set; }
}

public class InfoFactUsGaapNetIncomeLoss
{
    public ICollection<InfoFactUsGaapIncomeLossUnits> Units { get; set; }
}

public class InfoFactUsGaapIncomeLossUnits
{
    public ICollection<InfoFactUsGaapIncomeLossUnitsUsd> Usd { get; set; }
}

public class InfoFactUsGaapIncomeLossUnitsUsd
{
    public string Form { get; set; }
    public string Frame { get; set; }
    public decimal Val { get; set; }
}