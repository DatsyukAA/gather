using Account.EventBus;
using Account.Models;
using Account.Services;
using Account.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Account.Controllers;

[Authorize]
[ApiController]
[Route("/account")]
public class AuthenticationController : ControllerBase
{
    private readonly IAccountService _accountService;

    private readonly IBus Bus;

    public AuthenticationController(IAccountService accountService, IBus bus)
    {
        _accountService = accountService;
        Bus = bus;
    }
    [AllowAnonymous]
    [HttpGet("~/signup")]
    public IActionResult TestReg()
    {
        Bus.SendExchangeAsync("notifications", new UserCreatedNotification
        {
            Id = 1.ToString(),
            Name = "John"
        });
        return Ok();
    }
    [AllowAnonymous]
    [HttpPost("~/signup")]
    public IActionResult Register([FromBody] RegisterRequest model)
    {
        var response = _accountService.Register(model.Login, model.Password, model.Email, model.Name, this.IpAddress());

        if (response == null)
            return BadRequest(new { message = "User or email already registered." });

        this.setCookie("refreshToken", response.RefreshToken);

        Bus.SendExchangeAsync("notifications", new UserCreatedNotification
        {
            Id = response.Id.ToString(),
            Name = response.Username
        });
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("~/signin")]
    public IActionResult Authenticate([FromBody] AuthenticateRequest model)
    {
        var response = _accountService.Authenticate(model.Login, model.Password, this.IpAddress());

        if (response == null)
            return BadRequest(new { message = "Username or password is incorrect." });

        this.setCookie("refreshToken", response.RefreshToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("~/refresh-token")]
    public IActionResult RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken == null)
            return Unauthorized(new { message = "Invalid token" });
        var response = _accountService.RefreshToken(refreshToken, this.IpAddress());

        if (response == null)
            return Unauthorized(new { message = "Invalid token" });

        this.setCookie("refreshToken", response.RefreshToken);

        return Ok(response);
    }

    [HttpPost("~/revoke")]
    public IActionResult RevokeToken([FromBody] RevokeTokenRequest model)
    {
        // accept token from request body or cookie
        var token = model.Token ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = "Token is required" });

        var response = _accountService.RevokeToken(token, this.IpAddress());

        if (!response)
            return NotFound(new { message = "Token not found" });

        return Ok(new { message = "Token revoked" });
    }

    [HttpGet("~/")]
    public IActionResult GetAccounts()
    {
        var users = _accountService.GetAll();
        return Ok(users);
    }

    [HttpGet("~/{id}")]
    public IActionResult GetById(int id)
    {
        var user = _accountService.GetById(id);
        if (user == null) return NotFound();

        return Ok(user);
    }

    [HttpGet("~/{id}/refresh-tokens")]
    public IActionResult GetRefreshTokens(int id)
    {
        var user = _accountService.GetById(id);
        if (user == null) return NotFound();

        return Ok(user.RefreshTokens);
    }
}
