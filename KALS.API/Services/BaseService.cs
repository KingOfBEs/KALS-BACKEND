using System.Security.Claims;
using AutoMapper;
using KALS.Domain.DataAccess;
using KALS.Domain.Enums;
using KALS.Repository.Interface;

namespace KALS.API.Services;

public class BaseService<T> where T : class
{
    
    protected ILogger<T> _logger;
    protected IMapper _mapper;
    protected IHttpContextAccessor _httpContextAccessor;
    protected IConfiguration _configuration;
    
    public BaseService(ILogger<T> logger, IMapper mapper,
        IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        
        _logger = logger;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }
    protected RoleEnum GetRoleFromJwt()
    {
        string roleString = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(roleString)) return RoleEnum.None;
        
        Enum.TryParse<RoleEnum>(roleString, out RoleEnum role);
        return role;
        
    }

    protected Guid GetUserIdFromJwt()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId");
        if (userIdClaim != null)
        {
            return Guid.Parse(userIdClaim.Value);
        }
        return Guid.Empty;
    }
}