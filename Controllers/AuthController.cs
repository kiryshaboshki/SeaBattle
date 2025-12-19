using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SeaBattleApi.Auth;
using SeaBattleRepository.Implement;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SeaBattleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RepositoryUser repositoryUser;

        public AuthController(RepositoryUser repositoryUser)
        {
            this.repositoryUser = repositoryUser;
        }

        [HttpPost("GetToken")]
        public async Task<ActionResult<string>> Login(string login, string password)
        {
            var user = await repositoryUser.SearchEntryByConditionAsync(s => s.Login == login
                && s.Password == password);
            if (user.Id == 0)
                return NotFound();

            var claimAuth = new List<Claim> {
                new Claim(ClaimTypes.Name, login),
                new Claim("ID", user.Id.ToString(), ClaimValueTypes.Integer32)
            };
            var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            claims: claimAuth,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(120)),
            signingCredentials: new SigningCredentials(
                AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var result = new JwtSecurityTokenHandler().WriteToken(jwt);
            return result;
        }

        [HttpPost("Registration")]
        public async Task<ActionResult> Registration(AuthData data)
        {
            var user = await repositoryUser.
                SearchEntryByConditionAsync(s => s.Login == data.Login);
            if (user.Id != 0)
                return BadRequest("Пользователь с таким логином уже существует");

            await repositoryUser.CreateAsync(new SeaBattleRepository.DTO.UserDTO
            {
                Login = data.Login,
                Password = data.Password,
            });

            await repositoryUser.SaveAsync();

            return Ok();
        }

    }
}
