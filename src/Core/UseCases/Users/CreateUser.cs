using Core.Contracts.Authentication;
using Core.Contracts.DTOs.Users.Request;
using Core.Contracts.DTOs.Users.Response;
using Core.Contracts.Results;
using Core.Contracts.UnitOfWork;
using Core.Contracts.UseCases.Users;
using Core.Domain.Common.DomainResults;
using Core.Domain.Users;
using Core.Utilities.Mappers;
using Core.Utilities.Validations;
using FluentValidation;
using FluentValidation.Results;
using System.Net;

namespace Core.UseCases.Users;
public class CreateUser : ICreateUser
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateUserRequestDto> _validator;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUser(
        IUnitOfWork unitOfWork,
        IValidator<CreateUserRequestDto> validator,
        IPasswordHasher passwordHasher
    )
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<CreateUserResponseDto>> ExecuteAsync(
        CreateUserRequestDto requestDto,
        CancellationToken cancellationToken
    )
    {
        ValidationResult validatorResult = _validator.Validate(requestDto);
        if (!validatorResult.IsValid)
        {
            return Result<CreateUserResponseDto>.Failure(HttpStatusCode.BadRequest)
                                                .WithErrors([.. validatorResult.Errors.Select(x => x.ErrorMessage)]);
        }

        bool userExists = await _unitOfWork.UserRepository.ExistsByEmailAsync(requestDto.Email, cancellationToken);
        if (userExists)
        {
            return Result<CreateUserResponseDto>.Failure(HttpStatusCode.Conflict)
                                                .WithErrors([ValidationMessages.User.USER_EMAIL_EXISTS]);
        }

        string passwordHash = _passwordHasher.Hash(requestDto.Password);

        DomainResult<User> userResult = User.Create(
            requestDto.FirstName,
            requestDto.LastName,
            requestDto.Email,
            passwordHash,
            requestDto.Role
        );
        if (!userResult.IsSuccess)
        {
            return Result<CreateUserResponseDto>.Failure(HttpStatusCode.BadRequest)
                                                .WithErrors(userResult.Errors);
        }

        CreateUserResponseDto responseDto = userResult.Value.ToCreateUserResponseDto();
        return Result<CreateUserResponseDto>.Success(HttpStatusCode.Created)
                                            .WithPayload(responseDto);
    }
}