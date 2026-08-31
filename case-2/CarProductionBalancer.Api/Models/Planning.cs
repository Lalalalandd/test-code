using System.ComponentModel.DataAnnotations;

namespace CarProductionBalancer.Api.Models;

public class Planning
{
    [Key]
    public Guid PlanningId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string RequestCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string CandidateToken { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Success";

    public int OriginalTotal { get; set; }
    public int BalancedTotal { get; set; }

    public ICollection<PlanningSlot> Slots { get; set; } = new List<PlanningSlot>();
}
