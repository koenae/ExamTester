namespace ExamTester.Models;

public class UserAnswer
{
    public int? SelectedOption { get; set; }
    public List<int>? SelectedOptions { get; set; }
    public List<int>? OrderedItems { get; set; }
    public string? TextAnswer { get; set; }
    public Dictionary<int, int>? MatchedPairs { get; set; }

    public bool HasAnswer => SelectedOption.HasValue
        || (SelectedOptions != null && SelectedOptions.Count > 0)
        || (OrderedItems != null && OrderedItems.Count > 0)
        || !string.IsNullOrWhiteSpace(TextAnswer)
        || (MatchedPairs != null && MatchedPairs.Count > 0);
}

public class ExamResult
{
    public Exam Exam { get; set; } = new();
    public Dictionary<int, UserAnswer> UserAnswers { get; set; } = new();
    public HashSet<int> MarkedForReview { get; set; } = new();
    public int TimeSpentSeconds { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ExamMode Mode { get; set; } = ExamMode.Exam;

    public int CorrectAnswers => Exam.Questions.Count(q => IsQuestionCorrect(q.Id));

    public int TotalQuestions => Exam.Questions.Count;

    public int AnsweredQuestions => UserAnswers.Count(a => a.Value.HasAnswer);

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
        if (!UserAnswers.TryGetValue(questionId, out var answer)) return false;

        return question.Type switch
        {
            QuestionType.SingleChoice or QuestionType.YesNo =>
                answer.SelectedOption == question.CorrectAnswer,

            QuestionType.MultipleChoice =>
                answer.SelectedOptions != null
                && question.CorrectAnswers != null
                && answer.SelectedOptions.OrderBy(x => x).SequenceEqual(question.CorrectAnswers.OrderBy(x => x)),

            QuestionType.DragAndDrop =>
                answer.OrderedItems != null
                && question.CorrectOrder != null
                && answer.OrderedItems.SequenceEqual(question.CorrectOrder),

            QuestionType.FillInTheBlank =>
                !string.IsNullOrWhiteSpace(answer.TextAnswer)
                && !string.IsNullOrWhiteSpace(question.CorrectText)
                && answer.TextAnswer.Trim().Equals(question.CorrectText.Trim(), StringComparison.OrdinalIgnoreCase),

            QuestionType.Matching =>
                answer.MatchedPairs != null
                && question.MatchPairs != null
                && answer.MatchedPairs.Count == question.MatchPairs.Count
                && answer.MatchedPairs.All(kvp => kvp.Value == kvp.Key),

            QuestionType.CaseStudy =>
                question.SubQuestions != null
                && question.SubQuestions.All(sq => IsSubQuestionCorrect(sq, answer)),

            _ => false
        };
    }

    private bool IsSubQuestionCorrect(Question subQuestion, UserAnswer parentAnswer)
    {
        return parentAnswer.SelectedOption == subQuestion.CorrectAnswer;
    }

    public int? GetUserAnswer(int questionId)
    {
        if (!UserAnswers.TryGetValue(questionId, out var answer)) return null;
        return answer.SelectedOption;
    }

    public UserAnswer? GetUserAnswerFull(int questionId)
    {
        return UserAnswers.TryGetValue(questionId, out var answer) ? answer : null;
    }

    public Dictionary<string, DomainScore> GetDomainBreakdown()
    {
        var domains = new Dictionary<string, DomainScore>();
        foreach (var question in Exam.Questions)
        {
            var domain = question.Domain ?? "General";
            if (!domains.ContainsKey(domain))
            {
                domains[domain] = new DomainScore { Domain = domain };
            }

            domains[domain].TotalQuestions++;
            if (IsQuestionCorrect(question.Id))
            {
                domains[domain].CorrectAnswers++;
            }
        }
        return domains;
    }
}

public class DomainScore
{
    public string Domain { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public double Percentage => TotalQuestions > 0 ? Math.Round((double)CorrectAnswers / TotalQuestions * 100, 1) : 0;
}

public enum ExamMode
{
    Exam,
    Practice,
    Study
}
