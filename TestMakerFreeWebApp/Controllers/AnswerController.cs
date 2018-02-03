using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TestMakerFreeWebApp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TestMakerFreeWebApp.Data;
using TestMakerFreeWebApp.Data.Models;

namespace TestMakerFreeWebApp.Controllers
{
    public class AnswerController : BaseApiController
    {

        #region Constructor

        public AnswerController(ApplicationDbContext db,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration) : base(db, roleManager, userManager, configuration)
        {
        }

        #endregion

        #region RESTful conventions methods
        /// <summary>
        /// Retrieves the Answer with the given {id}
        /// </summary>
        /// <param name="id">The ID of an existing Answer</param>
        /// <returns>the Answer with the given {id}</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var answer = this.Db.Answers.FirstOrDefault(a => a.Id == id);

            if (answer == null)
            {
                return NotFound($"Answer {id} has not been found");
            }

            return new JsonResult(
                answer.Adapt<AnswerViewModel>(),
                this.JsonSettings);
        }

        /// <summary>
        /// Adds a new Answer to the Database
        /// </summary>
        /// <param name="model">The AnswerViewModel containing the data to insert</param>
        [HttpPost]
        public IActionResult Post([FromBody]AnswerViewModel model)
        {
            // return a generic HTTP Status 500 (Server Error)
            // if the client payload is invalid.
            if (model == null)
            {
                return BadRequest();
            }

            // map the ViewModel to the Model
            var answer = model.Adapt<Answer>();

            answer.CreatedDate = DateTime.UtcNow;
            answer.LastModifiedDate = answer.CreatedDate;

            // add the new answer
            this.Db.Answers.Add(answer);
            // persist the changes into the Database.
            this.Db.SaveChanges();

            return new JsonResult(
                answer.Adapt<AnswerViewModel>(),
                this.JsonSettings);
        }

        /// <summary>
        /// Edit the Answer with the given {id}
        /// </summary>
        /// <param name="model">The AnswerViewModel containing the data to update</param>
        [HttpPut]
        public IActionResult Put([FromBody]AnswerViewModel model)
        {
            // return a generic HTTP Status 500 (Server Error)
            // if the client payload is invalid.
            if (model == null)
            {
                return BadRequest();
            }

            // retrieve the answer to edit
            var answer = this.Db.Answers.FirstOrDefault(q => q.Id ==
                                                      model.Id);

            // handle requests asking for non-existing answers
            if (answer == null)
            {

                return NotFound($"Answer {model.Id} has not been found");

            }

            // handle the update (without object-mapping)
            // by manually assigning the properties
            // we want to accept from the request
            answer.QuestionId = model.QuestionId;
            answer.Text = model.Text;
            answer.Value = model.Value;
            answer.Notes = model.Notes;

            // properties set from server-side
            answer.LastModifiedDate = DateTime.UtcNow;

            // persist the changes into the Database.
            this.Db.SaveChanges();

            // return the updated Quiz to the client.
            return new JsonResult(answer.Adapt<AnswerViewModel>(),
                this.JsonSettings);
        }

        /// <summary>
        /// Deletes the Answer with the given {id} from the Database
        /// </summary>
        /// <param name="id">The ID of an existing Answer</param>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            // retrieve the answer from the Database
            var answer = this.Db.Answers
                .FirstOrDefault(i => i.Id == id);

            // handle requests asking for non-existing answers
            if (answer == null)
            {
                return NotFound($"Answer {id} has not been found");
            }

            // remove the quiz from the DbContext.
            this.Db.Answers.Remove(answer);

            // persist the changes into the Database.
            this.Db.SaveChanges();

            // return an HTTP Status 200 (OK).
            return Ok();

        }
        #endregion

        // GET api/answer/all
        [HttpGet("all/{questionId}")]
        public IActionResult All(int questionId)
        {
            var answers = this.Db.Answers
                .Where(q => q.QuestionId == questionId)
                .ToList();

            return new JsonResult(
                answers.Adapt<List<AnswerViewModel>>(),
                this.JsonSettings);
        }
    }
}