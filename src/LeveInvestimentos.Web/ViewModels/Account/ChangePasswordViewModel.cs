using System.ComponentModel.DataAnnotations;

namespace LeveInvestimentos.Web.ViewModels.Account;

public sealed class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Informe a senha atual.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha atual")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nova senha.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova senha")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova senha.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "A confirmação deve ser igual à nova senha.")]
    [Display(Name = "Confirmar nova senha")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
