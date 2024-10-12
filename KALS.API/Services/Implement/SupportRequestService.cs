using System.Transactions;
using AutoMapper;
using KALS.API.Constant;
using KALS.API.Models.SupportRequest;
using KALS.API.Services.Interface;
using KALS.API.Utils;
using KALS.Domain.Entities;
using KALS.Domain.Enums;
using KALS.Repository.Interface;
using SupportRequest = KALS.Domain.Entities.SupportRequest;

namespace KALS.API.Services.Implement;

public class SupportRequestService: BaseService<SupportRequestService>, ISupportRequestService
{
    private readonly IMemberRepository _memberRepository;
    private readonly ISupportRequestRepository _supportRequestRepository;
    private readonly ILabMemberRepository _labMemberRepository;
    private readonly ISupportMessageRepository _supportMessageRepository;
    private readonly IStaffRepository _staffRepository;
    
    public SupportRequestService(ILogger<SupportRequestService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, 
        IConfiguration configuration, IMemberRepository memberRepository, ISupportRequestRepository supportRequestRepository, 
        ILabMemberRepository labMemberRepository, ISupportMessageRepository supportMessageRepository, IStaffRepository staffRepository) : base(logger, mapper, httpContextAccessor, configuration)
    {
        _memberRepository = memberRepository;
        _supportRequestRepository = supportRequestRepository;
        _labMemberRepository = labMemberRepository;
        _supportMessageRepository = supportMessageRepository;
        _staffRepository = staffRepository;
    }

    public async Task<SupportRequestResponse> CreateSupportRequest(Models.SupportRequest.SupportRequest request)
    {
        if (request.LabId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Lab.LabIdNotNull);
        
        var userId = GetUserIdFromJwt();
        if (userId == Guid.Empty) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);

        var member = await _memberRepository.GetMemberByUserId(userId);
        if(member == null) throw new BadHttpRequestException(MessageConstant.User.MemberNotFound);

        var labMember = await _labMemberRepository.GetLabMemberByLabIdAndMemberId(request.LabId, member.Id);
        if (labMember == null) throw new BadHttpRequestException(MessageConstant.LabMember.LabMemberNotFound);
        if (labMember.NumberOfRequest <= 0)
            throw new BadHttpRequestException(MessageConstant.SupportRequest.SupportRequestNumberIsOut);
        var openingSupportRequests = await _supportRequestRepository.GetSupportRequestIsOpen(member.Id);
        if (openingSupportRequests.Count > 0)
            throw new BadHttpRequestException(MessageConstant.SupportRequest.AnotherSupportRequestIsOpening);
        var supportRequest = new SupportRequest()
        {
            Id = Guid.NewGuid(),
            CreatedAt = TimeUtil.GetCurrentSEATime(),
            ModifiedAt = TimeUtil.GetCurrentSEATime(),
            Status = SupportRequestStatus.Open,
            MemberId = member.Id,
            LabId = labMember.LabId
        };
        var supportMessage = new SupportMessage()
        {
            Id = Guid.NewGuid(),
            CreatedAt = TimeUtil.GetCurrentSEATime(),
            ModifiedAt = TimeUtil.GetCurrentSEATime(),
            Content = request.Content,
            Type = SupportMessageType.Request,
            SupportRequest = supportRequest
        };

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                await _supportMessageRepository.InsertAsync(supportMessage);
                await _supportRequestRepository.InsertAsync(supportRequest);
                // labMember.NumberOfRequest -= 1;
                // _labMemberRepository.UpdateAsync(labMember);
                var isSuccess = await _supportRequestRepository.SaveChangesAsync();
                transaction.Complete();
                SupportRequestResponse response = null;
                if (isSuccess) response = _mapper.Map<SupportRequestResponse>(supportRequest);
                return response;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }

    public async Task<SupportRequestResponse> ResponseSupportMessage( Guid supportRequestId, ResponseSupportRequest request)
    {
        if (supportRequestId == Guid.Empty)
            throw new BadHttpRequestException(MessageConstant.SupportRequest.SupportRequestIdNotNull);
        
        var userId = GetUserIdFromJwt();
        if (userId == Guid.Empty) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);
        
        var staff = await _staffRepository.GetStaffByUserIdAsync(userId);
        if (staff == null) throw new BadHttpRequestException(MessageConstant.User.StaffNotFound);
        
        var supportRequest = await _supportRequestRepository.GetSupportRequestById(supportRequestId);
        if (supportRequest == null) throw new BadHttpRequestException(MessageConstant.SupportRequest.SupportRequestNotFound);
        if (supportRequest.Status == SupportRequestStatus.Closed)
            throw new BadHttpRequestException(MessageConstant.SupportRequest.SupportRequestClosed);
        
        supportRequest.StaffId = staff.Id;
        supportRequest.Status = SupportRequestStatus.Closed;
        supportRequest.ModifiedAt = TimeUtil.GetCurrentSEATime();
        var supportMessage = new SupportMessage()
        {
            Id = Guid.NewGuid(),
            CreatedAt = TimeUtil.GetCurrentSEATime(),
            ModifiedAt = TimeUtil.GetCurrentSEATime(),
            Content = request.Content,
            Type = SupportMessageType.Response,
            SupportRequestId = supportRequest.Id,
            SupportRequest = supportRequest
        };
        
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                await _supportMessageRepository.InsertAsync(supportMessage);
                _supportRequestRepository.UpdateAsync(supportRequest);
                var isSuccess = await _supportRequestRepository.SaveChangesAsync();
                transaction.Complete();
                SupportRequestResponse response = null;
                if (isSuccess) response = _mapper.Map<SupportRequestResponse>(supportRequest);
                return response;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        

    }
}