using System.Text.Json;
using ExamTester.Models;

namespace ExamTester.Services;

public class ExamService
{
    private Exam? _currentExam;
    private ExamResult? _currentResult;
    private int _currentQuestionIndex;

    public Exam? CurrentExam => _currentExam;
    public ExamResult? CurrentResult => _currentResult;
    public int CurrentQuestionIndex => _currentQuestionIndex;
    public Question? CurrentQuestion => _currentExam?.Questions.ElementAtOrDefault(_currentQuestionIndex);
    public bool IsExamActive => _currentExam != null && _currentResult != null;
    public bool IsFirstQuestion => _currentQuestionIndex == 0;
    public bool IsLastQuestion => _currentExam != null && _currentQuestionIndex == _currentExam.Questions.Count - 1;

    public event Action? OnExamStateChanged;

    public async Task<Exam?> LoadExamFromJsonAsync(string jsonContent)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _currentExam = JsonSerializer.Deserialize<Exam>(jsonContent, options);
            return _currentExam;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public void StartExam()
    {
        if (_currentExam == null) return;

        _currentResult = new ExamResult
        {
            Exam = _currentExam,
            StartTime = DateTime.Now,
            UserAnswers = new Dictionary<int, int?>()
        };

        foreach (var question in _currentExam.Questions)
        {
            _currentResult.UserAnswers[question.Id] = null;
        }

        _currentQuestionIndex = 0;
        NotifyStateChanged();
    }

    public void NavigateToQuestion(int index)
    {
        if (_currentExam == null) return;
        if (index < 0 || index >= _currentExam.Questions.Count) return;

        _currentQuestionIndex = index;
        NotifyStateChanged();
    }

    public void NextQuestion()
    {
        if (_currentExam == null) return;
        if (_currentQuestionIndex < _currentExam.Questions.Count - 1)
        {
            _currentQuestionIndex++;
            NotifyStateChanged();
        }
    }

    public void PreviousQuestion()
    {
        if (_currentQuestionIndex > 0)
        {
            _currentQuestionIndex--;
            NotifyStateChanged();
        }
    }

    public void SetAnswer(int questionId, int answerIndex)
    {
        if (_currentResult == null) return;
        _currentResult.UserAnswers[questionId] = answerIndex;
        NotifyStateChanged();
    }

    public void ClearAnswer(int questionId)
    {
        if (_currentResult == null) return;
        _currentResult.UserAnswers[questionId] = null;
        NotifyStateChanged();
    }

    public int? GetAnswer(int questionId)
    {
        if (_currentResult == null) return null;
        return _currentResult.UserAnswers.TryGetValue(questionId, out var answer) ? answer : null;
    }

    public void ToggleMarkForReview(int questionId)
    {
        if (_currentResult == null) return;

        if (_currentResult.MarkedForReview.Contains(questionId))
        {
            _currentResult.MarkedForReview.Remove(questionId);
        }
        else
        {
            _currentResult.MarkedForReview.Add(questionId);
        }
        NotifyStateChanged();
    }

    public bool IsMarkedForReview(int questionId)
    {
        return _currentResult?.MarkedForReview.Contains(questionId) ?? false;
    }

    public bool IsQuestionAnswered(int questionId)
    {
        return _currentResult?.UserAnswers.TryGetValue(questionId, out var answer) == true && answer.HasValue;
    }

    public ExamResult FinishExam(int timeSpentSeconds)
    {
        if (_currentResult == null)
        {
            throw new InvalidOperationException("No exam in progress");
        }

        _currentResult.EndTime = DateTime.Now;
        _currentResult.TimeSpentSeconds = timeSpentSeconds;

        var result = _currentResult;
        return result;
    }

    public void ResetExam()
    {
        _currentExam = null;
        _currentResult = null;
        _currentQuestionIndex = 0;
        NotifyStateChanged();
    }

    public QuestionStatus GetQuestionStatus(int questionId)
    {
        if (_currentResult == null) return QuestionStatus.NotAnswered;

        var isAnswered = IsQuestionAnswered(questionId);
        var isMarked = IsMarkedForReview(questionId);

        if (isMarked && isAnswered) return QuestionStatus.AnsweredAndMarked;
        if (isMarked) return QuestionStatus.MarkedForReview;
        if (isAnswered) return QuestionStatus.Answered;
        return QuestionStatus.NotAnswered;
    }

    private void NotifyStateChanged() => OnExamStateChanged?.Invoke();
}

public enum QuestionStatus
{
    NotAnswered,
    Answered,
    MarkedForReview,
    AnsweredAndMarked
}
