using AutoMapper;
using KALS.API.Constant;
using KALS.API.Models.GoogleDrive;
using KALS.API.Models.Lab;
using KALS.API.Models.Product;
using KALS.API.Services.Interface;
using KALS.API.Utils;
using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Enums;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KALS.API.Services.Implement;

public class LabService: BaseService<LabService>, ILabService
{
    private readonly IProductRepository _productRepository;
    private readonly ILabProductRepository _labProductRepository;
    private readonly ILabRepository _labRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILabMemberRepository _labMemberRepository;
    private readonly IFirebaseService _firebaseService;
    private readonly IGoogleDriveService _googleDriveService;
    
    public LabService(ILogger<LabService> logger, IMapper mapper, 
        IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IProductRepository productRepository, 
        ILabProductRepository labProductRepository, ILabRepository labRepository, IMemberRepository memberRepository, 
        IUserRepository userRepository, ILabMemberRepository labMemberRepository, IFirebaseService firebaseService,
        IGoogleDriveService googleDriveService) : base(logger, mapper, httpContextAccessor, configuration)
    {
        _productRepository = productRepository;
        _labProductRepository = labProductRepository;
        _labRepository = labRepository;
        _memberRepository = memberRepository;
        _userRepository = userRepository;
        _labMemberRepository = labMemberRepository;
        _firebaseService = firebaseService;
        _googleDriveService = googleDriveService;
    }

    public async Task<GetProductResponse> AssignLabToProductAsync(Guid productId, AssignLabsToProductRequest request)
    {
        if(productId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        var product = await _productRepository.GetProductByIdAsync(productId);
        if(product == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
        
        var (newLabIds, removeLabIds) = await _labProductRepository.GetNewAndRemoveLabIdsAsync(productId, request.LabIds);
        if(!newLabIds.Any() && !removeLabIds.Any()) return _mapper.Map<GetProductResponse>(product);
        var members = await _memberRepository.GetMembersOrderProductAsync(productId);
        if (members != null)
        {
            var removeLabMembers = await _labMemberRepository.GetLabMembersByLabIds(removeLabIds);
            if (removeLabIds.Any())
            {
                foreach (var labMember in removeLabMembers)
                {
                    _labMemberRepository.DeleteAsync(labMember);
                }
            }
            if (newLabIds.Any())
            {
                foreach (var member in members)
                {
                    foreach (var newLabId in newLabIds)
                    {
                        bool isMemberInLab = await _labMemberRepository.IsMemberInLab(member.Id, newLabId);
                        if (!isMemberInLab)
                        {
                            await _labMemberRepository.InsertAsync(
                                new LabMember()
                                {
                                    LabId = newLabId,
                                    MemberId = member.Id
                                }
                            ); 
                        }
                    }
                }
            }
        }
        if (removeLabIds.Any())
        {
            // var removeLabProducts = product.LabProducts.Where(lp => removeLabIds.Contains(lp.LabId)).ToList();
            var removeLabProducts = await _labProductRepository.GetLabProductsByLabIds(removeLabIds);
            foreach (var removeLabProduct in removeLabProducts)
            {
                _labProductRepository.DeleteAsync(removeLabProduct);
            }
        }
        if (newLabIds.Any())
        {
            foreach (var newLabId in newLabIds)
            {
                var newLab = await _labRepository.GetLabByIdAsync(newLabId);
                if (newLab != null)
                {
                    await _labProductRepository.InsertAsync(new LabProduct()
                    {
                        LabId = newLabId,
                        ProductId = productId
                    });
                }
            }
        }
        GetProductResponse response = null;
        bool isSuccess = await _labProductRepository.SaveChangesAsync();
        if(isSuccess) response = _mapper.Map<GetProductResponse>(product);
        return response;
    }

    public async Task<IPaginate<LabResponse>> GetLabsAsync(int page, int size, string? searchName)
    {
        var role = GetRoleFromJwt();
        IPaginate<LabResponse> labsResponse;
        switch (role)
        {
            case RoleEnum.Member:
                var userId = GetUserIdFromJwt();
                if (userId == Guid.Empty) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);
                
                var member = await _memberRepository.GetMemberByUserId(userId);
                if (member == null) throw new BadHttpRequestException(MessageConstant.User.MemberNotFound);
                
                var labsByMember = await _labRepository.GetLabsPagingByMemberId(member.Id, page, size, searchName);
                labsResponse = _mapper.Map<IPaginate<LabResponse>>(labsByMember);
                labsResponse.Items.Select(lr => lr.ProductNames = labsByMember.Items.SelectMany(l => l.LabProducts)
                    .Where(lp => lp.LabId == lr.Id)
                    .Select(lp => lp.Product.Name).ToList());
                break;
            case RoleEnum.Manager:
            case RoleEnum.Staff:
                var labsByManager = await _labRepository.GetLabsPagingAsync(page, size, searchName);
                labsResponse = _mapper.Map<IPaginate<LabResponse>>(labsByManager);
                break;
            default:
                throw new BadHttpRequestException(MessageConstant.User.RoleNotFound);
        }
        return labsResponse;
    }

    public async Task<LabResponse> GetLabByIdAsync(Guid labId)
    {
        if(labId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Lab.LabIdNotNull);
        var lab = await _labRepository.GetLabByIdAsync(labId);
        
        return _mapper.Map<LabResponse>(lab);
    }

    public async Task<ProductWithLabResponse> GetLabsByProductIdAsync(Guid productId)
    {
        if (productId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        var product = await _productRepository.GetProductByIdAsync(productId);
        var productWithLabResponse = _mapper.Map<ProductWithLabResponse>(product);
        return productWithLabResponse;
    }

    public async Task<LabResponse> CreateLabAsync(CreateLabRequest request)
    {
        var userId = GetUserIdFromJwt();
        if (userId == Guid.Empty) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);
        // var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
        //     predicate: u => u.Id == userId
        // );
        var user = await _userRepository.GetUserByIdAsync(userId);
        if(user == null) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);
        
        var googleDriveResponse = await _googleDriveService.UploadToGoogleDrive(request.File);
        if (googleDriveResponse == null) throw new BadHttpRequestException(MessageConstant.Lab.UploadFileFail);
        if(string.IsNullOrEmpty(request.Name)) request.Name = request.File.FileName;
        
        var lab = _mapper.Map<Lab>(request);
        lab.Id = Guid.NewGuid();
        lab.Url = googleDriveResponse.Url;
        lab.CreatedAt = TimeUtil.GetCurrentSEATime();
        lab.ModifiedAt = TimeUtil.GetCurrentSEATime();
        lab.CreatedBy = user.Id;
        lab.ModifiedBy = user.Id;
        
        await _labRepository.InsertAsync(lab);
        bool isSuccess = await _labRepository.SaveChangesAsync();
        LabResponse labResponse = null;
        if (isSuccess) labResponse = _mapper.Map<LabResponse>(lab);
        return labResponse;
    }

    public async Task<LabResponse> UpdateLabAsync(Guid labId, UpdateLabRequest request)
    {
        if (labId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Lab.LabIdNotNull);
        var lab = await _labRepository.GetLabByIdAsync(labId);
        if (lab == null) throw new BadHttpRequestException(MessageConstant.Lab.LabNotFound);

        if (request.File != null)
        {
            var googleDriveResponse = await _googleDriveService.UploadToGoogleDrive(request.File);
            if (googleDriveResponse.Url == null) throw new BadHttpRequestException(MessageConstant.Lab.UploadFileFail);
            lab.Url = googleDriveResponse.Url;
        }

        lab.Name = !string.IsNullOrEmpty(request.Name) ? request.Name : lab.Name;

        _labRepository.UpdateAsync(lab);
        var isSuccess = await _labRepository.SaveChangesAsync();
        LabResponse response = null;
        if (isSuccess) response = _mapper.Map<LabResponse>(lab);
        return response;
    }
}