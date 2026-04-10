using System.Text.Json;
using ExamTester.Models;

namespace ExamTester.Services;

public class PersistenceService
{
    private readonly string _dataDir;
    private readonly string _profilePath;
    private readonly string _customExamsDir;
    private readonly JsonSerializerOptions _jsonOptions;
    private UserProfile? _cachedProfile;

    public PersistenceService()
    {
        _dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ExamTester");
        _profilePath = Path.Combine(_dataDir, "profile.json");
        _customExamsDir = Path.Combine(_dataDir, "custom-exams");
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        EnsureDirectories();
    }

    private void EnsureDirectories()
    {
        Directory.CreateDirectory(_dataDir);
        Directory.CreateDirectory(_customExamsDir);
    }

    public async Task<UserProfile> LoadProfileAsync()
    {
        if (_cachedProfile != null) return _cachedProfile;

        if (File.Exists(_profilePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_profilePath);
                _cachedProfile = JsonSerializer.Deserialize<UserProfile>(json, _jsonOptions) ?? new UserProfile();
            }
            catch
            {
                _cachedProfile = new UserProfile();
            }
        }
        else
        {
            _cachedProfile = new UserProfile();
        }
        return _cachedProfile;
    }

    public async Task SaveProfileAsync(UserProfile profile)
    {
        _cachedProfile = profile;
        var json = JsonSerializer.Serialize(profile, _jsonOptions);
        await File.WriteAllTextAsync(_profilePath, json);
    }

    public async Task SaveLlmConfigAsync(LlmConfig config)
    {
        var profile = await LoadProfileAsync();
        profile.LlmConfig = config;
        await SaveProfileAsync(profile);
    }

    public async Task<LlmConfig> LoadLlmConfigAsync()
    {
        var profile = await LoadProfileAsync();
        return profile.LlmConfig;
    }

    public async Task SaveExamAttemptAsync(ExamAttempt attempt)
    {
        var profile = await LoadProfileAsync();
        profile.ExamHistory.Insert(0, attempt);

        if (profile.ExamHistory.Count > 500)
        {
            profile.ExamHistory = profile.ExamHistory.Take(500).ToList();
        }

        profile.LastActiveDate = DateTime.Now;
        profile.TotalStudyTimeMinutes += attempt.TimeSpentSeconds / 60;
        UpdateStudyStreak(profile);

        await SaveProfileAsync(profile);
    }

    public async Task<List<ExamAttempt>> GetExamHistoryAsync(string? examCode = null, int limit = 50)
    {
        var profile = await LoadProfileAsync();
        var history = profile.ExamHistory.AsEnumerable();

        if (!string.IsNullOrEmpty(examCode))
        {
            history = history.Where(a => a.ExamCode == examCode);
        }

        return history.Take(limit).ToList();
    }

    public async Task SaveCustomExamAsync(string fileName, string examJson)
    {
        var filePath = Path.Combine(_customExamsDir, fileName);
        await File.WriteAllTextAsync(filePath, examJson);

        var profile = await LoadProfileAsync();
        if (!profile.CustomExamFiles.Contains(fileName))
        {
            profile.CustomExamFiles.Add(fileName);
            await SaveProfileAsync(profile);
        }
    }

    public async Task<List<Exam>> GetCustomExamsAsync()
    {
        var exams = new List<Exam>();
        if (!Directory.Exists(_customExamsDir)) return exams;

        foreach (var file in Directory.GetFiles(_customExamsDir, "*.json"))
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var exam = JsonSerializer.Deserialize<Exam>(json, _jsonOptions);
                if (exam != null) exams.Add(exam);
            }
            catch
            {
                // Skip invalid files
            }
        }
        return exams;
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        var profile = await LoadProfileAsync();
        profile.Settings = settings;
        await SaveProfileAsync(profile);
    }

    public async Task<AppSettings> LoadSettingsAsync()
    {
        var profile = await LoadProfileAsync();
        return profile.Settings;
    }

    public async Task ClearHistoryAsync()
    {
        var profile = await LoadProfileAsync();
        profile.ExamHistory.Clear();
        await SaveProfileAsync(profile);
    }

    public async Task DeleteAttemptAsync(string attemptId)
    {
        var profile = await LoadProfileAsync();
        profile.ExamHistory.RemoveAll(a => a.Id == attemptId);
        await SaveProfileAsync(profile);
    }

    public async Task ToggleFavoriteExamAsync(string examId)
    {
        var profile = await LoadProfileAsync();
        if (!profile.FavoriteExams.Remove(examId))
        {
            profile.FavoriteExams.Add(examId);
        }
        await SaveProfileAsync(profile);
    }

    public async Task ExportDataAsync(string filePath)
    {
        var profile = await LoadProfileAsync();
        var json = JsonSerializer.Serialize(profile, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    private void UpdateStudyStreak(UserProfile profile)
    {
        var today = DateTime.Today;
        var lastActive = profile.LastActiveDate.Date;

        if (lastActive == today)
        {
            return;
        }
        else if (lastActive == today.AddDays(-1))
        {
            profile.StudyStreak++;
        }
        else
        {
            profile.StudyStreak = 1;
        }
    }
}
