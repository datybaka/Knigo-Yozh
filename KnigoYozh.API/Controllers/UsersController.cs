using MediatR;
using Microsoft.AspNetCore.Mvc;
using KnigoYozh.Api.DTOs;
using KnigoYozh.Application.Users.Commands.RegisterUser;

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
}