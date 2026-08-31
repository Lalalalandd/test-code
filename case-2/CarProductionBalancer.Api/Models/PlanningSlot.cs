using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarProductionBalancer.Api.Models;

public class PlanningSlot
{
    [Key]
    public Guid PlanningSlotId { get; set; } = Guid.NewGuid();

    public Guid PlanningId { get; set; }

    [ForeignKey(nameof(PlanningId))]
    public Planning? Planning { get; set; }

    public int SlotOrder { get; set; }

    [MaxLength(50)]
    public string SlotName { get; set; } = string.Empty;

    public int OriginalQuantity { get; set; }
    public int BalancedQuantity { get; set; }
    public bool IsActive { get; set; }
}
