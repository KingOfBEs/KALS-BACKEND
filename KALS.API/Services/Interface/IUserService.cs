using KALS.API.Models;
using KALS.API.Models.User;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;

namespace KALS.API.Services.Interface;

public interface IUserService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    
    Task<string> GenerateOtpAsync(GenerateOtpRequest request);
    
    Task<UserResponse> ForgetPassword(ForgetPasswordRequest request);
    
    Task<IPaginate<MemberResponse>> GetMembersAsync(int page, int size, MemberFilter? filter, string? sortBy, bool isAsc);
    
    Task<MemberResponse> GetMemberInformationAsync();
    Task<IPaginate<StaffResponse>> GetStaffsAsync(int page, int size, StaffFilter? filter, string? sortBy, bool isAsc);
    
    Task<UserResponse> UpdateMemberAsyncByManager(Guid id, UpdateMemberRequest request);
    
    Task<UserResponse> UpdateMemberAsync(UpdateMemberRequest request);
    Task<UserResponse> UpdateStaffAsync(Guid id, UpdateStaffRequest request);
    
    Task<StaffResponse> CreateStaffAsync(CreateStaffRequest request);
    
    
}