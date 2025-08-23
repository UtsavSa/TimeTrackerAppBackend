using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TimeTrackerApi.Models;

namespace TimeTrackerApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration, IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _jwtSettings = jwtSettings.Value;
        }

        //[HttpPost("register")]
        //public async Task<IActionResult> Register(RegisterDto dto)
        //{
        //    var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email };
        //    var result = await _userManager.CreateAsync(user, dto.Password);

        //    if (!result.Succeeded)
        //        return BadRequest(result.Errors); 

        //    return Ok(new { message = "User registered successfully." }); 
        //}

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        {
            // If DTO invalid, [ApiController] automatically returns 400 ValidationProblem

            var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return MapIdentityErrors(result.Errors);

            return Ok(new { message = "User registered successfully." });
        }


        //[HttpPost("login")]
        //public async Task<IActionResult> Login(LoginDto dto)
        //{
        //    var user = await _userManager.FindByEmailAsync(dto.Email);

        //    if (user == null)
        //        return Unauthorized(new { message = "User not found." });

        //    var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        //    if (!isPasswordValid)
        //        return Unauthorized(new { message = "Invalid password." });

        //    var token = GenerateJwtToken(user);

        //    return Ok(new
        //    {
        //        token,
        //        email = user.Email,
        //        userId = user.Id,
        //        message = "Login successful"
        //    });
        //}

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            // Model binding validation handled by [ApiController]

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new ProblemDetails
                {
                    Title = "Invalid credentials",
                    Detail = "Email or password is incorrect.",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://httpstatuses.com/401"
                });

            var ok = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!ok)
                return Unauthorized(new ProblemDetails
                {
                    Title = "Invalid credentials",
                    Detail = "Email or password is incorrect.",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://httpstatuses.com/401"
                });

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                email = user.Email,
                userId = user.Id,
                message = "Login successful"
            });
        }

        private IActionResult MapIdentityErrors(IEnumerable<IdentityError> errors)
        {
            // Detect duplicate email/user errors first — return 409
            var duplicate = errors.FirstOrDefault(e =>
                e.Code?.Contains("DuplicateEmail", StringComparison.OrdinalIgnoreCase) == true ||
                e.Code?.Contains("DuplicateUserName", StringComparison.OrdinalIgnoreCase) == true);

            if (duplicate != null)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Email already registered",
                    Detail = "Try logging in or use a different email.",
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://httpstatuses.com/409"
                });
            }

            // Otherwise treat as validation errors (usually password policy)
            var messages = errors.Select(e => e.Description).ToArray();
            var vpd = new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                // Frontend can map this to the "password" form control
                ["Password"] = messages.Length > 0 ? messages : new[] { "Password does not meet requirements." }
            })
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Type = "https://httpstatuses.com/400"
            };
            return ValidationProblem(vpd);
        }


        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new Claim(ClaimTypes.Name, user.UserName ?? ""),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}