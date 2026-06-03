using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LeveInvestimentos.Core.Abstractions;

public interface IFileStorage
{
    Task<StoredFile> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
}
