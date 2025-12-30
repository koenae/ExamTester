# Exam Simulator

A Blazor web application for simulating certification exams. Import your own questions via JSON and practice with a realistic exam experience.

## Features

- **JSON Import** - Load custom exam questions from JSON files
- **Countdown Timer** - Timed exams with visual warnings
- **Question Navigation** - Jump between questions, track progress
- **Mark for Review** - Flag questions to revisit before submitting
- **Results & Analytics** - Detailed score breakdown with explanations

## Requirements

- .NET 10 SDK

## Running the Application

```bash
dotnet run
```

Then open http://localhost:5200 in your browser.

## JSON Format

Create an exam file with this structure:

```json
{
  "examTitle": "My Practice Exam",
  "timeLimit": 60,
  "passingScore": 70,
  "questions": [
    {
      "id": 1,
      "text": "What is 2 + 2?",
      "options": ["3", "4", "5", "6"],
      "correctAnswer": 1,
      "explanation": "2 + 2 equals 4"
    }
  ]
}
```

| Field | Description |
|-------|-------------|
| `examTitle` | Name of the exam |
| `timeLimit` | Duration in minutes |
| `passingScore` | Percentage required to pass |
| `correctAnswer` | Zero-based index of correct option |
| `explanation` | Shown in results review (optional) |

A sample exam file is included at `wwwroot/sample-exam.json`.

## License

MIT
