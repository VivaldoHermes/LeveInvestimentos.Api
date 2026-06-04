using System.Threading;
using System.Threading.Tasks;

namespace LeveInvestimentos.Core.Abstractions;

public interface IEmailOutbox
{
    Task EnqueueAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}
