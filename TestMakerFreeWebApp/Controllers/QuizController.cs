using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TestMakerFreeWebApp.Data;
using TestMakerFreeWebApp.Data.Models;
using TestMakerFreeWebApp.ViewModels;

namespace TestMakerFreeWebApp.Controllers
{
    public class QuizController : BaseApiController
    {

        #region Constructor

        public QuizController(ApplicationDbContext db,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration) : base(db, roleManager, userManager, configuration)
        {
        }

        #endregion

        #region RESTful conventions methods
        /// <summary>
        /// GET: api/quiz/{id}
        /// Retrieves the Quiz with the given {id}
        /// </summary>
        /// <param name="id">The ID of an existing Quiz</param>
        /// <returns>the Quiz with the given {id}</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var quiz = this.Db.Quizzes.FirstOrDefault(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound($"Quiz {id} has not been found.");
            }

            return new JsonResult(
                quiz.Adapt<QuizViewModel>(),
                this.JsonSettings);
        }

        /// <summary>
        /// Adds a new Quiz to the Database
        /// </summary>
        /// <param name="model">The QuizViewModel containing the data to insert</param>
        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody]QuizViewModel model)
        {
            // return a generic HTTP Status 500 (Server Error)
            // if the client payload is invalid.
            if (model == null)
            {
                return BadRequest();
            }

            // handle the insert (without object-mapping)
            var quiz = new Quiz
            {
                Title = model.Title,
                Description = model.Description,
                Text = model.Text,
                Notes = model.Notes,
                CreatedDate = DateTime.UtcNow
            };

            // properties set from server-side
            quiz.LastModifiedDate = quiz.CreatedDate;

            // Set a temporary author using the Admin user's userId
            // as user login isn't supported yet: we'll change this later on.
            quiz.UserId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            // add the new quiz
            this.Db.Quizzes.Add(quiz);
            // persist the changes into the Database.
            this.Db.SaveChanges();
            // return the newly-created Quiz to the client.
            return new JsonResult(quiz.Adapt<QuizViewModel>(),
                this.JsonSettings);
        }

        /// <summary>
        /// Edit the Quiz with the given {id}
        /// </summary>
        /// <param name="model">The QuizViewModel containing the data to update</param>
        [HttpPut]
        [Authorize]
        public IActionResult Put([FromBody]QuizViewModel model)
        {
            // return a generic HTTP Status 500 (Server Error)
            // if the client payload is invalid.
            if (model == null)
            {
                return BadRequest();
            }

            // retrieve the quiz to edit
            var quiz = this.Db.Quizzes.FirstOrDefault(q => q.Id ==
                                                    model.Id);

            // handle requests asking for non-existing quizzes
            if (quiz == null)
            {
                return NotFound($"Quiz ID {model.Id} has not been found");
            }

            // handle the update (without object-mapping)
            // by manually assigning the properties
            // we want to accept from the request
            quiz.Title = model.Title;
            quiz.Description = model.Description;
            quiz.Text = model.Text;
            quiz.Notes = model.Notes;

            // properties set from server-side
            quiz.LastModifiedDate = DateTime.UtcNow;

            // persist the changes into the Database.
            this.Db.SaveChanges();

            // return the updated Quiz to the client.
            return new JsonResult(quiz.Adapt<QuizViewModel>(),
                this.JsonSettings);
        }

        /// <summary>
        /// Deletes the Quiz with the given {id} from the Database
        /// </summary>
        /// <param name="id">The ID of an existing Quiz</param>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            // retrieve the quiz from the Database
            var quiz = this.Db.Quizzes
                .FirstOrDefault(i => i.Id == id);

            // handle requests asking for non-existing quizzes
            if (quiz == null)
            {
                return NotFound($"Quiz ID {id} has not been found");
            }

            // remove the quiz from the DbContext.
            this.Db.Quizzes.Remove(quiz);

            // persist the changes into the Database.
            this.Db.SaveChanges();

            // return an HTTP Status 200 (OK).
            return Ok();

        }
        #endregion

        // GET: api/latest
        #region Attribute-based routing methods
        /// <summary>
        /// GET: api/quiz/latest
        /// Retrieves the {num} latest Quizzes
        /// </summary>
        /// <param name="num">the number of quizzes to retrieve</param>
        /// <returns>the {num} latest Quizzes</returns>
        [HttpGet("latest/{num:int?}")]
        public IActionResult Latest(int num = 10)
        {
            var latest = this.Db.Quizzes
                .OrderByDescending(q => q.CreatedDate)
                .Take(num)
                .ToList();

            // output the result in JSON format
            return new JsonResult(
                latest.Adapt<List<QuizViewModel>>(),
                this.JsonSettings);
        }

        /// <summary>
        /// GET: api/quiz/bytitle
        /// Retrieves the {num} Quizzes sorted by title {A to Z}
        /// </summary>
        /// <param name="num">the number of quizzes to retrieve</param>
        /// <returns>{num} Quizzes sorted by title</returns>
        [HttpGet("bytitle/{num:int?}")]
        public IActionResult ByTitle(int num = 10)
        {
            var byTitle = this.Db.Quizzes
                .OrderBy(q => q.Title)
                .Take(num)
                .ToList();

            return new JsonResult(
                byTitle.Adapt<List<QuizViewModel>>(),
                this.JsonSettings);
        }

        /// <summary>
        /// GET: api/quiz/mostViewed
        /// Retrieves the {num} random quizzes
        /// <param name="num">the number of quizzes to retrieve</param>
        /// <return>{num} random quizzes</return>
        /// </summary>
        [HttpGet("random/{num:int?}")]
        public IActionResult Random(int num = 10)
        {
            var random = this.Db.Quizzes
                .OrderBy(q => Guid.NewGuid())
                .Take(num)
                .ToList();

            return new JsonResult(
                random.Adapt<List<QuizViewModel>>(),
                this.JsonSettings);
        }
        #endregion


    }
}
