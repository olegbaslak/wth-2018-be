using System.Collections.Generic;
using WikiQuiz.Models;

namespace WikiQuiz.Services
{
    public class TriviaCreationService
    {
        public Trivia Create(int count = 1)
        {
            Trivia trivia = new Trivia();

            for (int i = 0; i < count; i++)
            {
                var q1 = new TriviaQuestion { Question = $"This is q-{i}", Answer = $"Answer for {i}", WrongAnswers = new List<string> { "Wrong1", "Wrong2", "Wrong3", "Wrong4" } };
                trivia.Add(q1);
            }

            return trivia;
        }
    }
}