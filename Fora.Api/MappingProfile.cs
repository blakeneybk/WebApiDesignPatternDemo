using AutoMapper;
using Fora.Data.Models;
using Fora.ImportService.Models;
using Fora.Service.CompanyService;
using Fora.Service.Funding.ChainHandlers.Contexts;
using InfoFact = Fora.Data.Models.InfoFact;
using InfoFactUsGaap = Fora.Data.Models.InfoFactUsGaap;
using InfoFactUsGaapIncomeLossUnits = Fora.Data.Models.InfoFactUsGaapIncomeLossUnits;
using InfoFactUsGaapIncomeLossUnitsUsd = Fora.Data.Models.InfoFactUsGaapIncomeLossUnitsUsd;
using InfoFactUsGaapNetIncomeLoss = Fora.Data.Models.InfoFactUsGaapNetIncomeLoss;

namespace Fora.Api;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<EdgarCompanyInfo, CompanyInfo>()
            .ForMember(dest => dest.Cik, opt => opt.MapFrom(src => src.Cik))
            .ForMember(dest => dest.EntityName, opt => opt.MapFrom(src => src.EntityName))
            .ForMember(dest => dest.Facts, opt => opt.MapFrom(src => src.Facts))
            .ReverseMap();

        CreateMap<EdgarCompanyInfo.InfoFact, InfoFact>()
            .ForMember(dest => dest.UsGaap, opt => opt.MapFrom(src => src.UsGaap))
            .ReverseMap();

        CreateMap<EdgarCompanyInfo.InfoFactUsGaap, InfoFactUsGaap>()
            .ForMember(dest => dest.NetIncomeLoss, opt => opt.MapFrom(src => src.NetIncomeLoss))
            .ReverseMap();

        CreateMap<EdgarCompanyInfo.InfoFactUsGaapNetIncomeLoss, InfoFactUsGaapNetIncomeLoss>()
            .ForMember(dest => dest.Units, opt => opt.MapFrom(src => MapUnits(src.Units)))
            .ReverseMap();

        CreateMap<EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnits, InfoFactUsGaapIncomeLossUnits>()
            .ForMember(dest => dest.Usd, opt => opt.MapFrom(src => src.Usd))
            .ReverseMap();

        CreateMap<EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnitsUsd, InfoFactUsGaapIncomeLossUnitsUsd>()
            .ForMember(dest => dest.Form, opt => opt.MapFrom(src => src.Form))
            .ForMember(dest => dest.Frame, opt => opt.MapFrom(src => src.Frame))
            .ForMember(dest => dest.Val, opt => opt.MapFrom(src => src.Val))
            .ReverseMap();

        CreateMap<FundingHandlerContext, CompanyFundingDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CompanyInfo.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CompanyInfo.EntityName))
            .ForMember(dest => dest.StandardFundableAmount, opt => opt.MapFrom(src => src.StandardFundableAmount))
            .ForMember(dest => dest.SpecialFundableAmount, opt => opt.MapFrom(src => src.SpecialFundableAmount));

        CreateMap<Fora.Data.Models.CompanyInfo, Fora.Service.CompanyService.CompanyDto>()
            .ForMember(dest => dest.Cik, opt => opt.MapFrom(src => src.Cik))
            .ForMember(dest => dest.EntityName, opt => opt.MapFrom(src => src.EntityName))
            .ForMember(dest => dest.Facts, opt => opt.MapFrom(src => src.Facts))
            .ReverseMap();

        CreateMap<Fora.Data.Models.InfoFact, Fora.Service.CompanyService.InfoFact>()
            .ForMember(dest => dest.UsGaap, opt => opt.MapFrom(src => src.UsGaap))
            .ReverseMap();

        CreateMap<Fora.Data.Models.InfoFactUsGaap, Fora.Service.CompanyService.InfoFactUsGaap>()
            .ForMember(dest => dest.NetIncomeLoss, opt => opt.MapFrom(src => src.NetIncomeLoss))
            .ReverseMap();

        CreateMap<Fora.Data.Models.InfoFactUsGaapNetIncomeLoss, Fora.Service.CompanyService.InfoFactUsGaapNetIncomeLoss>()
            .ForMember(dest => dest.Units, opt => opt.MapFrom(src => src.Units))
            .ReverseMap();

        CreateMap<Fora.Data.Models.InfoFactUsGaapIncomeLossUnits, Fora.Service.CompanyService.InfoFactUsGaapIncomeLossUnits>()
            .ForMember(dest => dest.Usd, opt => opt.MapFrom(src => src.Usd))
            .ReverseMap();

        CreateMap<Fora.Data.Models.InfoFactUsGaapIncomeLossUnitsUsd, Fora.Service.CompanyService.InfoFactUsGaapIncomeLossUnitsUsd>()
            .ForMember(dest => dest.Form, opt => opt.MapFrom(src => src.Form))
            .ForMember(dest => dest.Frame, opt => opt.MapFrom(src => src.Frame))
            .ForMember(dest => dest.Val, opt => opt.MapFrom(src => src.Val))
            .ReverseMap();
    }

    private ICollection<InfoFactUsGaapIncomeLossUnits> MapUnits(EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnits source)
    {
        return source.Usd.Select(x => new InfoFactUsGaapIncomeLossUnits
        {
            Usd = new List<InfoFactUsGaapIncomeLossUnitsUsd>
            {
                new()
                {
                    Form = x.Form,
                    Frame = x.Frame,
                    Val = x.Val
                }
            }
        }).ToList();
    }
}