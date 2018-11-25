using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WikiQuiz.Models
{
    public class Sentance
    {
        private const string WordsSplitPattern = @"[ ,.:?!;—()\-]+";

        public string Text { get; set; }
        public IList<string> Words { get; set; } = new List<string>();

        public override string ToString() => Text;

        public Sentance(string sentanceText)
        {
            Text = sentanceText;
            Words = Regex.Split(sentanceText, WordsSplitPattern);
        }
    }
}