using System;
using System.Threading.Tasks;
using QuizGame.Models;

namespace QuizGame.Game
{
    // Simple AI: answers correctly based on difficulty and delays by 1-3 seconds.
    public class AIPlayer
    {
        Random rng = new Random();

        public async Task<int> ChooseAnswerAsync(QuestionData q)
        {
            int delayMs = rng.Next(1000, 3000);
            await Task.Delay(delayMs);
            // chance of correct decreases with difficulty
            float p = q.difficulty switch { 1 => 0.8f, 2 => 0.6f, 3 => 0.4f, _ => 0.5f };
            if (rng.NextDouble() < p) return q.correctIndex;
            // random wrong
            int a;
            do { a = rng.Next(0, q.options.Length); } while (a == q.correctIndex);
            return a;
        }
    }
}
