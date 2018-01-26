using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TestMakerFreeWebApp.Data;

namespace TestMakerFreeWebApp.Controllers
{
    [Route("api/[controller]")]
    public class BaseApiController : Controller
    {
        #region Constructor

        public BaseApiController(ApplicationDbContext db)
        {
            this.Db = db;
            this.JsonSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };
        }

        #endregion

        #region Shared Properties

        protected ApplicationDbContext Db { get; private set; }
        protected JsonSerializerSettings JsonSettings { get; private set; }

        #endregion
    }
}
