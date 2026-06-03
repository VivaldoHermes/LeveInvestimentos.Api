using System.Threading;
using System.Threading.Tasks;

namespace LeveInvestimentos.Core.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
