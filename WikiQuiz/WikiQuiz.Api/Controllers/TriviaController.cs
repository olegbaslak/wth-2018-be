using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WikiQuiz.Services;

namespace WikiQuiz.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriviaController : ControllerBase
    {
        // GET api/trivia
        [HttpGet]
        public JsonResult Get()
        {
            Console.WriteLine("Queried");
            var service = new TriviaCreationService();
            
            return new JsonResult(service.Create(4));
        }
    }
}
