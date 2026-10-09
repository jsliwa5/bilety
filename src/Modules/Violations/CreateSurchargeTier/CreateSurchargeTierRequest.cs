namespace PTickets.Modules.Violations.CreateSurchargeTier;

public record CreateSurchargeTierRequest(int MinMinutes, int? MaxMinutes, decimal Amount);

