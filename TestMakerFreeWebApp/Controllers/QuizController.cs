using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TestMakerFreeWebApp.Data;
using TestMakerFreeWebApp.Data.Models;
using TestMakerFreeWebApp.ViewModels;

namespace TestMakerFreeWebApp.Controllers
{
    [Route("api/[controller]")]
    public class QuizController : Controller
    {
        #region Private Fields

        private readonly ApplicationDbContext db;

        #endregion

        #region Constructor

        public QuizController(ApplicationDbContext db)
        {
            this.db = db;
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
            var quiz = this.db.Quizzes.FirstOrDefault(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound(new
                {
                    Error = $"Quiz {id} has not been found."
                });
            }

            return new JsonResult(
                quiz.Adapt<QuizViewModel>(),
                new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                });
        }

        /// <summary>
        /// Adds a new Quiz to the Database
        /// </summary>
        /// <param name="model">The QuizViewModel containing the data to insert</param>
        [HttpPost]
        public IActionResult Post([FromBody]QuizViewModel model)
        {
            // return a generic HTTP Status 500 (Server Error)
            // if the client payload is invalid.
            if (model == null) return new StatusCodeResult(500);

            // handle the insert (without object-mapping)
            var quiz = new Quiz();

            // properties taken from the request
            quiz.Title = model.Title;
            quiz.Description = model.Description;
            quiz.Text = model.Text;
            quiz.Notes = model.Notes;

            // properties set from server-side
            quiz.CreatedDate = DateTime.UtcNow;
            quiz.LastModifiedDate = quiz.CreatedDate;

            // Set a temporary author using the Admin user's userId
            // as user login isn't supported yet: we'll change this later on.
            quiz.UserId = this.db.Users
                .FirstOrDefault(u => u.Username == "Admin").Id;

            // add the new quiz
            this.db.Quizzes.Add(quiz);
            // persist the changes into the Database.
            this.db.SaveChanges();
            // return the newly-created Quiz to the client.
            return new JsonResult(quiz.Adapt<QuizViewModel>(),
                new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                });
        }

        /// <summary>
        /// Edit the Quiz with the given {id}
        /// </summary>
        /// <param name="model>The QuizViewModel containing the data to update</param>
        [HttpPut]
        public IActionResult Put([FromBody]QuizViewModel model)
        {
            // return a generic HTTP Status 500 (Server Error)
            // if the client payload is invalid.
            if (model == null) return new StatusCodeResult(500);

            // retrieve the quiz to edit
            var quiz = this.db.Quizzes.FirstOrDefault(q => q.Id ==
                                                    model.Id);

            // handle requests asking for non-existing quizzes
            if (quiz == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Quiz ID {0} has not been found",
                        model.Id)
                });
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
            this.db.SaveChanges();

            // return the updated Quiz to the client.
            return new JsonResult(quiz.Adapt<QuizViewModel>(),
                new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                });
        }

        /// <summary>
        /// Deletes the Quiz with the given {id} from the Database
        /// </summary>
        /// <param name="id">The ID of an existing Quiz</param>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            // retrieve the quiz from the Database
            var quiz = this.db.Quizzes
                .FirstOrDefault(i => i.Id == id);

            // handle requests asking for non-existing quizzes
            if (quiz == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Quiz ID {0} has not been found", id)
                });
            }

            // remove the quiz from the DbContext.
            this.db.Quizzes.Remove(quiz);

            // persist the changes into the Database.
            this.db.SaveChanges();

            // return an HTTP Status 200 (OK).
            return new OkResult();
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
            var latest = this.db.Quizzes
                .OrderByDescending(q => q.CreatedDate)
                .Take(num)
                .ToList();

            // output the result in JSON format
            return new JsonResult(
                latest.Adapt<List<QuizViewModel>>(),
                new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                });
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
            var byTitle = this.db.Quizzes
                .OrderBy(q => q.Title)
                .Take(num)
                .ToList();

            return new JsonResult(
                byTitle.Adapt<List<QuizViewModel>>(),
                new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                });
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
            var random = this.db.Quizzes
                .OrderBy(q => Guid.NewGuid())
                .Take(num)
                .ToList();

            return new JsonResult(
                random.Adapt<List<QuizViewModel>>(),
                new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                });
        }
        #endregion


    }
}
