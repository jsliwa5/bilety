namespace PTickets.Modules.Notifications.Domain;

using PTickets.Shared.ValueObjects;

public class PhoneRegistration
{
    public Guid Id { get; private set; }
    public RegistrationNumber RegistrationNumber { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = string.Empty;
    public DateTime RegisteredAt { get; private set; }

    private PhoneRegistration() { }

    public static PhoneRegistration Create(RegistrationNumber registrationNumber, string phoneNumber, DateTime registeredAt)
    {
        return new PhoneRegistration
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = registrationNumber,
            PhoneNumber = phoneNumber,
            RegisteredAt = registeredAt
        };
    }
}

