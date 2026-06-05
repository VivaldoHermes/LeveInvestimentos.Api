using System;

namespace LeveInvestimentos.Core.Application.Account.Commands;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword);
