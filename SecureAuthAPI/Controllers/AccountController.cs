using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SecureAuthAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SecureAuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<ActionResult> RegisterUser([FromBody] UserRegisterDto userRegister)
        {
            var user = new IdentityUser { UserName = userRegister.Email, Email = userRegister.Email };
            var result = await _userManager.CreateAsync(user, userRegister.Password);
            if (result.Succeeded)
            {

            }
            return Ok();
        }
        [HttpPost("Login")]
        public async Task<ActionResult> LoginUser([FromBody] UserLoginDto userLogin)
        {

            var result = await _signInManager.PasswordSignInAsync(userLogin.Email, userLogin.Password, userLogin.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(userLogin.Email);
                if (user == null)
                {
                    return NotFound();
                }
                else
                {
                    var claims = new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub,user.Id),
                        new Claim(JwtRegisteredClaimNames.Sub,user.Email)
                    };

                    var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration["AuthKey"]));
                    var signingcredentails = new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256);

                    var jwt = new JwtSecurityToken(signingCredentials: signingcredentails, claims: claims);

                    return Ok(new UserAuth()
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(jwt)
                    });
                }
            }
            else
            {
                return BadRequest();
            }

        }
    }
}
