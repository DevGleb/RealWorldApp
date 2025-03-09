using FluentValidation;
using RealWorldApp.Models;

public class CommentValidator : AbstractValidator<Comment>
{
    public CommentValidator()
    {
        RuleFor(c => c.Body)
            .NotEmpty().WithMessage("Comment body cannot be empty.")
            .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters.");
    }
}
