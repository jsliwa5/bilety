namespace PTickets.Modules.Violations.Domain;

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
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ViolationType
        {
            Id = ViolationTypeId.New(),
            Name = name,
            Description = description
        };
    }

    public PenaltyAmount AddPenaltyAmount(decimal amount, DateTime effectiveFrom)
    {
        var penalty = PenaltyAmount.Create(Id, amount, effectiveFrom);
        _penaltyAmounts.Add(penalty);
        return penalty;
    }
}
