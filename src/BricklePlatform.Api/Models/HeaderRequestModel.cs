using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BricklePlatform.Api.Models;

public record HeaderRequestModel
{
    [FromHeader(Name = "correlationId")]
    [Required(ErrorMessage = "CorrelationId es obligatorio.")]
    public required string CorrelationId { get; set; }

    [FromHeader(Name = "user")]
    [Required(ErrorMessage = "User obligatorio.")]
    public required string User { get; set; }

    [FromHeader(Name = "source")]
    [Required(ErrorMessage = "Source obligatorio.")]
    public required string Source { get; set; }

    [FromHeader(Name = "requestDate")]
    [Required(ErrorMessage = "RequestDate obligatorio.")]
    public required DateTime RequestDate { get; set; }
}