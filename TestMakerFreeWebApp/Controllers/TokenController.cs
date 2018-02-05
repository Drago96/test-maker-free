using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TestMakerFreeWebApp.Data;
using TestMakerFreeWebApp.Data.Models;
using TestMakerFreeWebApp.ViewModels;

namespace TestMakerFreeWebApp.Controllers
{
    public class TokenController : BaseApiController
    {
        #region Constructor

        public TokenController(ApplicationDbContext db,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration) : base(db, roleManager, userManager, configuration)
        {
        }

        #endregion Constructor

        [HttpPost("auth")]
        public async Task<IActionResult> Jwt([FromBody] TokenRequestViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            switch (model.grant_type)
            {
                case "password":
                    return await this.GetToken(model);

                case "refresh_token":
                    return await this.RefreshToken(model);

                default:
                    return Unauthorized();
            }
        }

        private async Task<IActionResult> RefreshToken(TokenRequestViewModel model)
        {
            try
            {
                // check if the received refreshToken exists for the given clientId
                var rt = this.Db.Tokens
                    .FirstOrDefault(t => t.ClientId == model.client_id &&
                                         t.Value == model.refresh_token);

                if (rt == null)
                {
                    return Unauthorized();
                }

                var rtNew = this.CreateRefreshToken(rt.ClientId, rt.UserId);

                this.Db.Tokens.Remove(rt);

                this.Db.Tokens.Add(rtNew);

                this.Db.SaveChanges();

                var response = this.CreateAccessToken(rtNew.UserId, rtNew.Value);

                return Json(response);
            }
            catch
            {
                return Unauthorized();
            }
        }

        private Token CreateRefreshToken(string clientId, string userId)
        {
            return new Token
            {
                ClientId = clientId,
                UserId = userId,
                CreatedDate = DateTime.UtcNow,
                Type = 0,
                Value = Guid.NewGuid().ToString("N")
            };
        }

        private TokenResponseViewModel CreateAccessToken(string userId, string refreshToken)
        {
            DateTime now = DateTime.UtcNow;

            // add the registered claims for JWT (RFC7519).
            // For more info, see https://tools.ietf.org/html/rfc7519#section-4.1
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new
                        DateTimeOffset(now).ToUnixTimeSeconds().ToString())
                // TODO: add additional claims here
            };
            var tokenExpirationMins =
                Configuration.GetValue<int>
                    ("Auth:Jwt:TokenExpirationInMinutes");
            var issuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Configuration["Auth:Jwt:Key"]));
            var token = new JwtSecurityToken(
                issuer: Configuration["Auth:Jwt:Issuer"],
                audience: Configuration["Auth:Jwt:Audience"],
                claims: claims,
                notBefore: now,
                expires:
                now.Add(TimeSpan.FromMinutes(tokenExpirationMins)),
                signingCredentials: new SigningCredentials(
                    issuerSigningKey,
                    SecurityAlgorithms.HmacSha256)
            );
            var encodedToken = new
                JwtSecurityTokenHandler().WriteToken(token);

            // build & return the response
            return new TokenResponseViewModel()
            {
                token = encodedToken,
                expiration = tokenExpirationMins,
                refresh_token = refreshToken
            };
        }

        private async Task<IActionResult> GetToken(TokenRequestViewModel model)
        {
            try
            {
                var user = await this.UserManager.FindByNameAsync(model.username);

                if (user == null && model.username.Contains("@"))
                {
                    user = await this.UserManager.FindByEmailAsync(model.username);
                }

                if (user == null || !await this.UserManager.CheckPasswordAsync(user, model.password))
                {
                    return Unauthorized();
                }

                var refreshToken = this.CreateRefreshToken(model.client_id, user.Id);

                this.Db.Tokens.Add(refreshToken);
                this.Db.SaveChanges();

                var token = this.CreateAccessToken(user.Id, refreshToken.Value);

                return Json(token);
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }
        }
    }
}