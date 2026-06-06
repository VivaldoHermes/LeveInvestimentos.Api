using System;
using System.ComponentModel.DataAnnotations;
using LeveInvestimentos.Core.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace LeveInvestimentos.Web.ViewModels.Users;

public sealed class CreateUserViewModel
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(200, ErrorMessage = "O nome completo deve ter no máximo {1} caracteres.")]
    [Display(Name = "Nome completo")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de nascimento")]
    public DateOnly? BirthDate { get; set; }

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o perfil.")]
    [Display(Name = "Perfil")]
    public UserRole Role { get; set; } = UserRole.Subordinate;

    [Required(ErrorMessage = "Informe o logradouro.")]
    [StringLength(200, ErrorMessage = "O logradouro deve ter no máximo {1} caracteres.")]
    [Display(Name = "Logradouro")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o número.")]
    [StringLength(20, ErrorMessage = "O número deve ter no máximo {1} caracteres.")]
    [Display(Name = "Número")]
    public string Number { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a cidade.")]
    [StringLength(100, ErrorMessage = "A cidade deve ter no máximo {1} caracteres.")]
    [Display(Name = "Cidade")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o estado.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Use a sigla do estado com 2 letras.")]
    [Display(Name = "Estado")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o telefone fixo.")]
    [Display(Name = "Telefone fixo")]
    public string LandlinePhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o telefone celular.")]
    [Display(Name = "Telefone celular")]
    public string MobilePhone { get; set; } = string.Empty;

    [Display(Name = "Foto")]
    public IFormFile? ProfilePhoto { get; set; }

}
