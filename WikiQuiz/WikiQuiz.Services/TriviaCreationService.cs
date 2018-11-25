using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WikiQuiz.Models;
using WikiQuiz.Services.Fetching;

namespace WikiQuiz.Services
{
    public class TriviaCreationService
    {
        public Source _source { get; private set; }

        public enum Source
        {
            Wiki,
            Meme
        }

        public TriviaCreationService(Source source)
        {
            _source = source;
        }

        public async Task<Trivia> Create(int count = 1)
        {
            IFetchingService fetcher;

            if (_source == Source.Wiki) fetcher = new WikiFetchingService();
            else if (_source == Source.Meme) fetcher = new MemepediaFetchingService();
            else throw new NotSupportedException($"Source {_source} is not supported");

            Trivia trivia = new Trivia();

            for (int i = 0; i < count; i++)
            {
                var question = await fetcher.GetRandomQuestion();
                trivia.Add(question);
            }

            return trivia;

            /*
            if (_source == Source.Wiki)
            {
                var fetcher = new WikiFetchingService();

                Trivia trivia = new Trivia();

                for (int i = 0; i < count; i++)
                {
                    var question = await fetcher.GetRandomQuestion();
                    trivia.Add(question);
                }

                return trivia;
            }
            else if (_source == Source.Meme)
            {
                var fetcher = new MemepediaFetchingService();
            }

            throw new NotSupportedException($"Source {_source} is not supported.");*/
        }
    }
}