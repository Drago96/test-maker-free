using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using TestMakerFreeWebApp.Data;
using TestMakerFreeWebApp.Data.Models;
using TestMakerFreeWebApp.ViewModels;

namespace TestMakerFreeWebApp.Controllers
{
    public class UserController : BaseApiController
    {
        #region Constructor

        public UserController(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration
        ) : base(context, roleManager, userManager, configuration)
        {
        }

        #endregion Constructor

        #region RESTful Conventions

        ///<summary>
        /// POST: api/user
        /// </summary>
        /// <returns>Creates a new User and returns it accordingly.</returns>
        public async Task<IActionResult> Add([FromBody] UserViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var user = await this.UserManager.FindByNameAsync(model.UserName);

            if (user != null)
            {
                return BadRequest("User already exists.");
            }

            var now = DateTime.UtcNow;

            user = new ApplicationUser
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName,
                Email = model.Email,
                DisplayName = model.DisplayName,
                CreatedDate = now,
                LastModifiedDate = now
            };

            await this.UserManager.CreateAsync(user, model.Password);

            await this.UserManager.AddToRoleAsync(user, "RegisteredUser");

            user.EmailConfirmed = true;
            user.LockoutEnabled = false;

            this.Db.SaveChanges();

            return Json(user.Adapt<UserViewModel>(), this.JsonSettings);
        }

        #endregion RESTful Conventions
    }
}