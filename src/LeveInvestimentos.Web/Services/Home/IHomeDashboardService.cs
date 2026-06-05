using System;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Web.ViewModels.Home;

namespace LeveInvestimentos.Web.Services.Home;

public interface IHomeDashboardService
{
    Task<Result<HomeDashboardViewModel>> BuildForManagerAsync(
        Guid managerId,
        CancellationToken cancellationToken = default);
}
