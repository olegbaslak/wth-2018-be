using System.Collections.Generic;

namespace WikiQuiz.Models
{
    public class Trivia
    {
        public List<TriviaQuestion> Questions { get; set; }

        public Trivia()
        {
            Questions = new List<TriviaQuestion>();
        }

        public void Add(TriviaQuestion question)
        {
            Questions.Add(question);
        }
    }
}
