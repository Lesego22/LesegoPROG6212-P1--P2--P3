# RaceDay Event Management System

## Project Description
RaceDay is a comprehensive event management system designed for South African road running, walking, and cycling events. The system allows organisers to create and manage events, define categories, capture results, and track participant enrollments.

## Database Design
The system uses a relational database with 7 entities:
- **User** - Stores organiser and participant information
- **UserRole** - Reference table for user roles (Organiser, Participant)
- **Event** - Race events with date, location, distance
- **Category** - Race types and age groups
- **EventCategory** - Links events to categories (many-to-many)
- **Enrolment** - Participant registrations
- **Result** - Race results and finish times

## System Roles

### Organiser
- Create and manage events
- Define race categories
- Record race results
- View all enrollments

### Participant
- Browse available events
- Enroll in events
- Track personal results
- View performance history

## API Endpoints
The system includes 17 RESTful API endpoints covering:
- Authentication (register, login)
- User profiles
- Event management
- Category management
- Enrollments
- Results and leaderboards

## Files in /docs
- `RaceDay_ERD.png` - Entity Relationship Diagram
- `API_ENDPOINT_PLAN.md` - Complete API documentation
- `CREATE DATABASE RaceDay.sql` - Database creation script
- `CREATE TABLES.sql` - Table creation script
- `INSERT SAMPLE DATA.sql` - Sample data insertion
- `VERIFY DATA.sql` - Data verification queries
- `PROG6212 RaceDay`- the Api endpoints and the screenshots for section c

## GitHub Repository
All planning documents, SQL scripts, and ERD are available in the `/docs` folder.

## CI/CD Workflow Screenshot: 
<img width="998" height="322" alt="image" src="https://github.com/user-attachments/assets/0d647017-b64c-47f5-ad4b-306414a2f432" />

---

# Part 2 - Web API

RESTful ASP.NET Core Web API for RaceDay. **Organisers** create and manage events and capture results. **Participants** enrol in events and view their results.

> Part 2 is in the **Part2-Api** branch, in the **Part2-API** folder.

**Demo video (unlisted YouTube):** https://youtu.be/xMwFNx8FwxI

## Tech
- ASP.NET Core Web API (.NET 8, C#)
- Entity Framework Core 8, Code-First, SQL Server
- JWT authentication with role-based authorisation
- BCrypt password hashing
- Swagger (Swashbuckle)
- xUnit unit tests
- GitHub Actions CI

## The two roles
| | Organiser | Participant |
|---|---|---|
| Register and log in | Yes (needs an access code) | Yes |
| View and update own profile | Yes | Yes |
| View events and categories | Yes | Yes |
| Create, update, delete events | Yes (own events) | No (403) |
| Create categories | Yes | No (403) |
| View all enrolments for an event | Yes (own events) | No (403) |
| Enrol in an event | No (403) | Yes |
| View own enrolments | - | Yes |
| Record finish time and position | Yes (own events) | No (403) |
| View own results | - | Yes |
| View event leaderboard | Yes | Yes |

**How registration works:** anyone can register as a Participant. Registering as an Organiser also needs an `organiserAccessCode` (set in `appsettings.json`, default `RACEDAY-ORG-2026`), so users cannot make themselves Organisers.

**How the session works:** logging in returns a signed JWT with the user's id and role. It is sent as `Authorization: Bearer <token>` on every request. No or invalid token gives 401. The wrong role gives 403.

## Setup and run (Visual Studio 2022)
1. Install Visual Studio 2022 with the **ASP.NET and web development** workload (includes SQL Server LocalDB) and the .NET 8 SDK.
2. Clone the repo, switch to the `Part2-Api` branch, and open `Part2-API/RaceDay.Api.sln`.
3. Check the connection string in `RaceDay.Api/appsettings.json` (default: LocalDB, database `RaceDayApi`).
4. Open **Tools > NuGet Package Manager > Package Manager Console**, set *Default project* to `RaceDay.Api`, and run:
```
   Update-Database
```
   This creates the database from the migration already in the project.
5. Set `RaceDay.Api` as the startup project, choose the **http** profile and press **F5**. Swagger opens at `http://localhost:5080/swagger`.

### Try it in Swagger
1. `POST /api/auth/register` with `roleId`:
   - Participant: `22222222-2222-2222-2222-222222222222`
   - Organiser: `11111111-1111-1111-1111-111111111111` (plus `organiserAccessCode`)
2. Copy the `token`, click **Authorize**, paste it and confirm.
3. Call any endpoint. Each one shows its purpose, expected data and responses.

## Endpoints
| Method | Route | Role |
|---|---|---|
| POST | /api/auth/register | Public |
| POST | /api/auth/login | Public |
| GET, PUT | /api/users/{userId} | Self |
| GET | /api/events | Any logged in |
| POST | /api/events | Organiser |
| GET | /api/events/{eventId} | Any logged in |
| PUT, DELETE | /api/events/{eventId} | Organiser (owner) |
| GET | /api/categories | Any logged in |
| POST | /api/categories | Organiser |
| POST | /api/enrollments | Participant |
| GET | /api/enrollments/user/{userId} | Self |
| GET | /api/events/{eventId}/enrollments | Organiser (owner) |
| POST | /api/results | Organiser (owner) |
| GET | /api/results/user/{userId} | Self |
| GET | /api/events/{eventId}/results | Any logged in |
| POST | /api/events/{eventId}/categories | Organiser (owner) |

## Changes from the Part 1 plan
- `EventType` (Run, Walk, Cycle) was added to Event because the Part 2 brief requires it.
- An extra endpoint, `POST /api/events/{eventId}/categories`, links a category to an event.
- `organiserAccessCode` was added to registration (see above).

## Unit tests
In Visual Studio use **Test > Run All Tests**, or run `dotnet test` inside `Part2-API`. The tests cover registration and login, event management by Organisers, role rejection (401 and 403), enrolments, and results.

## CI/CD
`.github/workflows/part2-ci.yml` restores, builds and runs all tests on every push.

![Green build](docs/ci-green-build.png)

---
## Video Presentation
The video presentation is demonstrated on YouTube explaining the endpoints, Erd and SQL scripts for raceday.
YouTube Link: https://youtu.be/xMwFNx8FwxI

