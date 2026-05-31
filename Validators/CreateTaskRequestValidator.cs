using FluentValidation;
using todostodo.Controllers;
using todostodo.Data;

namespace todostodo.Validators;

/// <summary>
/// Validator for creating new tasks with business rule validation.
/// </summary>
public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the CreateTaskRequestValidator.
    /// </summary>
    /// <param name="context">The database context for async validation.</param>
    public CreateTaskRequestValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Task description is required")
            .MinimumLength(1)
            .MaximumLength(500)
            .WithMessage("Task description must be between 1 and 500 characters");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date cannot be in the past");

        RuleFor(x => x.DueTime)
            .Must(BeValidTime)
            .When(x => x.DueTime.HasValue)
            .WithMessage("Due time must be in a valid format");
    }

    private bool BeValidTime(TimeOnly? time)
    {
        return time.HasValue && time.Value >= TimeOnly.MinValue && time.Value <= TimeOnly.MaxValue;
    }
}
