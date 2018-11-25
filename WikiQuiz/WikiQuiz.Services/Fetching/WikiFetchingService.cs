using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.XPath;
using WikiQuiz.Models;
using WikiQuiz.Services.Extensions;

namespace WikiQuiz.Services.Fetching
{
    internal class WikiFetchingService : IFetchingService
    {
        private const string ApiEndpoint = "https://ru.wikipedia.org/wiki/Special:Random";

        private const string SquareBracetsPattern = @"\[.*?\]";

        private readonly IList<string> GarbishWords = new List<string> {
        "еще", "него", "сказать", "а", "ж", "нее", "со", "без", "же", "ней", "совсем", "более",
            "жизнь", "нельзя", "так", "больше", "за", "нет", "такой", "будет", "зачем", "ни", "там",
"будто", "здесь", "нибудь", "тебя", "бы", "и", "никогда", "тем", "был", "из", "ним", "теперь", "была", "из-за", "них", "то",
"были", "или", "ничего", "тогда", "было", "им", "но", "того",
"быть", "иногда", "ну", "тоже",
"в", "   их", "о", "только",
"вам","к","об","том",
"вас","кажется","один","тот",
"вдруг","как","он","три",
"ведь","какая","она","тут",
"во","какой","они","ты",
"вот","когда","опять","у",
"впрочем","конечно","от","уж",
"все","которого","перед","уже",
"всегда","которые","по","хорошо",
"всего","кто","под","хоть",
"всех","куда","после", "чего",
"всю","ли","потом","человек",
"вы","лучше","потому","чем",
"г","между","почти","через",
"где","меня","при","что",
"говорил","мне","про","чтоб",
"да","много","раз","чтобы",
"даже","может","разве","чуть",
"два","можно","с","эти",
"для","мой","сам","этого",
"до","моя","свое","этой",
"другой","мы","свою","этом",
"его","на","себе","этот",
"ее","над","себя","эту",
"ей","надо","сегодня","я",
"ему","наконец","сейчас",
"если","нас","сказал",
"есть","не","сказала"
        };

        public async Task<string> GetRandom()
        {
            var client = new RestClient(ApiEndpoint);
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteTaskAsync(request);
            return response.Content;
        }

        public TriviaQuestion Parse(string htmlContent)
        {
            //return ParseRandomly(htmlContent);
            return ParseDefenition(htmlContent);
        }

        private TriviaQuestion ParseDefenition(string htmlContent)
        {
            const string separator = "—";

            var paragraph = ExctractFirstParagraph(htmlContent);
            paragraph = Regex.Replace(paragraph, SquareBracetsPattern, string.Empty);
            var paragraphEntity = new Text(paragraph);
            var fullDefinition = paragraphEntity.Sentances[0].Text;

            var separatorIndex = fullDefinition.IndexOf(separator);
            var definition = fullDefinition.Substring(0, separatorIndex - 1);
            definition = Regex.Replace(definition, @"(\(.*?\))", string.Empty);
            var definitionText = fullDefinition.Substring(separatorIndex + 2);
            definitionText = definitionText[0].ToString().ToUpper() + definitionText.Substring(1);


            // Wrong angsers:
            var text = string.Join(". ", ExtractParagraps(htmlContent));
            var cleanText = new Text(Regex.Replace(text, SquareBracetsPattern, string.Empty));
            var cleanSentances = CleanFromGarbish(cleanText.Sentances.Select(s => s.Text)).ToList();

            var valueableWords = cleanSentances
                .SelectMany(s =>
                    new Sentance(s).Words
                        .Where(w =>
                            !s.StartsWith(w) &&
                            char.IsUpper(w[0])).ToList())
                .Distinct().ToList();

            var wrong1 = string.Empty;
            var wrong2 = string.Empty;
            var wrong3 = string.Empty;

            if (valueableWords.Count > 1)
            {
                wrong1 = valueableWords.RandomElement();
                valueableWords.Remove(wrong1);
            }
            else wrong1 = new Sentance(cleanSentances.RandomElement()).Words.RandomElement();

            if (valueableWords.Count > 1)
            {
                wrong2 = valueableWords.RandomElement();
                valueableWords.Remove(wrong2);
            }
            else wrong2 = new Sentance(cleanSentances.RandomElement()).Words.RandomElement();

            if (valueableWords.Count > 1)
            {
                wrong3 = valueableWords.RandomElement();
                valueableWords.Remove(wrong3);
            }
            else wrong3 = new Sentance(cleanSentances.RandomElement()).Words.RandomElement();

            var shuffledAnswers = new List<string> { wrong1, "*", wrong2, wrong3 };
            shuffledAnswers.Shuffle();
            var correctIndex = shuffledAnswers.IndexOf("*");
            shuffledAnswers[correctIndex] = definition;

            var triviaQuestion = new TriviaQuestion
            {
                Question = definitionText,
                Correct = correctIndex + 1,
                Answers = shuffledAnswers
            };

            return triviaQuestion;
        }

        private TriviaQuestion ParseRandomly(string htmlContent)
        {
            var paragraphs = ExtractParagraps(htmlContent).ToList();
            var randomParagraphText = paragraphs.RandomElement();

            var cleanText = Regex.Replace(randomParagraphText, SquareBracetsPattern, string.Empty);
            var textEntity = new Text(cleanText);
            textEntity.RemoveOneWordSentances();

            var sentanceEntity = textEntity.Sentances.RandomElement();
            var words = sentanceEntity.Words;
            var cleanWords = CleanFromGarbish(words);

            var randomWord = cleanWords.RandomElement();
            var boilerPlate = new string('_', randomWord.Length);

            var question = sentanceEntity.Text.Replace(randomWord, boilerPlate);

            var shuffledAnswers = new List<string> { "*", "W1", "W2", "W3" };
            shuffledAnswers.Shuffle();
            var correctIndex = shuffledAnswers.IndexOf("*");
            shuffledAnswers[correctIndex] = randomWord;

            var triviaQuestion = new TriviaQuestion
            {
                Question = question,
                Correct = correctIndex + 1,
                Answers = shuffledAnswers
            };
            return triviaQuestion;
        }

        private IEnumerable<string> ExtractParagraps(string htmlContent)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);
            var xpath = XPathExpression.Compile("//div[contains(@id,'content')]//p[text() and not(./br)]");
            var paragraphNodes = htmlDocument.DocumentNode.SelectNodes(xpath);
            var paragraphs = paragraphNodes.Select(p => p.InnerText).ToList();
            var paragraphsClean = paragraphs.Select(p => p != "\n" && p != " \n" && p != "→\n").ToList();

            return paragraphs;
        }

        private string ExctractFirstParagraph(string htmlContent)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);
            var xpath = XPathExpression.Compile("//div[contains(@id,'content')]//p[text() and not(./br)]");
            var paragraphNode = htmlDocument.DocumentNode.SelectSingleNode(xpath);
            return paragraphNode.InnerText;
        }

        private IList<string> CleanFromGarbish(IEnumerable<string> words)
        {
            var cleanWords = new List<string>();

            foreach (var currentWord in words)
            {
                if (!GarbishWords.Contains(currentWord))
                {
                    cleanWords.Add(currentWord);
                }
            }

            return cleanWords;
        }

        public async Task<TriviaQuestion> GetRandomQuestion()
        {
            var triesCount = 5;

            for (var i = 1; i <= triesCount; i++)
            {
                try
                {
                    var text = await GetRandom();
                    var question = Parse(text);
                    return question;
                }
                catch (Exception e)
                {
                    if (i == triesCount)
                    {
                        Console.WriteLine($"Error: {e.Message}. {e}");
                        throw;
                    }
                }
            }

            throw new Exception("Something went wrong during fetching question");
        }
    }
}