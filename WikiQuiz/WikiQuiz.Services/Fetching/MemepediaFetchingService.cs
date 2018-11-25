using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WikiQuiz.Models;

namespace WikiQuiz.Services.Fetching
{
    public class MemepediaFetchingService: IFetchingService
    {
        private const string MainPage = "https://memepedia.ru/memoteka/";

        public async Task<string> GetRandom()
        {
            var client = new RestClient(MainPage);
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteTaskAsync(request);
            return response.Content;
        }

        public Task<TriviaQuestion> GetRandomQuestion()
        {
            throw new NotImplementedException();
        }

        public TriviaQuestion Parse(string htmlContent)
        {
            throw new NotImplementedException();
        }
    }
}