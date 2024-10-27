using KALS.API.Models.Lab;
using KALS.API.Models.Product;
using KALS.Domain.Paginate;

namespace KALS.API.Services.Interface;

public interface ILabService
{
    Task<GetProductResponse> AssignLabToProductAsync(Guid productId, AssignLabsToProductRequest request);
    Task<IPaginate<LabResponse>> GetLabsAsync(int page, int size, string? searchName);
    Task<LabResponse> GetLabByIdAsync(Guid labId);
    Task<ProductWithLabResponse> GetLabsByProductIdAsync(Guid productId);
    
    Task<LabResponse> CreateLabAsync(CreateLabRequest request);
    
    Task<LabResponse> UpdateLabAsync(Guid labId, UpdateLabRequest request);
}