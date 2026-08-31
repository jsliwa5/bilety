namespace PTickets.Modules.Violations.Application.Dtos;

public record CreateSurchargeTierRequest(int MinMinutes, int? MaxMinutes, decimal Amount);
