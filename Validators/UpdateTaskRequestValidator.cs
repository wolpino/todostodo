using FluentValidation;
using todostodo.Controllers;
using todostodo.Data;

namespace todostodo.Validators;

/// <summary>
/// Validator for updating existing tasks with complex business rules.
/// </summary>
public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the UpdateTaskRequestValidator.
    /// </summary>
    /// <param name="context">The database context for async validation.</param>
    public UpdateTaskRequestValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Description)
            .MinimumLength(1)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Task description must be between 1 and 500 characters when provided");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date cannot be in the past");

        RuleFor(x => x.DueTime)
            .Must(BeValidTime)
            .When(x => x.DueTime.HasValue)
            .WithMessage("Due time must be in a valid format");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid task status");

        // Complex rule: If setting due date to today, ensure due time is in the future (if provided)
        RuleFor(x => x.DueTime)
            .Must((request, dueTime) =>
            {
                if (request.DueDate == DateOnly.FromDateTime(DateTime.Today) && dueTime.HasValue)
                {
                    return dueTime.Value > TimeOnly.FromDateTime(DateTime.Now);
                }
                return true;
            })
            .When(x => x.DueDate.HasValue && x.DueTime.HasValue)
            .WithMessage("For today's due date, time must be in the future");
    }

    private bool BeValidTime(TimeOnly? time)
    {
        return time.HasValue && time.Value >= TimeOnly.MinValue && time.Value <= TimeOnly.MaxValue;
    }
}
