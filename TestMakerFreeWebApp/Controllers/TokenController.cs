using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
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
        private readonly SignInManager<ApplicationUser> signInManager;

        #region Constructor

        public TokenController(ApplicationDbContext db,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration) : base(db, roleManager, userManager, configuration)
        {
            this.signInManager = signInManager;
        }

        #endregion Constructor

        [HttpGet("externalLogin/{provider}")]
        public IActionResult ExternalLogin(string provider, string returnUrl =
            null)
        {
            switch (provider.ToLower())
            {
                case "facebook":
                    // case "google":
                    // case "twitter":
                    // todo: add all supported providers here
                    // Redirect the request to the external provider.
                    var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Token", new { returnUrl });
                    var properties =
                        this.signInManager.ConfigureExternalAuthenticationProperties(
                            provider,
                            redirectUrl);
                    return Challenge(properties, provider);

                default:
                    // provider not supported
                    return BadRequest(new
                    {
                        Error = String.Format("Provider '{0}' is not supported.", provider)
                    });
            }
        }

        [HttpGet("externalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(
            string returnUrl = null, string remoteError = null)
        {
            if (!String.IsNullOrEmpty(remoteError))
            {
                // TODO: handle external provider errors
                throw new Exception(String.Format("External Provider error: {0}", remoteError));
            }
            // Extract the login info obtained from the External Provider
            var info = await this.signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                // if there's none, emit an error
                throw new Exception("ERROR: No login info available.");
            }
            // Check if this user already registered himself with this external provider before
            var user = await UserManager.FindByLoginAsync(info.LoginProvider,
                info.ProviderKey);
            if (user == null)
            {
                // If we reach this point, it means that this user never tried to logged in
                // using this external provider. However, it could have used other providers
                // and /or have a local account.
                // We can find out if that's the case by looking for his e-mail address.
                // Retrieve the 'emailaddress' claim
                var emailKey =
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
                var email = info.Principal.FindFirst(emailKey).Value;
                // Lookup if there's an username with this e-mail address in the Db
                user = await UserManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // No user has been found: register a new user
                    // using the info retrieved from the provider
                    DateTime now = DateTime.UtcNow;
                    // Create a unique username using the 'nameidentifier' claim
                    var idKey = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
                    var username = String.Format("{0}{1}{2}",
                        info.LoginProvider,
                        info.Principal.FindFirst(idKey).Value,
                        Guid.NewGuid().ToString("N"));
                    user = new ApplicationUser()
                    {
                        SecurityStamp = Guid.NewGuid().ToString(),
                        UserName = username,
                        Email = email,
                        CreatedDate = now,
                        LastModifiedDate = now
                    };
                    // Add the user to the Db with a random password
                    await UserManager.CreateAsync(
                        user,
                        DataHelper.GenerateRandomPassword());
                    // Assign the user to the 'RegisteredUser' role.
                    await UserManager.AddToRoleAsync(user, "RegisteredUser");
                    // Remove Lockout and E-Mail confirmation
                    user.EmailConfirmed = true;
                    user.LockoutEnabled = false;
                    // Persist everything into the Db
                    await this.Db.SaveChangesAsync();
                }
                // Register this external provider to the user
                var ir = await UserManager.AddLoginAsync(user, info);
                if (ir.Succeeded)
                {
                    // Persist everything into the Db
                    this.Db.SaveChanges();
                }
                else throw new Exception("Authentication error");
            }
            // create the refresh token
            var rt = CreateRefreshToken("TestMakerFree", user.Id);
            // add the new refresh token to the DB
            this.Db.Tokens.Add(rt);
            this.Db.SaveChanges();
            // create & return the access token
            var t = CreateAccessToken(user.Id, rt.Value);
            // output a <SCRIPT> tag to call a JS function
            // registered into the parent window global scope
            return Content(
                "<script type=\"text/javascript\">" +
                "window.opener.externalProviderLogin(" +
                JsonConvert.SerializeObject(t, JsonSettings) +
                ");" +
                "window.close();" +
                "</script>",
                "text/html"
            );
        }

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

        [HttpPost("facebook")]
        public async Task<IActionResult> Facebook([FromBody] ExternalLoginRequestViewModel model)
        {
            try
            {
                const string fbApiUrl = "https://graph.facebook.com/v2.10/";
                var fbApiQueryString = $"me?scope=email&access_token={model.access_token}&fields=id,name,email";
                string result = null;

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(fbApiUrl);
                    var response = await client.GetAsync(fbApiQueryString);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        throw new AuthenticationException();
                    }
                }

                var epInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                var info = new UserLoginInfo("facebook", epInfo["id"], "Facebook");

                var user = await this.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                if (user == null)
                {
                    user = await this.UserManager.FindByEmailAsync(epInfo["email"]);

                    if (user == null)
                    {
                        DateTime now = DateTime.UtcNow;
                        var username = $"FB{epInfo["id"]}{Guid.NewGuid()}";
                        user = new ApplicationUser
                        {
                            UserName = username,
                            CreatedDate = now,
                            SecurityStamp = Guid.NewGuid().ToString(),
                            Email = epInfo["email"],
                            DisplayName = epInfo["name"],
                            LastModifiedDate = now
                        };

                        await this.UserManager.CreateAsync(user, DataHelper.GenerateRandomPassword());
                        await this.UserManager.AddToRoleAsync(user, "RegisteredUser");

                        user.EmailConfirmed = true;
                        user.LockoutEnabled = false;

                        this.Db.SaveChanges();
                    }

                    var ir = await this.UserManager.AddLoginAsync(user, info);
                    if (ir.Succeeded)
                    {
                        this.Db.SaveChanges();
                    }
                    else
                    {
                        throw new AuthenticationException();
                    }
                }

                var rt = this.CreateRefreshToken(model.client_id, user.Id);

                this.Db.Tokens.Add(rt);
                this.Db.SaveChanges();

                var token = this.CreateAccessToken(user.Id, rt.Value);
                return Json(token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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