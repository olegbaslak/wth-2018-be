using System.Collections.Generic;

namespace WikiQuiz.Models
{
    public class TriviaQuestion
    {
        public string Question { get; set; }
        public List<string> WrongAnswers { get; set; }
        public string Answer { get; set; }
    }
}