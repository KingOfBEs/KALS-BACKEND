using FluentValidation;
using KALS.API.Constant;
using KALS.API.Models.User;

namespace KALS.API.Validator;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        // RuleFor(x => x.UsernameOrPhoneNumber)
        //     .NotEmpty().WithMessage(MessageConstant.User.UsernameOrPhonenumberNotNull);
    }
}