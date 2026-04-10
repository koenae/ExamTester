using ExamTester.Models;

namespace ExamTester.Services;

public class AnalyticsService
{
    private readonly PersistenceService _persistence;

    public AnalyticsService(PersistenceService persistence)
    {
        _persistence = persistence;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var profile = await _persistence.LoadProfileAsync();
        var history = profile.ExamHistory;

        return new DashboardStats
        {
            TotalExamsTaken = history.Count,
            TotalPassed = history.Count(a => a.IsPassed),
            TotalFailed = history.Count(a => !a.IsPassed),
            AverageScore = history.Count > 0 ? Math.Round(history.Average(a => a.ScorePercentage), 1) : 0,
            TotalStudyTimeMinutes = profile.TotalStudyTimeMinutes,
            StudyStreak = profile.StudyStreak,
            RecentAttempts = history.Take(5).ToList(),
            CertificationsAttempted = history.Select(a => a.ExamCode).Where(c => !string.IsNullOrEmpty(c)).Distinct().Count(),
            BestScore = history.Count > 0 ? history.Max(a => a.ScorePercentage) : 0,
            LastActiveDate = profile.LastActiveDate,
            PassRate = history.Count > 0 ? Math.Round((double)history.Count(a => a.IsPassed) / history.Count * 100, 1) : 0
        };
    }

    public async Task<CertificationAnalytics> GetCertificationAnalyticsAsync(string examCode)
    {
        var history = await _persistence.GetExamHistoryAsync(examCode, 100);

        var analytics = new CertificationAnalytics
        {
            ExamCode = examCode,
            TotalAttempts = history.Count,
            PassCount = history.Count(a => a.IsPassed),
            FailCount = history.Count(a => !a.IsPassed),
            AverageScore = history.Count > 0 ? Math.Round(history.Average(a => a.ScorePercentage), 1) : 0,
            BestScore = history.Count > 0 ? history.Max(a => a.ScorePercentage) : 0,
            WorstScore = history.Count > 0 ? history.Min(a => a.ScorePercentage) : 0,
            AverageTimeSeconds = history.Count > 0 ? (int)history.Average(a => a.TimeSpentSeconds) : 0,
            ScoreHistory = history.OrderBy(a => a.StartTime).Select(a => new ScorePoint
            {
                Date = a.StartTime,
                Score = a.ScorePercentage,
                IsPassed = a.IsPassed
            }).ToList()
        };

        var allDomainScores = new Dictionary<string, List<double>>();
        foreach (var attempt in history)
        {
            foreach (var ds in attempt.DomainScores)
            {
                if (!allDomainScores.ContainsKey(ds.Key))
                    allDomainScores[ds.Key] = new List<double>();
                allDomainScores[ds.Key].Add(ds.Value);
            }
        }

        analytics.DomainAverages = allDomainScores.ToDictionary(
            kvp => kvp.Key,
            kvp => Math.Round(kvp.Value.Average(), 1)
        );

        analytics.WeakDomains = analytics.DomainAverages
            .Where(d => d.Value < 70)
            .OrderBy(d => d.Value)
            .Select(d => d.Key)
            .ToList();

        analytics.StrongDomains = analytics.DomainAverages
            .Where(d => d.Value >= 80)
            .OrderByDescending(d => d.Value)
            .Select(d => d.Key)
            .ToList();

        return analytics;
    }

    public async Task<OverallAnalytics> GetOverallAnalyticsAsync()
    {
        var profile = await _persistence.LoadProfileAsync();
        var history = profile.ExamHistory;

        var vendorBreakdown = history
            .Where(a => !string.IsNullOrEmpty(a.Vendor))
            .GroupBy(a => a.Vendor)
            .ToDictionary(
                g => g.Key,
                g => new VendorStats
                {
                    Vendor = g.Key,
                    TotalAttempts = g.Count(),
                    PassCount = g.Count(a => a.IsPassed),
                    AverageScore = Math.Round(g.Average(a => a.ScorePercentage), 1),
                    Certifications = g.Select(a => a.ExamCode).Distinct().Count()
                }
            );

        var weeklyActivity = history
            .Where(a => a.StartTime >= DateTime.Now.AddDays(-28))
            .GroupBy(a => a.StartTime.Date.AddDays(-(int)a.StartTime.DayOfWeek))
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            );

        var monthlyScores = history
            .Where(a => a.StartTime >= DateTime.Now.AddMonths(-6))
            .GroupBy(a => new DateTime(a.StartTime.Year, a.StartTime.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new MonthlyScore
            {
                Month = g.Key,
                AverageScore = Math.Round(g.Average(a => a.ScorePercentage), 1),
                ExamCount = g.Count()
            })
            .ToList();

        return new OverallAnalytics
        {
            VendorBreakdown = vendorBreakdown,
            WeeklyActivity = weeklyActivity,
            MonthlyScores = monthlyScores,
            TotalStudyTimeMinutes = profile.TotalStudyTimeMinutes,
            StudyStreak = profile.StudyStreak,
            MostAttemptedExam = history
                .GroupBy(a => a.ExamTitle)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "None"
        };
    }
}

public class DashboardStats
{
    public int TotalExamsTaken { get; set; }
    public int TotalPassed { get; set; }
    public int TotalFailed { get; set; }
    public double AverageScore { get; set; }
    public int TotalStudyTimeMinutes { get; set; }
    public int StudyStreak { get; set; }
    public List<ExamAttempt> RecentAttempts { get; set; } = new();
    public int CertificationsAttempted { get; set; }
    public double BestScore { get; set; }
    public DateTime LastActiveDate { get; set; }
    public double PassRate { get; set; }
}

public class CertificationAnalytics
{
    public string ExamCode { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public int PassCount { get; set; }
    public int FailCount { get; set; }
    public double AverageScore { get; set; }
    public double BestScore { get; set; }
    public double WorstScore { get; set; }
    public int AverageTimeSeconds { get; set; }
    public List<ScorePoint> ScoreHistory { get; set; } = new();
    public Dictionary<string, double> DomainAverages { get; set; } = new();
    public List<string> WeakDomains { get; set; } = new();
    public List<string> StrongDomains { get; set; } = new();
}

public class OverallAnalytics
{
    public Dictionary<string, VendorStats> VendorBreakdown { get; set; } = new();
    public Dictionary<DateTime, int> WeeklyActivity { get; set; } = new();
    public List<MonthlyScore> MonthlyScores { get; set; } = new();
    public int TotalStudyTimeMinutes { get; set; }
    public int StudyStreak { get; set; }
    public string MostAttemptedExam { get; set; } = string.Empty;
}

public class ScorePoint
{
    public DateTime Date { get; set; }
    public double Score { get; set; }
    public bool IsPassed { get; set; }
}

public class VendorStats
{
    public string Vendor { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public int PassCount { get; set; }
    public double AverageScore { get; set; }
    public int Certifications { get; set; }
}

public class MonthlyScore
{
    public DateTime Month { get; set; }
    public double AverageScore { get; set; }
    public int ExamCount { get; set; }
}
