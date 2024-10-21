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
        var members = await _memberRepository.GetMembersOrderProductAsync(productId);
        if (members != null)
        {
            // var labMembers = await _labMemberRepository.GetListAsync();
            // var removeLabMembers = labMembers.Where(l => removeLabIds.Contains(l.LabId));
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
                        // bool isMemberInLab = labMembers.Any(lm => lm.LabId == newLabId && lm.MemberId == member.Id);
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
                // var member = await _unitOfWork.GetRepository<Member>().SingleOrDefaultAsync(
                //     predicate: m => m.UserId == userId
                // );
                var member = await _memberRepository.GetMemberByUserId(userId);
                if (member == null) throw new BadHttpRequestException(MessageConstant.User.MemberNotFound);
                // labs = await _unitOfWork.GetRepository<Lab>().GetPagingListAsync(
                //     selector: l => new LabResponse()
                //     {
                //         Id = l.Id,
                //         Name = l.Name,
                //         Url = l.Url,
                //         CreatedAt = l.CreatedAt,
                //         CreatedBy = l.CreatedBy,
                //         ModifiedAt = l.ModifiedAt,
                //         ModifiedBy = l.ModifiedBy,
                //     },
                //     predicate: l => l.LabMembers!.Any(lm => lm.MemberId.Equals(member.Id)) && 
                //                     (searchName.IsNullOrEmpty() || l.Name.Contains(searchName!)),
                //     page: page,
                //     size: size,
                //     orderBy: l => l.OrderByDescending(l => l.CreatedAt)
                // );
                var labsByMember = await _labRepository.GetLabsPagingByMemberId(member.Id, page, size, searchName);
                labsResponse = _mapper.Map<IPaginate<LabResponse>>(labsByMember);
                labsResponse.Items.Select(lr => lr.ProductNames = labsByMember.Items.SelectMany(l => l.LabProducts)
                    .Where(lp => lp.LabId == lr.Id)
                    .Select(lp => lp.Product.Name).ToList());
                break;
            case RoleEnum.Manager:
            case RoleEnum.Staff:
                // labs = await _unitOfWork.GetRepository<Lab>().GetPagingListAsync(
                //     selector: l => new LabResponse()
                //     {
                //         Id = l.Id,
                //         Name = l.Name,
                //         Url = l.Url,
                //         CreatedAt = l.CreatedAt,
                //         ModifiedAt = l.ModifiedAt,
                //         CreatedBy = l.CreatedBy,
                //         ModifiedBy = l.ModifiedBy,
                //     },
                //     predicate: l => (searchName.IsNullOrEmpty() || l.Name.Contains(searchName!)),
                //     page: page,
                //     size: size,
                //     orderBy: l => l.OrderByDescending(l => l.CreatedAt)
                // );
                var labsByManager = await _labRepository.GetLabsPagingAsync(page, size, searchName);
                // _logger.LogInformation(labsByManager.Items.Select(l => l.LabProducts!.Count).ToList().ToString());
                labsResponse = _mapper.Map<IPaginate<LabResponse>>(labsByManager);
                _logger.LogInformation("LabsResponse: {0}", labsResponse.Items.ToList());
                // labsResponse.Items.Select(lr => lr.ProductNames = labsByManager.Items.SelectMany(l => l.LabProducts)
                //     .Where(lp => lp.LabId == lr.Id)
                //     .Select(lp => lp.Product.Name).ToList());
                break;
            default:
                throw new BadHttpRequestException(MessageConstant.User.RoleNotFound);
        }
        return labsResponse;
    }

    public async Task<LabResponse> GetLabByIdAsync(Guid labId)
    {
        if(labId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Lab.LabIdNotNull);
        // var lab =  await _unitOfWork.GetRepository<Lab>().SingleOrDefaultAsync(
        //     selector: l => new LabResponse()
        //     {
        //         Id = l.Id,
        //         Name = l.Name,
        //         Url = l.Url,
        //         CreatedAt = l.CreatedAt,
        //         ModifiedAt = l.ModifiedAt
        //     },
        //     predicate: l => l.Id == labId
        // );
        var lab = await _labRepository.GetLabByIdAsync(labId);
        
        return _mapper.Map<LabResponse>(lab);
    }

    public async Task<ProductWithLabResponse> GetLabsByProductIdAsync(Guid productId)
    {
        if (productId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        // var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
        //     predicate: p => p.Id == productId,
        //     include: p => p.Include(p => p.LabProducts)
        //         .ThenInclude(lp => lp.Lab),
        //     selector: p => new ProductWithLabResponse()
        //     {
        //         Id = p.Id,
        //         Name = p.Name,
        //         Description = p.Description,
        //         Quantity = p.Quantity,
        //         Price = p.Price,
        //         CreatedAt = p.CreatedAt,
        //         ModifiedAt = p.ModifiedAt,
        //         IsHidden = p.IsHidden,
        //         
        //         Labs = p.LabProducts
        //             .Where(lp => lp.ProductId == productId)
        //             .Select(lp => lp.Lab).Select(l => new LabResponse()
        //             {
        //                 Id = l.Id,
        //                 Name = l.Name,
        //                 Url = l.Url,
        //                 CreatedAt = l.CreatedAt,
        //                 ModifiedAt = l.ModifiedAt,
        //                 CreatedBy = l.CreatedBy,
        //                 ModifiedBy = l.ModifiedBy
        //             }).ToList()
        //     }
        // );
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
        if(request.Name is null) request.Name = request.File.FileName;
        var lab = _mapper.Map<Lab>(request);
        lab.Id = Guid.NewGuid();
        lab.Url = googleDriveResponse.Url;
        lab.CreatedAt = TimeUtil.GetCurrentSEATime();
        lab.ModifiedAt = TimeUtil.GetCurrentSEATime();
        lab.CreatedBy = user.Id;
        lab.ModifiedBy = user.Id;
        
        // var lab = new Lab()
        // {
        //     Id = Guid.NewGuid(),
        //     Name = request.Name ??= googleDriveResponse.FileName,
        //     Url = googleDriveResponse.Url,
        //     CreatedAt = TimeUtil.GetCurrentSEATime(),
        //     ModifiedAt = TimeUtil.GetCurrentSEATime(),
        //     CreatedBy = user.Id,
        //     UploadedBy = user.Id
        // };
        // await _unitOfWork.GetRepository<Lab>().InsertAsync(lab);
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
            var fileUrl = await _firebaseService.UploadFileToFirebaseAsync(request.File);
            if (fileUrl == null) throw new BadHttpRequestException(MessageConstant.Lab.UploadFileFail);
            lab.Url = fileUrl;
        }

        lab.Name = !string.IsNullOrEmpty(request.Name) ? request.Name : lab.Name;

        _labRepository.UpdateAsync(lab);
        var isSuccess = await _labRepository.SaveChangesAsync();
        LabResponse response = null;
        if (isSuccess) response = _mapper.Map<LabResponse>(lab);
        return response;
    }
}