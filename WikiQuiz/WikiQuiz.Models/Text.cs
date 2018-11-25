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
            InnerText = text;
            Sentances = Regex.Split(text, SentancesSplitPattern, RegexOptions.Singleline)
                .Select(s => new Sentance(s))
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
