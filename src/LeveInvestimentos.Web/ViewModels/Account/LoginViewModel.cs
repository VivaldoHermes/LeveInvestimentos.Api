using System.ComponentModel.DataAnnotations;

namespace LeveInvestimentos.Web.ViewModels.Account;

public sealed class LoginViewModel
{
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Lembrar login")]
    public bool RememberMe { get; set; }
}
