namespace ExamTester.Models;

public class ExamResult
{
    public Exam Exam { get; set; } = new();
    public Dictionary<int, int?> UserAnswers { get; set; } = new();
    public HashSet<int> MarkedForReview { get; set; } = new();
    public int TimeSpentSeconds { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public int CorrectAnswers => Exam.Questions
        .Count(q => UserAnswers.TryGetValue(q.Id, out var answer) && answer == q.CorrectAnswer);

    public int TotalQuestions => Exam.Questions.Count;

    public int AnsweredQuestions => UserAnswers.Count(a => a.Value.HasValue);

    public double ScorePercentage => TotalQuestions > 0
        ? Math.Round((double)CorrectAnswers / TotalQuestions * 100, 1)
        : 0;

    public bool IsPassed => ScorePercentage >= Exam.PassingScore;

    public string TimeSpentFormatted
    {
        get
        {
            var span = TimeSpan.FromSeconds(TimeSpentSeconds);
            return span.Hours > 0
                ? $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}"
                : $"{span.Minutes:D2}:{span.Seconds:D2}";
        }
    }

    public bool IsQuestionCorrect(int questionId)
    {
        var question = Exam.Questions.FirstOrDefault(q => q.Id == questionId);
        if (question == null) return false;
        return UserAnswers.TryGetValue(questionId, out var answer) && answer == question.CorrectAnswer;
    }

    public int? GetUserAnswer(int questionId)
    {
        return UserAnswers.TryGetValue(questionId, out var answer) ? answer : null;
    }
}
