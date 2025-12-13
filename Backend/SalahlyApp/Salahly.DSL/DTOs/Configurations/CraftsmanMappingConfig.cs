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
                .Map(dest => dest.CraftName, src => src.Craft != null ? src.Craft.Name : null)
                .Map(dest => dest.JobTitle, src => src.Craft != null ? src.Craft.Name : null)
                .Map(dest => dest.RatingAverage, src => src.User.RatingAverage)
                .Map(dest => dest.TotalCompletedBookings, src => src.TotalCompletedBookings)
                .Map(dest => dest.IsAvailable, src => src.IsAvailable)
                .Map(dest => dest.HourlyRate, src => src.HourlyRate)
                .Map(dest => dest.Bio, src => src.Bio)
                .Map(dest => dest.YearsOfExperience, src => src.YearsOfExperience)
                .Map(dest => dest.VerificationStatus, src => src.VerificationStatus)
                .Map(dest => dest.Portfolio, src => src.Portfolio)
                .Map(dest => dest.IsVerified, src => src.IsVerified)
                .Map(dest => dest.Balance, src => src.Balance)
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
                //.Map(dest => dest.Id, src => src.UserId)
                .Map(dest => dest.CraftId, src => src.CraftId)
                .Map(dest => dest.Bio, src => src.Bio)
                .Map(dest => dest.YearsOfExperience, src => src.YearsOfExperience)
                .Map(dest => dest.HourlyRate, src => src.HourlyRate)
                .Map(dest => dest.IsAvailable, src => true)
                .Map(dest => dest.User.RatingAverage, src => 5)
                .Map(dest => dest.TotalCompletedBookings, src => 0);

            // UpdateCraftsmanDto to Craftsman
            config.NewConfig<UpdateCraftsmanDto, Craftsman>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.CraftId, src => src.CraftId)
                .Map(dest => dest.Bio, src => src.Bio)
                .Map(dest => dest.YearsOfExperience, src => src.YearsOfExperience)
                .Map(dest => dest.HourlyRate, src => src.HourlyRate);
            config.NewConfig<Craftsman, CraftsManAdminViewDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FullName, src => src.User.FullName)
                .Map(dest => dest.IsVerified, src => src.IsVerified);

            config.NewConfig<Craftsman, CraftsmanShortDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FullName, src => src.User.FullName)
                .Map(dest => dest.CraftName, src => src.Craft != null ? src.Craft.Name : null)
                .Map(dest => dest.RatingAverage, src => src.User.RatingAverage)
                .IgnoreNullValues(true);
        }
    }
}
