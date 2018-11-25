using System.Threading.Tasks;
using WikiQuiz.Models;

namespace WikiQuiz.Services.Fetching
{
    public interface IFetchingService
    {
        Task<TriviaQuestion> GetRandomQuestion();
    }
}