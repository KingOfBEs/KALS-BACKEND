using System.Text;
using KALS.API.Services.Implement;
using KALS.API.Services.Interface;
using KALS.Domain.DataAccess;
using KALS.Repository.Implement;
using KALS.Repository.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace KALS.API.Extensions;

public static class DependencyService
{
    // public static IServiceCollection AddUnitOfWork(this IServiceCollection service)
    // {
    //     service.AddScoped<IUnitOfWork<KitAndLabDbContext>, UnitOfWork<KitAndLabDbContext>>();
    //     return service;
    // }

    public static IServiceCollection AddRepositories(this IServiceCollection service)
    {
        service.AddScoped<IProductRepository, ProductRepository>();
        service.AddScoped<ICategoryRepository, CategoryRepository>();
        service.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        service.AddScoped<ILabProductRepository, LabProductRepository>();
        service.AddScoped<IMemberRepository, MemberRepository>();
        service.AddScoped<ILabRepository, LabRepository>();
        service.AddScoped<IUserRepository, UserRepository>();
        service.AddScoped<IOrderRepository, OrderRepository>();
        service.AddScoped<IOrderItemRepository, OrderItemRepository>();
        service.AddScoped<ILabMemberRepository, LabMemberRepository>();
        service.AddScoped<IPaymentRepository, PaymentRepository>();
        service.AddScoped<IProductImageRepository, ProductImageRepository>();
        service.AddScoped<IProductRelationshipRepository, ProductRelationshipRepository>();
        service.AddScoped<IStaffRepository, StaffRepository>();
        service.AddScoped<ISupportRequestRepository, SupportRequestRepository>();
        service.AddScoped<ISupportMessageRepository, SupportMessageRepository>();
        return service;
    }
    public static IServiceCollection AddDatabase(this IServiceCollection service)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
        service.AddDbContext<KitAndLabDbContext>(options => options.UseSqlServer(CreateConnectionString(configuration)));
        return service;
    }

    public static IServiceCollection AddRedis(this IServiceCollection service)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
        service.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));
        service.AddScoped<IRedisService, RedisService>();
        return service;
    }
    private static string CreateConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("ConnectionStrings:MyConnectionString");
        return connectionString;
    }
    public static IServiceCollection AddServices(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddScoped<IUserService, UserService>();
        service.AddScoped<IProductService, ProductService>();
        service.AddScoped<ILabService, LabService>();
        service.AddScoped<ICategoryService, CategoryService>();
        service.AddScoped<ICartService, CartService>();
        service.AddScoped<IPaymentService, PaymentService>();
        service.AddScoped<IOrderService, OrderService>();
        service.AddScoped<ISupportRequestService, SupportRequestService>();
        service.AddScoped<ISupportMessageImageRepository, SupportMessageImageRepository>();
        service.AddHttpClient<IFirebaseService, FirebaseService>();
        service.AddScoped<IGoogleDriveService, GoogleDriveService>();
        return service;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection service)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
        service.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = configuration["JWT:Issuer"],
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!)),
                    };
                });
        return service;
    }
    public static IServiceCollection AddConfigSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo() {Title = "Kit And Lab System", Version = "v1"});
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer",
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
            options.MapType<TimeOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "time",
                Example = OpenApiAnyFactory.CreateFromJson("\"13:45:42.0000000\"")
            });
        });
        return services;
    }
    
    
}