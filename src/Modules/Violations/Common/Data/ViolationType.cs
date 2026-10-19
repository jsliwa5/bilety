namespace PTickets.Modules.Violations.Common.Data;

using PTickets.Shared;

public class ViolationType
{
    private readonly List<PenaltyAmount> _penaltyAmounts = [];

    public ViolationTypeId Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public IReadOnlyCollection<PenaltyAmount> PenaltyAmounts => _penaltyAmounts.AsReadOnly();

    private ViolationType() { }

    public static ViolationType Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new PTickets.Modules.Violations.Common.Exceptions.InvalidViolationTypeNameException();
        }

        return new ViolationType
        {
            Id = ViolationTypeId.New(),
            Name = name.Trim(),
            Description = description?.Trim()
        };
    }

    public PenaltyAmount AddPenaltyAmount(decimal amount, DateTime effectiveFrom, DateTime? currentUtcDate = null)
    {
        var penalty = PenaltyAmount.Create(Id, amount, effectiveFrom, currentUtcDate);
        _penaltyAmounts.Add(penalty);
        return penalty;
    }
}

