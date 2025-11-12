using Mapster;
using Salahly.DAL.Entities;
using System;

namespace Salahly.DSL.DTOs.Configurations
{
    public static class CraftsmanMappingConfig
    {
        public static void RegisterCraftsmanMappings(TypeAdapterConfig config)
        {
            // Craftsman to CraftsmanDto
            config.NewConfig<Craftsman, CraftsmanDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.CraftId, src => src.CraftId)
                .Map(dest => dest.RatingAverage, src => src.RatingAverage)
                .Map(dest => dest.TotalCompletedBookings, src => src.TotalCompletedBookings)
                .Map(dest => dest.IsAvailable, src => src.IsAvailable)
                .Map(dest => dest.HourlyRate, src => src.HourlyRate)
                .Map(dest => dest.Bio, src => src.Bio)
                .Map(dest => dest.YearsOfExperience, src => src.YearsOfExperience)
                .Map(dest => dest.VerifiedAt, src => src.VerifiedAt)
                .Map(dest => dest.Portfolio, src => src.Portfolio)
                // Map CraftsmanServiceAreas to simplified ServiceAreaDto
                .Map(dest => dest.ServiceAreas, src => src.CraftsmanServiceAreas.Select(csa => csa.Adapt<ServiceAreaDto>()))
                .AfterMapping((src, dest) =>
                {
                    if (src.User != null)
                    {
                        dest.ProfileImageUrl = src.User.ProfileImageUrl;
                        dest.FullName = src.User.FullName;
                    }
                });

            // CreateCraftsmanDto to Craftsman
            config.NewConfig<CreateCraftsmanDto, Craftsman>()
                .Map(dest => dest.CraftId, src => src.CraftId)
                .Map(dest => dest.Bio, src => src.Bio)
                .Map(dest => dest.YearsOfExperience, src => src.YearsOfExperience)
                .Map(dest => dest.HourlyRate, src => src.HourlyRate)
                .Map(dest => dest.IsAvailable, src => true)
                .Map(dest => dest.RatingAverage, src => 0m)
                .Map(dest => dest.TotalCompletedBookings, src => 0);

            // UpdateCraftsmanDto to Craftsman
            config.NewConfig<UpdateCraftsmanDto, Craftsman>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.CraftId, src => src.CraftId)
                .Map(dest => dest.Bio, src => src.Bio)
                .Map(dest => dest.YearsOfExperience, src => src.YearsOfExperience)
                .Map(dest => dest.HourlyRate, src => src.HourlyRate);
        }
    }
}
