using FluentValidation;
using RealWorldApp.Models;

public class ArticleValidator : AbstractValidator<Article>
{
    public ArticleValidator()
    {
        RuleFor(a => a.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters long.");

        RuleFor(a => a.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(a => a.Body)
            .NotEmpty().WithMessage("Body is required.");


        RuleFor(a => a.Slug)
            .Must(string.IsNullOrWhiteSpace)
            .WithMessage("Slug should not be provided manually.");
    }
}
