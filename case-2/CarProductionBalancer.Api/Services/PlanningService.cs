using CarProductionBalancer.Api.Data;
using CarProductionBalancer.Api.DTOs;
using CarProductionBalancer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarProductionBalancer.Api.Services;

public class PlanningService
{
    private readonly AppDbContext _db;

    public PlanningService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(PlanningResponse response, bool isNew)> CreateAsync(CreatePlanningRequest request)
    {
        // Validasi ga boleh ada quantity minus
        var errors = new List<string>();
        for (int i = 0; i < request.Slots.Count; i++)
        {
            if (request.Slots[i].OriginalQuantity < 0)
                errors.Add($"Slot[{i}] '{request.Slots[i].SlotName}': nilai tidak boleh negatif.");
        }
        if (errors.Any())
            throw new ValidationException(errors);

        // Jalankan logic balancing dari case-1
        var originalQty = request.Slots.Select(s => s.OriginalQuantity).ToArray();
        var balanced = Core.ProductionBalancer.Balance(originalQty);

        var planning = new Planning
        {
            RequestCode = request.RequestCode,
            CandidateToken = request.CandidateToken,
            CreatedAt = DateTime.UtcNow,
            Status = "Success",
            OriginalTotal = originalQty.Sum(),
            BalancedTotal = balanced.Sum(),
        };

        for (int i = 0; i < request.Slots.Count; i++)
        {
            planning.Slots.Add(new PlanningSlot
            {
                SlotOrder = i + 1,
                SlotName = request.Slots[i].SlotName,
                OriginalQuantity = request.Slots[i].OriginalQuantity,
                BalancedQuantity = balanced[i],
                IsActive = request.Slots[i].OriginalQuantity > 0,
            });
        }

        try
        {
            _db.Plannings.Add(planning);
            await _db.SaveChangesAsync();
            return (MapToResponse(planning), true);
        }
        catch (DbUpdateException)
        {
            // Kalau RequestCode udah ada, database bakal tolak karena UNIQUE constraint.
            // Ambil data yang lama buat dibalikin (idempotent).
            _db.ChangeTracker.Clear();
            var existing = await _db.Plannings
                .Include(p => p.Slots.OrderBy(s => s.SlotOrder))
                .FirstAsync(p => p.RequestCode == request.RequestCode);
            return (MapToResponse(existing), false);
        }
    }

    public async Task<List<PlanningListItem>> GetHistoryAsync()
    {
        return await _db.Plannings
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .Select(p => new PlanningListItem
            {
                PlanningId = p.PlanningId,
                RequestCode = p.RequestCode,
                CandidateToken = p.CandidateToken,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                OriginalTotal = p.OriginalTotal,
                BalancedTotal = p.BalancedTotal,
            })
            .ToListAsync();
    }

    public async Task<PlanningResponse?> GetByIdAsync(Guid id)
    {
        var planning = await _db.Plannings
            .Include(p => p.Slots.OrderBy(s => s.SlotOrder))
            .FirstOrDefaultAsync(p => p.PlanningId == id);

        return planning == null ? null : MapToResponse(planning);
    }

    private static PlanningResponse MapToResponse(Planning p) => new()
    {
        PlanningId = p.PlanningId,
        RequestCode = p.RequestCode,
        CandidateToken = p.CandidateToken,
        Status = p.Status,
        CreatedAt = p.CreatedAt,
        OriginalTotal = p.OriginalTotal,
        BalancedTotal = p.BalancedTotal,
        Slots = p.Slots
            .OrderBy(s => s.SlotOrder)
            .Select(s => new SlotResponse
            {
                SlotOrder = s.SlotOrder,
                SlotName = s.SlotName,
                OriginalQuantity = s.OriginalQuantity,
                BalancedQuantity = s.BalancedQuantity,
                IsActive = s.IsActive,
            }).ToList()
    };
}

public class ValidationException : Exception
{
    public List<string> Errors { get; }
    public ValidationException(List<string> errors) : base("Validation failed.") => Errors = errors;
}
