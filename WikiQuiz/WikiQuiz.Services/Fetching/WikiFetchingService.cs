using Bogus;
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

        private readonly Faker _faker = new Faker();

        private readonly IList<string> GarbishWords = new List<string> {
        "а",
        "и",
        "на",
        "в",
        "про",
        "от",
        ""
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
            var paragraphEntity = new Text(paragraph);
            var fullDefinition = paragraphEntity.Sentances[0].Text;

            var separatorIndex = fullDefinition.IndexOf(separator);
            var definition = fullDefinition.Substring(0, separatorIndex - 1);
            var definitionText = fullDefinition.Substring(separatorIndex + 2);
            definitionText = definitionText[0].ToString().ToUpper() + definitionText.Substring(1);

            var shuffledAnswers = new List<string> { "*", "W1", "W2", "W3" };
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
            var paragraphs = ExtractParagraps(htmlContent);
            var randomParagraphText = _faker.PickRandom(paragraphs);

            var cleanText = Regex.Replace(randomParagraphText, SquareBracetsPattern, string.Empty);
            var textEntity = new Text(cleanText);
            textEntity.RemoveOneWordSentances();

            var sentanceEntity = _faker.PickRandom(textEntity.Sentances);
            var words = sentanceEntity.Words;
            var cleanWords = CleanFromGarbish(words);

            var randomWord = _faker.PickRandom(cleanWords);
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