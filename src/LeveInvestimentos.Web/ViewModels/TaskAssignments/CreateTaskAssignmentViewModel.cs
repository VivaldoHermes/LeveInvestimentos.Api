using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LeveInvestimentos.Web.ViewModels.TaskAssignments;

public sealed class CreateTaskAssignmentViewModel
{
    [Required(ErrorMessage = "Informe o subordinado.")]
    [Display(Name = "Subordinado")]
    public Guid? SubordinateId { get; set; }

    [Required(ErrorMessage = "Informe a descricao.")]
    [StringLength(2000, ErrorMessage = "A descricao deve ter no maximo {1} caracteres.")]
    [Display(Name = "Descricao")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data limite.")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Data limite")]
    public DateTime? DueDate { get; set; }

    public IReadOnlyCollection<SelectListItem> Subordinates { get; set; } = Array.Empty<SelectListItem>();
}
