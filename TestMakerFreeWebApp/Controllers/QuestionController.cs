using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TestMakerFreeWebApp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using TestMakerFreeWebApp.Data;
using TestMakerFreeWebApp.Data.Models;

namespace TestMakerFreeWebApp.Controllers
{
    public class QuestionController : BaseApiController
    {

        #region Constructor

        public QuestionController(ApplicationDbContext db,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration) : base(db, roleManager, userManager, configuration)
        { 
        }

        #endregion

        #region RESTful conventions methods
        /// <summary>
        /// Retrieves the Question with the given {id}
        /// </summary>
        /// <param name="id">The ID of an existing Question</param>
        /// <returns>the Question with the given {id}</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var question = this.Db.Questions.FirstOrDefault(q => q.Id == id);

            if (question == null)
            {
                return NotFound($"Question ID {id} has not been found");
            }

            return new JsonResult(
                question.Adapt<QuestionViewModel>(),
                this.JsonSettings);
        }

        /// <summary>
        /// Adds a new Question to the Database
        /// </summary>
        /// <param name="model">The QuestionViewModel containing the data to insert</param>
        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody]QuestionViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            // map the ViewModel to the Model
            var question = model.Adapt<Question>();

            // properties set from server-side
            question.CreatedDate = DateTime.UtcNow;
            question.LastModifiedDate = question.CreatedDate;

            this.Db.Questions.Add(question);
            this.Db.SaveChanges();

            return new JsonResult(
                question.Adapt<QuestionViewModel>(),
                this.JsonSettings);

        }

        /// <summary>
        /// Edit the Question with the given {id}
        /// </summary>
        /// <param name="model">The QuestionViewModel containing the data to update</param>
        [HttpPut]
        [Authorize]
        public IActionResult Put([FromBody]QuestionViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var question = this.Db.Questions.FirstOrDefault(q => q.Id == model.Id);

            if (question == null)
            {
                return NotFound($"Question ID {model.Id} has not been found.");
            }

            // handle the update (without object-mapping)
            // by manually assigning the properties
            // we want to accept from the request
            question.QuizId = model.QuizId;
            question.Text = model.Text;
            question.Notes = model.Notes;

            // properties set from server-side
            question.LastModifiedDate = DateTime.UtcNow;

            // persist the changes into the Database.
            this.Db.SaveChanges();

            // return the updated Quiz to the client.
            return new JsonResult(question.Adapt<QuestionViewModel>(),
                this.JsonSettings);
        }

        /// <summary>
        /// Deletes the Question with the given {id} from the Database
        /// </summary>
        /// <param name="id">The ID of an existing Question</param>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var question = this.Db.Questions.FirstOrDefault(q => q.Id == id);

            if (question == null)
            {
                return NotFound($"Question ID {id} has not been found.");
            }

            this.Db.Questions.Remove(question);

            this.Db.SaveChanges();

            return Ok();
        }
        #endregion

        // GET api/question/all
        [HttpGet("all/{quizId}")]
        public IActionResult All(int quizId)
        {
            var questions = this.Db.Questions
                .Where(q => q.QuizId == quizId)
                .ToList();

            return new JsonResult(
                questions.Adapt<List<QuestionViewModel>>(),
                this.JsonSettings);
        }
    }
}