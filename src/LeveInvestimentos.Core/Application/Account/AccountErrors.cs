using LeveInvestimentos.Core.Application.Common;

namespace LeveInvestimentos.Core.Application.Account;

public static class AccountErrors
{
    public static readonly Error InvalidCredentials = new(
        "Account.InvalidCredentials",
        "E-mail ou senha inválidos.");

    public static readonly Error UserNotFound = new(
        "Account.UserNotFound",
        "Usuário não encontrado.");

    public static readonly Error UserIdRequired = new(
        "Account.UserIdRequired",
        "Usuário autenticado não foi identificado.");

    public static readonly Error PasswordDataRequired = new(
        "Account.PasswordDataRequired",
        "Informe os dados para trocar a senha.");

    public static Error PasswordChangeFailed(string message)
    {
        return new Error("Account.PasswordChangeFailed", message);
    }
}
