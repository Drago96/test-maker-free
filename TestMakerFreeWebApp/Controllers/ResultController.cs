using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using TestMakerFreeWebApp.Data;
using TestMakerFreeWebApp.Data.Models;
using TestMakerFreeWebApp.ViewModels;

namespace TestMakerFreeWebApp.Controllers
{
    public class ResultController : BaseApiController
    {
        #region Constructor

        public ResultController(ApplicationDbContext db,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration) : base(db, roleManager, userManager, configuration)
        {
        }

        #endregion Constructor

        #region RESTful conventions methods

        /// <summary>
        /// GET: api/result/{id}
        /// Retrieves the Result with the given {id}
        /// </summary>
        /// <param name="id">The ID of an existing Result</param>
        /// <returns>the Result with the given {id}</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var result = this.Db.Results
                .FirstOrDefault(i => i.Id == id);

            // handle requests asking for non-existing results
            if (result == null)
            {
                return NotFound($"Result ID {id} has not been found.");
            }

            return new JsonResult(
                result.Adapt<ResultViewModel>(),
                this.JsonSettings);
        }

        /// <summary>
        /// Adds a new Result to the Database
        /// </summary>
        /// <param name="model">The ResultViewModel containing the data to insert</param>
        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody]ResultViewModel model)
        {
            // return a generic HTTP Status 500 (Server Error)
            // if the client payload is invalid.
            if (model == null)
            {
                return BadRequest();
            }

            // map the ViewModel to the Model
            var result = model.Adapt<Result>();

            // override those properties
            // that should be set from the server-side only
            result.CreatedDate = DateTime.UtcNow;
            result.LastModifiedDate = result.CreatedDate;

            // add the new result
            this.Db.Results.Add(result);

            // persist the changes into the Database.
            this.Db.SaveChanges();

            // return the newly-created Result to the client.
            return new JsonResult(result.Adapt<ResultViewModel>(),
                this.JsonSettings);
        }

        /// <summary>
        /// Edit the Result with the given {id}
        /// </summary>
        /// <param name="model">The ResultViewModel containing the data to update</param>
        [HttpPut]
        [Authorize]
        public IActionResult Put([FromBody]ResultViewModel model)
        {
            // return a generic HTTP Status 500 (Server Error)
            // if the client payload is invalid.
            if (model == null) return new StatusCodeResult(500);
            // retrieve the result to edit
            var result = this.Db.Results.FirstOrDefault(q => q.Id ==
                                                      model.Id);
            // handle requests asking for non-existing results
            if (result == null)
            {
                return NotFound($"Result ID {model.Id} has not been found.");
            }

            // handle the update (without object-mapping)
            // by manually assigning the properties
            // we want to accept from the request
            result.QuizId = model.QuizId;
            result.Text = model.Text;
            result.MinValue = model.MinValue;
            result.MaxValue = model.MaxValue;
            result.Notes = model.Notes;

            // properties set from server-side
            result.LastModifiedDate = DateTime.UtcNow;

            // persist the changes into the Database.
            this.Db.SaveChanges();

            // return the updated Quiz to the client.
            return new JsonResult(result.Adapt<ResultViewModel>(),
                this.JsonSettings);
        }

        /// <summary>
        /// Deletes the Result with the given {id} from the Database
        /// </summary>
        /// <param name="id">The ID of an existing Result</param>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            // retrieve the result from the Database
            var result = this.Db.Results
                .FirstOrDefault(i => i.Id == id);

            // handle requests asking for non-existing results
            if (result == null)
            {
                return NotFound($"Result ID {id} has not been found.");
            }

            // remove the quiz from the DbContext.
            this.Db.Results.Remove(result);

            // persist the changes into the Database.
            this.Db.SaveChanges();

            // return an HTTP Status 200 (OK).
            return Ok();
        }

        #endregion RESTful conventions methods

        // GET api/question/all
        [HttpGet("All/{quizId}")]
        public IActionResult All(int quizId)
        {
            var results = this.Db.Results
                .Where(q => q.QuizId == quizId)
                .ToList();

            return new JsonResult(
                results.Adapt<List<ResultViewModel>>(),
                this.JsonSettings);
        }
    }
}