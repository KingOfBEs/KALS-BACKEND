using KALS.Domain.Entities;

namespace KALS.Repository.Interface;

public interface ILabProductRepository: IGenericRepository<LabProduct>
{
    Task<(List<Guid> newLabIds, List<Guid> removeLabIds)> GetNewAndRemoveLabIdsAsync(Guid productId, List<Guid> requestedLabIds);
    Task<ICollection<LabProduct>> GetLabProductsByLabIds(List<Guid> labIds);
}