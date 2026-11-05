using System.Text.Json;
using AutoMapper;
using MiniDpp.Domain.Entities;
using MiniDpp.Dtos.Passport;
using MiniDpp.Dtos.Product;

namespace MiniDpp.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Batch, BatchDto>();

            CreateMap<Product, ProductListDto>()
                .ForMember(d => d.Name, o => o.MapFrom(s => string.IsNullOrEmpty(s.Name) ? s.Model.Name : s.Name))
                .ForMember(d => d.ModelName, o => o.MapFrom(s => s.Model.Name))
                .ForMember(d => d.Batches, o => o.MapFrom(s => s.Batches.OrderBy(b => b.Id)));

            CreateMap<Product, PassportDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.ModelName, o => o.MapFrom(s => s.Model.Name))
                .ForMember(d => d.CarbonKgCo2e, o => o.MapFrom(s => (double?)s.CarbonKgCo2e))
                .ForMember(d => d.Batches, o => o.MapFrom(s => s.Batches.OrderBy(b => b.Id)))
                .ForMember(d => d.Role, o => o.Ignore());

            CreateMap<Product, ProductSnapshotDto>()
                .ForMember(d => d.ModelName, o => o.MapFrom(s => s.Model.Name))
                .ForMember(d => d.Batches, o => o.MapFrom(s => s.Batches.OrderBy(b => b.BatchNumber)));

            CreateMap<ProductVersion, ProductVersionDto>()
                .ForMember(d => d.Data, o => o.MapFrom(s =>
                    JsonSerializer.Deserialize<ProductSnapshotDto>(s.SnapshotJson, JsonOptions.Default)));
        }
    }
}
