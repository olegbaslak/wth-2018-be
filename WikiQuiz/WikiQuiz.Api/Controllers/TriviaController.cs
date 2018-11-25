using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WikiQuiz.Services;
using static WikiQuiz.Services.TriviaCreationService;

namespace WikiQuiz.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriviaController : ControllerBase
    {
        // GET api/trivia
        [HttpGet]
        public async Task<JsonResult> Get()
        {
            Console.WriteLine("Queried");
            var service = new TriviaCreationService(Source.Any);
            var trivia = await service.Create(5);

            return new JsonResult(trivia);
        }

        // GET api/trivia/{count}?source=wiki
        [HttpGet("{count}/{source?}")]
        public async Task<JsonResult> Get(int count, [FromQuery] string s = "Any")
        {
            var isParsed = Enum.TryParse(s, out Source quizSource);

            var service = new TriviaCreationService(quizSource);
            var trivia = await service.Create(count);

            return new JsonResult(trivia);
        }
    }
}
