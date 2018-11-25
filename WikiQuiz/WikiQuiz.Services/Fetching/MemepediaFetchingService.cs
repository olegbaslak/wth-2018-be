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
    public class MemepediaFetchingService : IFetchingService
    {
        private const string MainPageUrl = "https://memepedia.ru/memoteka/";

        private XPathExpression AllMemesXPath(string except) => XPathExpression.Compile($"//*[@class='alpha-block']//a[not(text()='{except}')]");
        private XPathExpression TwitterEmbdedXPath => XPathExpression.Compile("//*[contains(@class,'twitter-tweet')]");
        private XPathExpression VideoInPostXPath => XPathExpression.Compile("//*[not(contains(@class,'EmbedBrokenMedia')) and not(contains(@src,'youtube')) and not(contains(@class,'ytp'))]/div/video");
        private XPathExpression ImageInPostXPath => XPathExpression.Compile("//div[@class='bb-col col-site-main']//img[contains(@class,'alignnone')]");

        private XPathExpression MemeCategoryXPath => XPathExpression.Compile("//*[@class='alpha-nav-ul']//a");
        private XPathExpression MemeByCategoryXPath(string category) => XPathExpression.Compile($"//*[@class='alpha-block' and h4[text()='{category}']]//a");

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
            var categories = htmlDocument.DocumentNode.SelectNodes(MemeCategoryXPath).Select(c => c.InnerText).ToList();
            var randomCategory = categories.RandomElement();
            var memesInCategory = htmlDocument.DocumentNode.SelectNodes(MemeByCategoryXPath(randomCategory)).ToList();
            var randomMemeNode = memesInCategory.RandomElement();
            var memeTitle = randomMemeNode.InnerText;
            var memeUrl = randomMemeNode.GetAttributeValue("href", "");

            var allMemeA = htmlDocument.DocumentNode.SelectNodes(AllMemesXPath(memeTitle)).ToList();

            var memePageContent = await FetchContent(memeUrl);
            htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(memePageContent);

            var imagesHtlm = htmlDocument.DocumentNode.SelectNodes(ImageInPostXPath)?.Select(n => n.OuterHtml).ToList() ?? new List<string>();
            var videosHtlm = htmlDocument.DocumentNode.SelectNodes(VideoInPostXPath)?.Select(n => n.OuterHtml).ToList() ?? new List<string>();
            //var twitter = htmlDocument.DocumentNode.SelectNodes(TwitterEmbdedXPath).Select(n => n.OuterHtml);

            string randomMemeImageHtml = string.Empty;
            if (videosHtlm.Count > 0)
            {
                randomMemeImageHtml = videosHtlm.RandomElement();
                randomMemeImageHtml = Regex.Replace(randomMemeImageHtml, @"(<video )", "<video autoplay");
            }
            else
            {
                randomMemeImageHtml = imagesHtlm.RandomElement();
            }
            randomMemeImageHtml = CleanupImageHtml(randomMemeImageHtml);

            var allMemeImagesHtml = imagesHtlm.Concat(videosHtlm).ToList();
            var wrong1 = allMemeA.RandomElement();
            allMemeA.Remove(wrong1);
            var wrong2 = allMemeA.RandomElement();
            allMemeA.Remove(wrong2);
            var wrong3 = allMemeA.RandomElement();

            var shuffledAnswers = new List<string> { new string(wrong1.InnerText), new string(wrong2.InnerText), "*", new string(wrong3.InnerText) };
            shuffledAnswers.Shuffle();
            var correctIndex = shuffledAnswers.IndexOf("*");
            shuffledAnswers[correctIndex] = memeTitle;

            var triviaQuestion = new TriviaQuestion
            {
                Question = new string(randomMemeImageHtml),
                Correct = correctIndex + 1,
                Answers = new List<string>(shuffledAnswers)
            };


            mainPageContent = null;
            htmlDocument = null;
            categories = null;
            randomCategory = null;
            memesInCategory = null;
            randomMemeNode = null;
            memePageContent = null;
            allMemeA = null;
            imagesHtlm = null;
            videosHtlm = null;
            allMemeImagesHtml = null;
            shuffledAnswers = null;

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

        private string CleanupImageHtml(string imageHtml)
        {
            imageHtml = Regex.Replace(imageHtml, "(width=\".*?\")", string.Empty);
            imageHtml = Regex.Replace(imageHtml, "(height=\".*?\")", string.Empty);

            return imageHtml;
        }
    }
}