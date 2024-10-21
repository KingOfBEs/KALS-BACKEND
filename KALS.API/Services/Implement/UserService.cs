using System.Transactions;
using AutoMapper;
using KALS.API.Constant;
using KALS.API.Models;
using KALS.API.Models.Sms;
using KALS.API.Models.User;
using KALS.API.Services.Interface;
using KALS.API.Utils;
using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Enums;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using BadHttpRequestException = Microsoft.AspNetCore.Http.BadHttpRequestException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace KALS.API.Services.Implement;

public class UserService : BaseService<UserService>, IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IRedisService _redisService;
    public UserService(ILogger<UserService> logger, IMapper mapper, 
        IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IUserRepository userRepository,
        IMemberRepository memberRepository, IStaffRepository staffRepository, IRedisService redisService) : base(logger, mapper, httpContextAccessor, configuration)
    {
        _userRepository = userRepository;
        _memberRepository = memberRepository;
        _staffRepository = staffRepository;
        _redisService = redisService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
        //     predicate: u => u.Username == request.UsernameOrPhoneNumber || u.PhoneNumber == request.UsernameOrPhoneNumber);
        var user = await _userRepository.GetUserByUsernameOrPhoneNumber(request.UsernameOrPhoneNumber);
        if (user == null) throw new BadHttpRequestException(MessageConstant.User.UserNotFound);

        if(user.Password != PasswordUtil.HashPassword(request.Password)) throw new BadHttpRequestException(MessageConstant.User.PasswordIncorrect);
        var guidClaim = new Tuple<string, Guid>("userId", user.Id);
        var token = JwtUtil.GenerateJwtToken(user, guidClaim, _configuration);
        var refreshToken = JwtUtil.GenerateRefreshToken();
        var loginResponse = new LoginResponse()
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName, 
            PhoneNumber = user.PhoneNumber,
            Token = token,
            RefreshToken = refreshToken
        };
        return loginResponse;
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        
        // var userList = await _unitOfWork.GetRepository<User>().GetListAsync();
        var userList = await _userRepository.GetAllUsers();
        if (userList.Any(u => u.Username == request.Username)) throw new BadHttpRequestException(MessageConstant.User.UserNameExisted);
        if (userList.Any(u => u.PhoneNumber == request.PhoneNumber)) throw new BadHttpRequestException(MessageConstant.User.PhoneNumberExisted);
        var user = _mapper.Map<User>(request);
        // var redis = ConnectionMultiplexer.Connect(_configuration.GetConnectionString("Redis"));
        // var db = redis.GetDatabase();
        var key = request.PhoneNumber;
        var existingOtp = await _redisService.GetStringAsync(key);
        
        if (string.IsNullOrEmpty(existingOtp)) throw new BadHttpRequestException(MessageConstant.Sms.OtpNotFound);
        if (existingOtp != request.Otp) throw new BadHttpRequestException(MessageConstant.Sms.OtpIncorrect);
        
        user.Password = PasswordUtil.HashPassword(request.Password);
        user.Role = RoleEnum.Member;
        var member = new Member()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user
        };
        // await _unitOfWork.GetRepository<User>().InsertAsync(user);
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                await _memberRepository.InsertAsync(member);
                var isSuccess = await _memberRepository.SaveChangesAsync();
                transaction.Complete();
                LoginResponse response = null;
                if (isSuccess)
                {
                    response = _mapper.Map<LoginResponse>(user);
                    response.Token = JwtUtil.GenerateJwtToken(user, new Tuple<string, Guid>("userId", user.Id),
                        _configuration);
                    response.RefreshToken = JwtUtil.GenerateRefreshToken();
                }

                return response;
            }
            catch (TransactionException ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
    }

    public async Task<string> GenerateOtpAsync(GenerateOtpRequest request)
    {
        // var redis = ConnectionMultiplexer.Connect(_configuration.GetConnectionString("Redis"));
        // var db = redis.GetDatabase();
        var key = request.PhoneNumber;
        
        var existingOtp = await _redisService.GetStringAsync(key);
        if (!string.IsNullOrEmpty(existingOtp)) throw new BadHttpRequestException(MessageConstant.Sms.OtpAlreadySent);
        
        if(request.PhoneNumber == null) throw new BadHttpRequestException(MessageConstant.User.PhoneNumberNotFound);
        var phoneNumberArray = new string[] { request.PhoneNumber };
        var otp = OtpUtil.GenerateOtp();
        var content = "Mã OTP của bạn là: " + otp;
        var response = SmsUtil.sendSMS(phoneNumberArray, content, _configuration);
        _logger.LogInformation(response);
        var smsResponse = JsonSerializer.Deserialize<SmsModel.SmsResponse>(response);
        if (smsResponse.status != "success" && smsResponse.code != "00")
        {
            throw new BadHttpRequestException(MessageConstant.Sms.SendSmsFailed);
        }
        
        await _redisService.SetStringAsync(key, otp, TimeSpan.FromMinutes(2));
        return request.PhoneNumber;
    }

    public async Task<UserResponse> ForgetPassword(ForgetPasswordRequest request)
    {
        // var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
        //     predicate: u => u.PhoneNumber == request.PhoneNumber
        // );
        var user = await _userRepository.GetUserByPhoneNumber(request.PhoneNumber);
        if (user == null) throw new BadHttpRequestException(MessageConstant.User.UserNotFound);
        // var redis = ConnectionMultiplexer.Connect(_configuration.GetConnectionString("Redis"));
        // var db = redis.GetDatabase();
        var key = request.PhoneNumber;
        var existingOtp = await _redisService.GetStringAsync(key);
        
        if (string.IsNullOrEmpty(existingOtp)) throw new BadHttpRequestException(MessageConstant.Sms.OtpNotFound);
        if (existingOtp != request.Otp) throw new BadHttpRequestException(MessageConstant.Sms.OtpIncorrect);
        
        user.Password = PasswordUtil.HashPassword(request.Password);
        // _unitOfWork.GetRepository<User>().UpdateAsync(user);
        _userRepository.UpdateAsync(user);
        UserResponse response = null;
        var isSuccess = await _userRepository.SaveChangesAsync();
        if (isSuccess) response = _mapper.Map<UserResponse>(user);
        return response;
    }

    public async Task<IPaginate<MemberResponse>> GetMembersAsync(int page, int size, MemberFilter? filter, string? sortBy, bool isAsc)
    {
        var members = await _memberRepository.GetMembersPagingAsync(page, size, filter, sortBy, isAsc);
        var response = _mapper.Map<IPaginate<MemberResponse>>(members);
        return response;
    }

    public async Task<MemberResponse> GetMemberInformationAsync()
    {
        var userId = GetUserIdFromJwt();
        if (userId == Guid.Empty) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);
        
        var member = await _memberRepository.GetMemberByUserId(userId);
        if (member == null) throw new BadHttpRequestException(MessageConstant.User.MemberNotFound);
        
        var response = _mapper.Map<MemberResponse>(member);
        response.Username = member.User.Username;
        response.FullName = member.User.FullName;
        response.PhoneNumber = member.User.PhoneNumber;
        return response;
    }

    public async Task<IPaginate<StaffResponse>> GetStaffsAsync(int page, int size, StaffFilter? filter, string? sortBy, bool isAsc)
    {
        var staffs = await _staffRepository.GetStaffPagingAsync(page, size, filter, sortBy, isAsc);
        var response = _mapper.Map<IPaginate<StaffResponse>>(staffs);
        return response;
    }

    public async Task<UserResponse> UpdateMemberAsyncByManager(Guid id, UpdateMemberRequest request)
    {
        if (id == Guid.Empty) throw new BadHttpRequestException(MessageConstant.User.UserIdNotNull);
        
        var member = await _memberRepository.GetMemberByUserId(id);
        if (member == null) throw new BadHttpRequestException(MessageConstant.User.MemberNotFound);
        request.TrimString();
        member.Commune = string.IsNullOrEmpty(request.Commune) ? member.Commune : request.Commune;
        member.Province = string.IsNullOrEmpty(request.Province) ? member.Province : request.Province;
        member.District = string.IsNullOrEmpty(request.District) ? member.District : request.District;
        member.Address = string.IsNullOrEmpty(request.Address) ? member.Address : request.Address;
        member.ProvinceCode = request.ProvinceCode ?? member.ProvinceCode;
        member.DistrictCode = request.DistrictCode ?? member.DistrictCode;
        member.CommuneCode = request.CommuneCode ?? member.CommuneCode;
        member.User.Username = string.IsNullOrEmpty(request.Username) ? member.User.Username : request.Username;
        member.User.FullName = string.IsNullOrEmpty(request.FullName) ? member.User.FullName : request.FullName;
        
        // _unitOfWork.GetRepository<Member>().UpdateAsync(member);
        _memberRepository.UpdateAsync(member);
        var isSuccess = await _memberRepository.SaveChangesAsync();
        UserResponse response = null;
        if (isSuccess) response = _mapper.Map<UserResponse>(member.User);
        return response;

    }

    public async Task<UserResponse> UpdateMemberAsync(UpdateMemberRequest request)
    {
        var userId = GetUserIdFromJwt();
        if (userId == Guid.Empty) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);
        // var member = await _unitOfWork.GetRepository<Member>().SingleOrDefaultAsync(
        //     predicate: m => m.UserId == userId,
        //     include: m => m.Include(m => m.User)
        // );
        var member = await _memberRepository.GetMemberByUserId(userId);
        if (member == null) throw new BadHttpRequestException(MessageConstant.User.MemberNotFound);
        request.TrimString();
        member.Commune = string.IsNullOrEmpty(request.Commune) ? member.Commune : request.Commune;
        member.Province = string.IsNullOrEmpty(request.Province) ? member.Province : request.Province;
        member.District = string.IsNullOrEmpty(request.District) ? member.District : request.District;
        member.Address = string.IsNullOrEmpty(request.Address) ? member.Address : request.Address;
        member.ProvinceCode = request.ProvinceCode ?? member.ProvinceCode;
        member.DistrictCode = request.DistrictCode ?? member.DistrictCode;
        member.CommuneCode = request.CommuneCode ?? member.CommuneCode;
        member.User.Username = string.IsNullOrEmpty(request.Username) ? member.User.Username : request.Username;
        member.User.FullName = string.IsNullOrEmpty(request.FullName) ? member.User.FullName : request.FullName;
        
        // _unitOfWork.GetRepository<Member>().UpdateAsync(member);
        // var isSuccess = await _unitOfWork.CommitAsync() > 0;
        _memberRepository.UpdateAsync(member);
        var isSuccess = await _memberRepository.SaveChangesAsync();
        UserResponse response = null;
        if (isSuccess) response = _mapper.Map<UserResponse>(member.User);
        return response;
    }

    public async Task<UserResponse> UpdateStaffAsync(Guid staffId, UpdateStaffRequest request)
    {
        if (staffId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.User.StaffIdNotNull);
        var staff = await _staffRepository.GetStaffByIdAsync(staffId);
        if (staff == null) throw new BadHttpRequestException(MessageConstant.User.StaffNotFound);
        request.TrimString();
        staff.Type = (StaffType)(!request.Type.HasValue ? staff.Type : request.Type);
        staff.User.Username = string.IsNullOrEmpty(request.Username) ? staff.User.Username : request.Username;
        staff.User.Password = string.IsNullOrEmpty(request.Password) ? staff.User.Password : PasswordUtil.HashPassword(request.Password);
        staff.User.FullName = string.IsNullOrEmpty(request.FullName) ? staff.User.FullName : request.FullName;
        
        // _unitOfWork.GetRepository<Staff>().UpdateAsync(staff);
        _staffRepository.UpdateAsync(staff);
        var isSuccess = await _staffRepository.SaveChangesAsync();
        UserResponse response = null;
        if (isSuccess) response = _mapper.Map<UserResponse>(staff.User);
        return response;
    }

    public async Task<StaffResponse> CreateStaffAsync(CreateStaffRequest request)
    {
        var userList = await _userRepository.GetAllUsers();
        if (userList.Any(u => u.Username == request.Username)) throw new BadHttpRequestException(MessageConstant.User.UserNameExisted);
        if (userList.Any(u => u.PhoneNumber == request.PhoneNumber)) throw new BadHttpRequestException(MessageConstant.User.PhoneNumberExisted);
        var user = _mapper.Map<User>(request);
        user.Password = PasswordUtil.HashPassword(request.Password);
        user.Role = RoleEnum.Staff;
        var staff = new Staff()
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            UserId = user.Id,
            User = user
        };
        await _staffRepository.InsertAsync(staff);
        StaffResponse response = null;
        var isSuccess = await _staffRepository.SaveChangesAsync();
        if(isSuccess) return _mapper.Map<StaffResponse>(staff);
        return response;
    }

    public async Task<StaffResponse> GetStaffById(Guid id)
    {
        if (id == Guid.Empty) throw new BadHttpRequestException(MessageConstant.User.StaffIdNotNull);
        var staff = await _staffRepository.GetStaffByIdAsync(id);
        if (staff == null) throw new BadHttpRequestException(MessageConstant.User.StaffNotFound);
        var response = _mapper.Map<StaffResponse>(staff);
        return response;
    }
}