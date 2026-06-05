using System;

namespace LeveInvestimentos.Core.Application.Account.DTOs;

public sealed record AccountSignInResult(
    Guid UserId,
    bool RequiresPasswordChange);
