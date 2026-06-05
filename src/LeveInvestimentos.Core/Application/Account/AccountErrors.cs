using LeveInvestimentos.Core.Application.Common;

namespace LeveInvestimentos.Core.Application.Account;

public static class AccountErrors
{
    public static readonly Error InvalidCredentials = new(
        "Account.InvalidCredentials",
        "E-mail ou senha invalidos.");

    public static readonly Error UserNotFound = new(
        "Account.UserNotFound",
        "Usuario nao encontrado.");

    public static readonly Error UserIdRequired = new(
        "Account.UserIdRequired",
        "Usuario autenticado nao foi identificado.");

    public static readonly Error PasswordDataRequired = new(
        "Account.PasswordDataRequired",
        "Informe os dados para trocar a senha.");

    public static Error PasswordChangeFailed(string message)
    {
        return new Error("Account.PasswordChangeFailed", message);
    }
}
