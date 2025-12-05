# User Document Processor

## Project Description
A User Management System where users can register via API and upload documents. The system handles background processing of documents, sends notifications, and performs nightly cleanup tasks.

## Features / Highlights
- User registration with document upload
- Background document processing with 30-second delay
- Welcome message and completion message notifications
- Nightly cleanup of stale and orphan files
- Automatic retry policy for all background jobs (2 retries: 5 min and 10 min)
- Health Checks: Liveness and Readiness
- Structured logging for jobs and cleanup
- **Hangfire Dashboard** for monitoring background jobs
- **Swagger / OpenAPI** documentation for API endpoints

## Tech Stack / Requirements
- **Language & Framework:** C#, .NET 9
- **Libraries:** Hangfire, MediatR, EF Core, FluentValidation, Swagger
- **OS:** Windows / Linux / macOS

## Getting Started / Setup
1. Clone the repository:
```bash
git clone https://github.com/<org>/<repo>.git
cd <repo>
```

2. Configure:
- Update `appsettings.json` connection strings

3. Build & Run:
```bash
dotnet build
dotnet run
```

4. Health Checks:
- Liveness: `/health/self`
- Readiness: `/health/ready`

5. **Hangfire Dashboard** available at `/hangfire`
6. **Swagger UI** available at `/swagger`

## API Endpoints
### Register User
**POST** `/api/users/register`
- **Input:** `name`, `email`, `document` (file)
- **Response:**
```json
{
  "userId": "<GUID>",
  "status": "Registered",
  "message": "User registered successfully."
}
```

## Background Jobs
1. **Welcome Message Job**: Runs immediately after registration
2. **Document Processing Job**: Runs 30 seconds after registration, converts document to PDF (simulation allowed)
3. **Completion Message Job**: Runs after document processing, sends completion notification
4. **Nightly Cleanup Job**: Runs daily at 00:00 to remove stale or orphan files
5. **Retry Policy**: Maximum 2 retries; Retry #1 = 5 min, Retry #2 = 10 min

## Testing
- Unit Tests & Integration Tests with xUnit
- Run tests:
```bash
dotnet test
```

## Folder Structure / Architecture
- **Api**: Web API controllers and request/response models
- **Application**: Commands, Handlers, DTOs, Interfaces, Services
- **Domain**: Entities, Enums, Repositories interfaces
- **Infrastructure**: EF Core persistence, file storage, background jobs, services
- **Tests**: Unit and integration tests

The project follows a **Clean Architecture** pattern.

## Database Migrations
### Using PMC:
```powershell
Add-Migration InitialCreate -Project UserDocumentProcessor.Infrastructure -StartupProject UserDocumentProcessor.API -OutputDir Persistence/Migrations
Update-Database -Project UserDocumentProcessor.Infrastructure -StartupProject UserDocumentProcessor.API
```

### Using CLI:
```bash
dotnet ef migrations add InitialCreate \
    --project UserDocumentProcessor.Infrastructure \
    --startup-project UserDocumentProcessor.API \
    --output-dir Persistence/Migrations

dotnet ef database update \
    --project UserDocumentProcessor.Infrastructure \
    --startup-project UserDocumentProcessor.API
```

## Notes / Future Improvements
- Add support for Email and SMS notifications

