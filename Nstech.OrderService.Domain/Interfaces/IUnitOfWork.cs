using System.Threading;
using System.Threading.Tasks;

namespace Nstech.OrderService.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<bool> CommitAsync(CancellationToken cancellationToken = default);
}
