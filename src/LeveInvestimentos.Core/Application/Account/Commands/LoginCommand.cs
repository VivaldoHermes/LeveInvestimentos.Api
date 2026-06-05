namespace LeveInvestimentos.Core.Application.Account.Commands;

public sealed record LoginCommand(
    string Email,
    string Password,
    bool RememberMe);
