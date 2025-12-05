using FluentValidation;
using UserDocumentProcessor.API.Models.Requests;

namespace UserDocumentProcessor.API.Validators
{
    public class UserRegisterValidator : AbstractValidator<RegisterUserRequest>
    {
        public UserRegisterValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("نام الزامی است.")
                .MinimumLength(3).WithMessage("حداقل 3 کاراکتر لازم است.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ایمیل الزامی است.")
                .EmailAddress().WithMessage("فرمت ایمیل صحیح نیست.");

            RuleFor(x => x.Document)
                .NotNull().WithMessage("فایل سند الزامی است.")
                .Must(file => file.Length < 10 * 1024 * 1024)
                .WithMessage("حجم فایل نباید بیشتر از 10MB باشد.");
        }
    }
}
