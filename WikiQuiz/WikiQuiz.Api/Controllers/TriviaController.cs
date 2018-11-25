using System;
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
            Console.WriteLine($"GET api/trivia/{5}?s={Source.Any}");
            var service = new TriviaCreationService(Source.Any);
            var trivia = await service.Create(5);

            return new JsonResult(trivia);
        }

        // GET api/trivia/{count}?source=Any
        [HttpGet("{count}/{source?}")]
        public async Task<JsonResult> Get(int count, [FromQuery] string s = "Any")
        {
            Console.WriteLine($"GET api/trivia/{count}?s={s}");

            var isParsed = Enum.TryParse(s, out Source quizSource);

            var service = new TriviaCreationService(quizSource);
            var trivia = await service.Create(count);

            return new JsonResult(trivia);
        }
    }
}
