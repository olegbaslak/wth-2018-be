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
            Any,
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
            Trivia trivia = new Trivia();

            if (_source == Source.Wiki) fetcher = new WikiFetchingService();
            else if (_source == Source.Meme) fetcher = new MemepediaFetchingService();

            else if (_source == Source.Any)
            {
                var wikiFetcher = new WikiFetchingService();
                var memeFether = new MemepediaFetchingService();

                trivia = new Trivia();

                for (int i = 0; i < count; i += 2)
                {
                    GC.AddMemoryPressure(300000);
                    var question = await memeFether.GetRandomQuestion();
                    trivia.Add(question);
                    GC.RemoveMemoryPressure(300000);
                    GC.Collect();
                }
                for (int i = 1; i < count; i += 2)
                {
                    var question = await wikiFetcher.GetRandomQuestion();
                    trivia.Add(question);
                }

                return trivia;
            }

            else
            {
                throw new NotSupportedException($"Source {_source} is not supported");
            }

            for (int i = 0; i < count; i++)
            {
                var question = await fetcher.GetRandomQuestion();
                trivia.Add(question);
            }

            GC.Collect();
            return trivia;
        }
    }
}