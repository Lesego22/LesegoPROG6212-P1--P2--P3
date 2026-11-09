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

---
 
# Part 3 - MVC Web Application
 
The RaceDay website. It is an ASP.NET Core MVC app that gets all its data from the Part 2 API.
It has no database code and no business rules of its own, and it never talks to Azure directly.
 
> Part 3 is in the **Part3-Mvc** branch. The website is in the **Part3-Web/RaceDay.Web** folder.
> The Part 2 API (in **Part2-API**) was extended with image upload and a few extra endpoints.
 
**Demo video (unlisted YouTube):** [https://youtu.be/A5VPolBRdLc]
 
## What it does
| Organiser | Participant |
|---|---|
| Dashboard: events created, enrolments per event, upcoming dates | Home page: upcoming events with filters (location, type, dates) |
| Create, edit and delete events (with banner image) | Event page with full details and categories |
| Add age or distance categories to each event | Enrol in an event and pick a category |
| See everyone enrolled and confirm or cancel places | My enrolments with colour-coded status |
| Enter finish times and positions | My results (for example "47th out of 312 finishers") |
| Profile page | Profile page with profile picture upload |
 
Enrolment statuses are colour coded: green = Confirmed, amber = Pending, red = Cancelled.
 
## How login works
The website calls the API to log in, then keeps the user's token, role and name in the **ASP.NET Core session**.
Every page checks the session. A visitor who is not logged in is sent to the login page, and a user with the wrong role
is sent to an "access denied" page. Logging out clears the session.
 
## Azure Blob Storage
Image uploads (event banners and profile pictures) go from the website to the **API**, and the API stores them in Azure Blob Storage.
The website shows images by asking the API for them, so it never talks to Azure.
 
The API reads its storage settings from `AzureBlob:ConnectionString` and `AzureBlob:ContainerName`.
- **Real Azure account:** put your connection string in user secrets (do not commit it):
  `dotnet user-secrets set "AzureBlob:ConnectionString" "<your connection string>"` (run inside `Part2-API/RaceDay.Api`).
- **Local testing without Azure:** the default `UseDevelopmentStorage=true` uses the free Azurite storage emulator
  (`npm install -g azurite` then `azurite --silent`).
 
## Run it locally (Visual Studio 2022)
1. Open `Part2-API/RaceDay.Api.sln`.
2. Update the database: Package Manager Console, default project `RaceDay.Api`, run `Update-Database`.
3. Start Azurite (or set your Azure connection string, see above).
4. Right-click the solution > **Configure Startup Projects** > **Multiple startup projects**. Set `RaceDay.Api` and `RaceDay.Web` to **Start**.
5. Press **F5**. The API runs at http://localhost:5080 and the website at http://localhost:5090.
6. To register as an Organiser, use the access code `RACEDAY-ORG-2026`.
 
## Run it in Docker
Make sure the API is running first (step 5 above), then:
 
```
docker build -t raceday-web ./Part3-Web/RaceDay.Web
docker run --rm -p 8080:8080 -e ApiBaseUrl=http://host.docker.internal:5080/ raceday-web
```
 
Open http://localhost:8080. The container behaves the same as the app running locally.
`host.docker.internal` is how the container reaches the API running on your computer. If the site says it cannot reach the service,
start the API with `dotnet run --urls http://0.0.0.0:5080` inside `Part2-API/RaceDay.Api` and try again.
 
## Project structure (Part3-Web/RaceDay.Web)
- `Controllers` - one controller per area (Account, Events, Enrolments, Results, Profile, Manage for the organiser, Images)
- `Services` - `ApiClient` (all API calls), session helpers and the role-check attribute
- `Models` - API response classes, form classes and view models
- `Views` - Razor pages for each controller plus the shared layout
- `wwwroot/css/site.css` - the RaceDay theme
- `Dockerfile` - two-stage build (SDK to build, small runtime image to run)
 
## CI/CD
`.github/workflows/part2-ci.yml` restores and builds the API and the MVC project, runs the unit tests,
and builds the Docker image on every push.
 
![Green build](<img width="1920" height="778" alt="Screenshot 2026-11-09 185152" src="https://github.com/user-attachments/assets/aa8470a4-0297-41b7-b14f-ca2d37ae0f60" />

)


