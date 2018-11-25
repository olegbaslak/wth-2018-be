using System.Collections.Generic;

namespace WikiQuiz.Models
{
    public class TriviaQuestion
    {
        public string Question { get; set; }
        public List<string> Answers { get; set; }
        public int Correct { get; set; }
    }
}