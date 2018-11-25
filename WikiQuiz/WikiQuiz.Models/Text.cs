using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WikiQuiz.Models
{
    public class Text
    {
        private const string SentancesSplitPattern = @"[.?!]+";

        public string InnerText { get; set; }
        public IList<Sentance> Sentances { get; set; } = new List<Sentance> { };

        public override string ToString() => InnerText;

        public Text(string text)
        {
            text = text.Replace("\r", string.Empty).Replace("\n", string.Empty);

            // Removing all text in bracets
            text = Regex.Replace(text, @"(\(.*?\))", string.Empty);

            // Remove ., and . in parenthes
            text = Regex.Replace(text, @"(\.,)", string.Empty);
            text = Regex.Replace(text, @"\(.*?(\.).*?\)", string.Empty);

            InnerText = text;
            Sentances = Regex.Split(text, SentancesSplitPattern, RegexOptions.Singleline)
                .Where(s=>s.Trim() != string.Empty)
                .Select(s => new Sentance(s.Trim()))
                .Where(s => s.Words.Count > 0)
                .ToList();
        }

        public void RemoveOneWordSentances()
        {
            var newSentances = new List<Sentance>();

            foreach (var item in Sentances)
            {
                if (item.Words.Count > 1)
                {
                    newSentances.Add(item);
                }
            }

            Sentances = newSentances;
        }
    }
}
