using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LeveInvestimentos.Infrastructure.Files;

public interface IImageFileValidator
{
    Task<ValidatedImageFile> ValidateAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
