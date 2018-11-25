using Bogus;
using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using WikiQuiz.Models;
using WikiQuiz.Services.Extensions;

namespace WikiQuiz.Services.Fetching
{
    public class MemepediaFetchingService : IFetchingService
    {
        private const string MainPageUrl = "https://memepedia.ru/memoteka/";

        private XPathExpression AllMemesXPath(string except) => XPathExpression.Compile($"//*[@class='alpha-block']//a[not(text()='{except}')]");
        private XPathExpression TwitterEmbdedXPath => XPathExpression.Compile("//*[contains(@class,'twitter-tweet')]");
        private XPathExpression VideoInPostXPath => XPathExpression.Compile("//*[not(contains(@class,'EmbedBrokenMedia')) and not(contains(@src,'youtube')) and not(contains(@class,'ytp'))]/div/video");
        private XPathExpression ImageInPostXPath => XPathExpression.Compile("//div[@class='bb-col col-site-main']//img[contains(@class,'alignnone')]");

        private XPathExpression MemeCategoryXPath => XPathExpression.Compile("//*[@class='alpha-nav-ul']//a");
        private XPathExpression MemeByCategoryXPath(string category) => XPathExpression.Compile($"//*[@class='alpha-block' and h4[text()='{category}']]//a");

        private readonly Faker _faker = new Faker();

        public async Task<string> FetchContent(string page)
        {
            var client = new RestClient(page);
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteTaskAsync(request);
            return response.Content;
        }

        private async Task<TriviaQuestion> GetRandomQuestionAndParse()
        {
            var mainPageContent = await FetchContent(MainPageUrl);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(mainPageContent);
            var categories = htmlDocument.DocumentNode.SelectNodes(MemeCategoryXPath).Select(c => c.InnerText);
            var randomCategory = _faker.PickRandom(categories);
            var memesInCategory = htmlDocument.DocumentNode.SelectNodes(MemeByCategoryXPath(randomCategory)).ToList();
            var randomMemeNode = _faker.PickRandom(memesInCategory);
            var memeTitle = randomMemeNode.InnerText;
            var memeUrl = randomMemeNode.GetAttributeValue("href", "");

            var allMemeA = htmlDocument.DocumentNode.SelectNodes(AllMemesXPath(memeTitle)).ToList();

            var memePageContent = await FetchContent(memeUrl);
            htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(memePageContent);

            var imagesHtlm = htmlDocument.DocumentNode.SelectNodes(ImageInPostXPath)?.Select(n => n.OuterHtml) ?? new List<string>();
            var videosHtlm = htmlDocument.DocumentNode.SelectNodes(VideoInPostXPath)?.Select(n => n.OuterHtml) ?? new List<string>();
            //var twitter = htmlDocument.DocumentNode.SelectNodes(TwitterEmbdedXPath).Select(n => n.OuterHtml);

            var allMemeImagesHtml = imagesHtlm.Concat(videosHtlm).ToList();
            var randomMemeImageHtml = _faker.PickRandom(allMemeImagesHtml);

            var wrong1 = _faker.PickRandom(allMemeA);
            allMemeA.Remove(wrong1);
            var wrong2 = _faker.PickRandom(allMemeA);
            allMemeA.Remove(wrong2);
            var wrong3 = _faker.PickRandom(allMemeA);

            var shuffledAnswers = new List<string> { "*", wrong1.InnerText, wrong2.InnerText, wrong3.InnerText };
            shuffledAnswers.Shuffle();
            var correctIndex = shuffledAnswers.IndexOf("*");
            shuffledAnswers[correctIndex] = memeTitle;

            var triviaQuestion = new TriviaQuestion
            {
                Question = randomMemeImageHtml,
                Correct = correctIndex + 1,
                Answers = shuffledAnswers
            };

            return triviaQuestion;
        }

        public async Task<TriviaQuestion> GetRandomQuestion()
        {
            var triesCount = 5;

            for (var i = 1; i <= triesCount; i++)
            {
                try
                {
                    var question = await GetRandomQuestionAndParse();
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

        public TriviaQuestion Parse(string htmlContent)
        {
            throw new NotImplementedException();
        }
    }
}