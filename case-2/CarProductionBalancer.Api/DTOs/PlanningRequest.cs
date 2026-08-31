using System.ComponentModel.DataAnnotations;

namespace CarProductionBalancer.Api.DTOs;

public class SlotRequest
{
    [Required(ErrorMessage = "SlotName is required.")]
    [MaxLength(50)]
    public string SlotName { get; set; } = string.Empty;

    [Required(ErrorMessage = "OriginalQuantity is required.")]
    public int OriginalQuantity { get; set; }
}

public class CreatePlanningRequest
{
    [Required(ErrorMessage = "RequestCode is required.")]
    [MaxLength(100)]
    public string RequestCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "CandidateToken is required.")]
    [MaxLength(200)]
    public string CandidateToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slots are required.")]
    [MinLength(1, ErrorMessage = "At least 1 slot is required.")]
    [MaxLength(7, ErrorMessage = "Maximum 7 slots allowed.")]
    public List<SlotRequest> Slots { get; set; } = new();
}
