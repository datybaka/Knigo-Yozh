using KnigoYozh.Api.DTOs;
using KnigoYozh.Application.Users.Commands.LoginUser;
using KnigoYozh.Application.Users.Commands.RegisterUser;
using KnigoYozh.Application.Users.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnigoYozh.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender; // Интерфейс MediatR для отправки команд

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        // 1. Превращаем DTO в Команду
        var command = new RegisterUserCommand(
            request.Email,
            request.Username,
            request.Password,
            request.Role);

        // 2. Отправляем в MediatR (здесь сработает ValidationBehavior -> Handler)
        Guid userId = await _sender.Send(command, cancellationToken);

        // 3. Возвращаем 200 OK с ID нового пользователя
        return Ok(new { Id = userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
    [FromBody] LoginUserCommand command,
    CancellationToken cancellationToken)
    {
        LoginResponse response = await _sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var query = new GetCurrentUserQuery();
        var profile = await _sender.Send(query, cancellationToken);
        return Ok(profile);
    }
}

