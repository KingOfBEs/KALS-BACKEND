using AutoMapper;
using KALS.API.Constant;
using KALS.API.Models.WarrantyRequest;
using KALS.API.Services.Interface;
using KALS.API.Utils;
using KALS.Domain.Entities;
using KALS.Domain.Enums;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using BadHttpRequestException = Microsoft.AspNetCore.Http.BadHttpRequestException;

namespace KALS.API.Services.Implement;

public class WarrantyRequestService: BaseService<WarrantyRequestService>, IWarrantyRequestService
{
    private readonly IWarrantyRequestRepository _warrantyRequestRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IFirebaseService _firebaseService;
    private readonly IMemberRepository _memberRepository;
    private readonly IStaffRepository _staffRepository;
    public WarrantyRequestService(ILogger<WarrantyRequestService> logger, 
        IMapper mapper, IHttpContextAccessor httpContextAccessor, 
        IConfiguration configuration, IWarrantyRequestRepository warrantyRequestRepository, IOrderItemRepository orderItemRepository,
        IFirebaseService firebaseService, IMemberRepository memberRepository, IStaffRepository staffRepository) : base(logger, mapper, httpContextAccessor, configuration)
    {
        _warrantyRequestRepository = warrantyRequestRepository;
        _orderItemRepository = orderItemRepository;
        _firebaseService = firebaseService;
        _memberRepository = memberRepository;
        _staffRepository = staffRepository;
    }

    public async Task<IPaginate<WarrantyRequestWithImageResponse>> GetWarrantyRequestsAsync(int page, int size, Guid? memberId)
    {
        var role = GetRoleFromJwt();
        IPaginate<WarrantyRequestWithImageResponse> response = null;
        switch (role)
        {
            case RoleEnum.Member:
                var warrantyRequestWithMembers = await _warrantyRequestRepository.GetWarrantyRequestsAsync(page, size, memberId);
                response = _mapper.Map<IPaginate<WarrantyRequestWithImageResponse>>(warrantyRequestWithMembers);
                break;
            case RoleEnum.Manager:
            case RoleEnum.Staff:
                var warrantyRequests =  await _warrantyRequestRepository.GetWarrantyRequestsAsync(page, size, null);
                response = _mapper.Map<IPaginate<WarrantyRequestWithImageResponse>>(warrantyRequests);
                break;
            default:
                throw new UnauthorizedAccessException(MessageConstant.User.RoleNotFound);
        }

        return response;
    }

    public async Task<WarrantyRequestWithImageResponse> CreateWarrantyRequestAsync(CreateWarrantyRequestRequest request)
    {
        var orderItem = await _orderItemRepository.GetOrderItemByIdAsync(request.OrderItemId);
        if (orderItem == null) throw new BadHttpRequestException(MessageConstant.OrderItem.OrderItemNotFound);
        if (orderItem.WarrantyExpired < TimeUtil.GetCurrentSEATime())
            throw new BadHttpRequestException(MessageConstant.WarrantyRequest.WarrantyExpired);
        var userId = GetUserIdFromJwt();
        var member = await _memberRepository.GetMemberByUserId(userId);
        if (member == null) throw new BadHttpRequestException(MessageConstant.User.MemberNotFound);
        
        var warrantyRequest = _mapper.Map<WarrantyRequest>(request);
        warrantyRequest.Status = WarrantyRequestStatus.WaitForResponse;
        warrantyRequest.CreatedAt = TimeUtil.GetCurrentSEATime();
        warrantyRequest.ModifiedAt = TimeUtil.GetCurrentSEATime();

        foreach (var imageBase64 in request.Images)
        {
            var imageUrl = await _firebaseService.UploadFileToFirebaseAsync(imageBase64);
            if(imageUrl == null) throw new BadHttpRequestException(MessageConstant.WarrantyRequest.UploadImageFailed);
            warrantyRequest.WarrantyRequestImages.Add(new WarrantyRequestImage
            {
                Id = Guid.NewGuid(),
                ImageUrl = imageUrl
            });
        }

        await _warrantyRequestRepository.InsertAsync(warrantyRequest);
        var isSuccess = await _warrantyRequestRepository.SaveChangesAsync();
        WarrantyRequestWithImageResponse response = null;
        if(isSuccess) response = _mapper.Map<WarrantyRequestWithImageResponse>(warrantyRequest);
        return response;
    }

    public async Task<WarrantyRequestWithImageResponse> UpdateWarrantyRequestAsync(Guid warrantyRequestId, UpdateWarrantyRequestRequest request)
    {
        if(request.Status != WarrantyRequestStatus.Accepted && request.Status != WarrantyRequestStatus.Denied)
            throw new BadHttpRequestException(MessageConstant.WarrantyRequest.WarrantyRequestStatusInvalid);
        if (warrantyRequestId == Guid.Empty)
            throw new BadHttpRequestException(MessageConstant.WarrantyRequest.WarrantyRequestIdNotNull);
        
        var warrantyRequest = await _warrantyRequestRepository.GetWarrantyRequestByIdAsync(warrantyRequestId);
        if (warrantyRequest == null) throw new BadHttpRequestException(MessageConstant.WarrantyRequest.WarrantyRequestNotFound);
        
        if (warrantyRequest.Status == WarrantyRequestStatus.Accepted)
            throw new BadHttpRequestException(MessageConstant.WarrantyRequest.WarrantyRequestAccepted);
        if(warrantyRequest.Status == WarrantyRequestStatus.Denied)
            throw new BadHttpRequestException(MessageConstant.WarrantyRequest.WarrantyRequestDenied);
        
        warrantyRequest.ResponseContent = request.ResponseContent;
        var staff = await _staffRepository.GetStaffByUserIdAsync(GetUserIdFromJwt());
        if (staff == null) throw new BadHttpRequestException(MessageConstant.User.StaffNotFound);
        warrantyRequest.ResponseBy = staff.Id;
        warrantyRequest.Status = request.Status;
        warrantyRequest.ModifiedAt = TimeUtil.GetCurrentSEATime();
        
        _warrantyRequestRepository.UpdateAsync(warrantyRequest);
        var isSuccess = await _warrantyRequestRepository.SaveChangesAsync();
        WarrantyRequestWithImageResponse response = null;
        if(isSuccess) response = _mapper.Map<WarrantyRequestWithImageResponse>(warrantyRequest);
        return response;
    }
}