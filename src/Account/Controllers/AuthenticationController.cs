using Account.Entities;
using Account.EventBus;
using Account.Models;
using Account.Models.Authenticate;
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
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IAccountService accountService, IBus bus, ILogger<AuthenticationController> logger)
    {
        _accountService = accountService;
        Bus = bus;
        _logger = logger;
    }

    [HttpGet("~/")]
    public ActionResult<User> GetAuthorizedAccount()
    {
        var users = _accountService.GetById(int.Parse(Request.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value ?? "0"));
        _logger.LogInformation($"Current user requested.");
        return Ok(users);
    }

    [HttpPut("~/")]
    public ActionResult<User> Update([FromBody] User model)
    {
        var response = _accountService.Update(model, this.IpAddress());

        if (response == null)
        {
            _logger.LogInformation($"User {model.Username} or email {model.Email} not found.");
            return BadRequest(new { message = "User or email already registered." });
        }

        this.setCookie("refreshToken", response.RefreshToken);

        _logger.LogInformation($"User {response.Id}[{response.Username}] registered.");
        Bus.SendExchangeAsync("notifications", new Notification
        {
            Sender = response.Id.ToString(),
            Title = "User was registered",
            Text = $"{response.Username} now join to us!"
        });
        return Ok(response);
    }

    [HttpGet("~/{id}")]
    public ActionResult<User> GetById(int id)
    {
        var user = _accountService.GetById(id);
        if (user == null)
        {
            _logger.LogInformation($"User {id} requested. User not found.");
            return NotFound();
        }
        _logger.LogInformation($"User {id} requested.");
        return Ok(user);
    }

    [HttpGet("~/{id}/refresh-tokens")]
    public ActionResult<IEnumerable<RefreshToken>> GetRefreshTokens(int id)
    {
        var user = _accountService.GetById(id);
        if (user == null)
        {
            _logger.LogInformation($"User's {id} refresh tokens requested. Not found.");
            return NotFound();
        }
        _logger.LogInformation($"User's {id} refresh tokens requested.");
        return Ok(user.RefreshTokens);
    }

    [AllowAnonymous]
    [HttpPost("~/signup")]
    public ActionResult<AuthenticateResponse> Register([FromBody] RegisterRequest model)
    {
        var response = _accountService.Register(model.Login, model.Password, model.Email, model.Name, this.IpAddress());

        if (response == null)
        {
            _logger.LogInformation($"User {model.Login} or email {model.Email} already registered.");
            return BadRequest(new { message = "User or email already registered." });
        }

        this.setCookie("refreshToken", response.RefreshToken);

        _logger.LogInformation($"User {response.Id}[{response.Username}] registered.");
        Bus.SendExchangeAsync("notifications", new Notification
        {
            Sender = response.Id.ToString(),
            Title = "User was registered",
            Text = $"{response.Username} now join to us!"
        });
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("~/signin")]
    public ActionResult<AuthenticateResponse> Authenticate([FromBody] AuthenticateRequest model)
    {
        var response = _accountService.Authenticate(model.Login, model.Password, this.IpAddress());

        if (response == null)
        {
            _logger.LogInformation($"User {model.Login} failed to authenticate.");
            return BadRequest(new { message = "Username or password is incorrect." });
        }

        this.setCookie("refreshToken", response.RefreshToken);
        _logger.LogInformation($"User {response.Id}[{response.Username}] authenticated successfully.");
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("~/refresh-token")]
    public ActionResult<AuthenticateResponse> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken == null)
        {
            _logger.LogInformation($"Refresh token revoked because token is null.");
            return Unauthorized(new { message = "Invalid token" });
        }
        var response = _accountService.RefreshToken(refreshToken, this.IpAddress());

        if (response == null)
        {
            _logger.LogInformation($"Refresh token revoked because token is invalid. Invalid token: {refreshToken}.");
            return Unauthorized(new { message = "Invalid token" });
        }

        this.setCookie("refreshToken", response.RefreshToken);
        _logger.LogInformation($"Refresh token for user {response.Id}[{response.Username}] refreshed successfully.");
        return Ok(response);
    }

    [HttpPost("~/revoke")]
    public IActionResult RevokeToken([FromQuery] string? token)
    {
        token ??= Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogInformation($"Refresh token revoked because token is null.");
            return BadRequest(new { message = "Token is required" });
        }

        var response = _accountService.RevokeToken(token, this.IpAddress());

        if (!response)
        {
            _logger.LogInformation($"Refresh token revoked because token not found. Invalid token: {token}.");
            return NotFound(new { message = "Token not found" });
        }
        _logger.LogInformation($"Refresh token revoked successfully. Token: {token}.");
        return Ok(new { message = "Token revoked" });
    }

    [HttpGet("~/list")]
    public ActionResult<IEnumerable<User>> GetAccounts()
    {
        var users = _accountService.GetAll();
        _logger.LogInformation($"List of accounts requested.");
        return Ok(users);
    }
}
