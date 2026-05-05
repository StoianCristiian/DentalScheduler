using DentalScheduler.Application.Appointments.Queries.GetDoctors;
using DentalScheduler.Application.Interfaces;
using DentalScheduler.Infrastructure.Identity;
using DentalScheduler.Infrastructure.Persistance;
using DentalScheduler.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon.S3;

namespace DentalScheduler.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IDoctorRoleChecker, DoctorRoleChecker>();

            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddApiEndpoints();

            services.AddAuthentication()
                .AddBearerToken(IdentityConstants.BearerScheme);

            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IPaymentService, StripePaymentService>();
            services.AddHttpClient<ISmartSchedulingService, SmartSchedulingService>(client => 
            {
                client.BaseAddress = new Uri(configuration["AiServiceUrl"] ?? "http://localhost:5000");
            });
            
            // Add AWS S3
            var awsOptions = configuration.GetAWSOptions();
            var accessKey = configuration["AWS_ACCESS_KEY_ID"];
            var secretKey = configuration["AWS_SECRET_ACCESS_KEY"];
            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
            {
                awsOptions.Credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
            }
            var region = configuration["AWS_REGION"];
            if (!string.IsNullOrEmpty(region))
            {
                awsOptions.Region = Amazon.RegionEndpoint.GetBySystemName(region);
            }
            
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonS3>();
            services.AddScoped<IAwsS3Service, AwsS3Service>();

            return services;
        }
    }
}