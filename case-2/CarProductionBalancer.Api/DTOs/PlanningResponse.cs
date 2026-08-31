namespace CarProductionBalancer.Api.DTOs;

public class SlotResponse
{
    public int SlotOrder { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public int OriginalQuantity { get; set; }
    public int BalancedQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class PlanningResponse
{
    public Guid PlanningId { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public string CandidateToken { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int OriginalTotal { get; set; }
    public int BalancedTotal { get; set; }
    public List<SlotResponse> Slots { get; set; } = new();
}

public class PlanningListItem
{
    public Guid PlanningId { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public string CandidateToken { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int OriginalTotal { get; set; }
    public int BalancedTotal { get; set; }
}

public class ValidationErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
