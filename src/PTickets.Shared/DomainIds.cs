namespace PTickets.Shared;

public readonly record struct ZoneId(Guid Value)
{
    public static ZoneId New() => new(Guid.NewGuid());
    public static ZoneId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct StreetId(Guid Value)
{
    public static StreetId New() => new(Guid.NewGuid());
    public static StreetId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct InspectorId(Guid Value)
{
    public static InspectorId New() => new(Guid.NewGuid());
    public static InspectorId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct InspectionId(Guid Value)
{
    public static InspectionId New() => new(Guid.NewGuid());
    public static InspectionId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct SessionId(Guid Value)
{
    public static SessionId New() => new(Guid.NewGuid());
    public static SessionId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct ViolationTypeId(Guid Value)
{
    public static ViolationTypeId New() => new(Guid.NewGuid());
    public static ViolationTypeId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct PenaltyTierId(Guid Value)
{
    public static PenaltyTierId New() => new(Guid.NewGuid());
    public static PenaltyTierId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct NoticeId(Guid Value)
{
    public static NoticeId New() => new(Guid.NewGuid());
    public static NoticeId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct FileId(Guid Value)
{
    public static FileId New() => new(Guid.NewGuid());
    public static FileId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}
