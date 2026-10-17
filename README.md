
# RaceDay Management System

**Unlisted YouTube Video:** https://youtu.be/9kiV4AAdrtE

## Project Overview

RaceDay is a race event management system developed using C#, ASP.NET Core Web API, Entity Framework Core, and Microsoft SQL Server.

The system allows Organisers to create and manage race events, while Participants can browse events, enroll in available categories, and view their race results.

This repository contains the following stages of the Programming Portfolio of Evidence (POE):

- **Part 1 — System Planning and Database Design**
- **Part 2 — REST API Development, Testing and CI/CD**

The project follows a database-driven approach and implements authentication, role-based authorization, CRUD operations, and automated testing.

---

# Part 1 — Planning and Database Design

## Overview

Part 1 focuses on the design and planning of the RaceDay system before implementation.

The following components were completed:

- Entity Relationship Diagram (ERD)
- API Endpoint Plan
- SQL Server Database Script
- Database sample data

The supporting documentation is available in the `Part 1/docs` folder.

## System Roles

The system contains two main user roles.

### Organiser

Organisers are responsible for:

- Creating events
- Updating events
- Deleting events
- Creating and managing event categories
- Viewing participant enrollments
- Recording and updating participant results

### Participant

Participants are able to:

- Register and log in
- Browse available events
- View event details
- Select event categories
- Enroll in events
- View their enrollments
- View their race results

Users are distinguished by their assigned roles in the database.

## Database Design

The RaceDay database consists of six main entities:

1. User
2. UserRole
3. Event
4. Category
5. Enrollment
6. Result

### Main Relationships

- One role can be assigned to many users.
- One Organiser can organise many events.
- One event can contain many categories.
- One Participant can have many enrollments.
- One event can have many enrollments.
- One category can be selected by many Participants.
- An enrollment can have a result.

Primary keys and foreign keys are used to maintain referential integrity between the tables.

## API Endpoint Plan

The original API endpoint plan covers:

- Authentication
- User profiles
- Events
- Categories
- Enrollments
- Results

The final implemented endpoints are documented in the Part 2 section below.

## Database Tables

### User

Stores registered user information, password hashes, and assigned roles.

### UserRole

Stores the two supported system roles: Organiser and Participant.

### Event

Stores event information, including name, description, date, location, distance, event type, and Organiser.

### Category

Stores categories associated with events, including category names and age groups.

### Enrollment

Records Participants enrolled in events and their selected categories.

### Result

Stores Participants' finishing times and finishing positions.

## Sample Data

The Part 1 SQL scripts contain sample data for:

- 2 Organisers
- 2 Participants
- 3 Events
- 6 Event Categories
- 5 Enrollments
- Sample race results

This sample data demonstrates database relationships and foreign key functionality.

**Note:** The Part 1 sample data is not automatically inserted into the Part 2 database when migrations are applied.

## Part 1 CI/CD Workflow

Part 1 uses a GitHub Actions workflow named:

`Part 1 - Repository Structure Validation`

The workflow checks that the repository contains:

- The `Part 1/docs` directory
- An ERD file
- An API endpoint plan
- A SQL script
- A root README.md file

### Part 1 CI/CD Evidence

![Part 1 GitHub Actions Validation](https://github.com/user-attachments/assets/26b5c837-3bce-4198-955d-9902b926cee6)

---

# Part 2 — RaceDay REST API

## Overview

Part 2 implements the RaceDay backend using ASP.NET Core Web API and Entity Framework Core.

The API provides:

- User registration and authentication
- Role-based authorization
- User profile management
- Event CRUD operations
- Category management
- Participant enrollments
- Race result management
- Database persistence
- Swagger API documentation
- Automated unit and integration testing
- Continuous Integration using GitHub Actions

The project targets **.NET 8**.

## Technologies Used

| Technology | Purpose |
|---|---|
| C# | Application programming language |
| .NET 8 | Application framework |
| ASP.NET Core Web API | REST API development |
| Entity Framework Core | Object-relational mapping |
| SQL Server | Database management |
| BCrypt.Net | Password hashing and verification |
| Cookie Authentication | Authentication session management |
| Swagger / OpenAPI | API documentation and testing |
| xUnit | Automated testing |
| EF Core InMemory | Temporary test database |
| Microsoft.AspNetCore.Mvc.Testing | HTTP integration testing |
| GitHub Actions | Continuous Integration |
| Visual Studio 2022 | Development environment |

## Project Structure

The Visual Studio solution contains two projects.

### RaceDayApp_POE

The main ASP.NET Core Web API project.

```text
RaceDayApp_POE/
├── Controllers/
│   ├── AuthController.cs
│   ├── EventsController.cs
│   ├── UsersController.cs
│   ├── CategoriesController.cs
│   ├── EnrollmentsController.cs
│   └── ResultsController.cs
├── Data/
├── DTOs/
├── Models/
├── Migrations/
├── Program.cs
└── RaceDayApp_POE.csproj
```

### appTesting

The automated testing project.

```text
appTesting/
├── AuthControllerTests.cs
├── EventsControllerTests.cs
├── EnrollmentsControllerTests.cs
├── AuthorizationIntegrationTests.cs
└── appTesting.csproj
```

The test project references the main API project.

---

## Database Configuration

RaceDay uses Entity Framework Core Code First with SQL Server.

### Database Setup

1. Open the Visual Studio solution.
2. Confirm that SQL Server or SQL Server LocalDB is installed.
3. Open the API project's `appsettings.json`.
4. Configure the database connection string.
5. Open the Package Manager Console.
6. Select `RaceDayApp_POE` as the default project.
7. Execute the database migration command.

```powershell
Update-Database
```

This applies the existing EF Core migrations and creates or updates the required database schema.

### Automatic Role Seeding

The API automatically checks for the required user roles when the application starts.

If a role is missing, the application inserts it into the database.

The supported roles are:

- Organiser
- Participant

Existing roles remain unchanged.

The seeding process requires the database schema to have been created through migrations before the API starts.

---

## Authentication and Authorization

The API uses ASP.NET Core Cookie Authentication.

### Registration

Users register using:

- First name
- Surname
- Email address
- Password
- Role type

The user selects either `Organiser` or `Participant`.

### Password Security

Passwords are hashed using BCrypt before being stored in the database.

Original passwords are not stored or returned through the API.

### Login

Users authenticate using their registered email address and password.

Successful authentication creates a session using a secure authentication cookie.

### Authorization

The API uses role-based authorization to restrict access to protected endpoints.

For example:

- Only Organisers can create events.
- Only Organisers can manage categories for events they own.
- Only Participants can enroll in events.
- Organisers can manage results for events they own.
- Users can access their own profile information.

The API also verifies event ownership to prevent Organisers from modifying events belonging to other Organisers.

---

## Implemented API Endpoints

The RaceDay REST API contains 22 implemented HTTP operations.

### Authentication

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Authenticate a registered user |
| POST | `/api/auth/logout` | End the authenticated session |
| GET | `/api/auth/me` | Retrieve current authentication details |

### User Profiles

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/users/me` | Retrieve the logged-in user's profile |
| PUT | `/api/users/me` | Update the logged-in user's profile |

### Events

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/events` | Retrieve all race events |
| GET | `/api/events/{eventID}` | Retrieve a specific event |
| POST | `/api/events` | Create a new event |
| PUT | `/api/events/{eventID}` | Update an owned event |
| DELETE | `/api/events/{eventID}` | Delete an owned event |

### Categories

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/events/{eventID}/categories` | Retrieve event categories |
| POST | `/api/events/{eventID}/categories` | Create an event category |
| PUT | `/api/events/{eventID}/categories/{categoryID}` | Update a category |
| DELETE | `/api/events/{eventID}/categories/{categoryID}` | Delete a category |

### Enrollments

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/enrollments` | Enroll in an event |
| GET | `/api/enrollments/mine` | Retrieve own enrollments |
| GET | `/api/events/{eventID}/enrollments` | View event enrollments |

### Results

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/enrollments/{enrollmentID}/result` | Record a race result |
| PUT | `/api/enrollments/{enrollmentID}/result` | Update a race result |
| GET | `/api/results/mine` | Retrieve own race results |
| GET | `/api/events/{eventID}/results` | Retrieve event results |

---

## Swagger API Documentation

Swagger UI provides interactive documentation for the API.

Each endpoint includes descriptions explaining its purpose.

### Running Swagger

1. Set `RaceDayApp_POE` as the startup project.
2. Run the application using the HTTPS development profile.
3. Open the Swagger URL shown by Visual Studio.

Example:

```text
https://localhost:<port>/swagger
```

Swagger allows developers to submit requests, inspect responses, and test the implemented API endpoints.

---

## Automated Testing

The RaceDay solution contains a separate xUnit testing project named `appTesting`.

Tests are designed to verify application functionality and expected behavior.

Unit tests use Entity Framework Core InMemory instead of the development SQL Server database.

Integration tests use an ASP.NET Core test server to verify authorization behavior through HTTP requests.

### Authentication Tests

| Test | Expected Behavior |
|---|---|
| Valid registration | User is saved successfully |
| Duplicate email | Registration is rejected |
| Incorrect password | Unauthorized response |
| Correct credentials | Authentication succeeds |

**Total: 4 tests**

### Event Management Tests

| Test | Expected Behavior |
|---|---|
| Organiser creates an event | Event is successfully created |
| Different Organiser updates an event | Access is forbidden |
| Event with existing categories is deleted | Deletion is rejected |

**Total: 3 tests**

### Enrollment Tests

| Test | Expected Behavior |
|---|---|
| Valid enrollment | Enrollment is saved and retrievable |
| Duplicate enrollment | Duplicate registration is rejected |
| Enrollment in a past event | Request is rejected |

**Total: 3 tests**

### Authorization Integration Tests

| Test | Expected HTTP Response |
|---|---|
| Organiser creates an event | 201 Created |
| Participant attempts to create an event | 403 Forbidden |
| Unauthenticated user attempts to create an event | 401 Unauthorized |

**Total: 3 tests**

### Test Results

| Test Category | Passed |
|---|---:|
| Authentication | 4 |
| Event management | 3 |
| Enrollments | 3 |
| Authorization integration | 3 |
| **Total** | **13 / 13** |

All 13 tests passed successfully in Visual Studio Test Explorer.

### Running the Tests

Tests can be executed using Visual Studio:

1. Open the solution.
2. Navigate to **Test → Test Explorer**.
3. Select **Run All Tests**.
4. Review the results.

Alternatively, use:

```bash
dotnet test
```

Run this command from the solution directory.

---

## Continuous Integration and Continuous Delivery

GitHub Actions is used to automate repository validation, application builds, and testing.

Two independent workflows are maintained.

### Part 1 — Repository Structure Validation

Validates the availability of required planning documentation and SQL files.

### Part 2 — RaceDay API CI

The Part 2 workflow performs the following operations:

1. Checks out the repository.
2. Installs the .NET 8 SDK.
3. Locates the API and testing projects.
4. Restores NuGet dependencies.
5. Builds the API and test project in Release mode.
6. Runs the automated tests.
7. Uploads the test results as an artifact.

The workflow runs automatically on:

- Pushes to `main` or `master`
- Pull requests targeting `main` or `master`

This ensures that changes are automatically built and tested before they are accepted.

### Part 2 CI/CD Evidence

**Status:** Awaiting successful GitHub Actions workflow verification.

<!-- Replace this placeholder with the screenshot after the workflow passes. -->

![Part 2 GitHub Actions Results](https://github.com/Baand1le/Prog-2B-POE/blob/main/Part%202/Part%202%20CI%20CD%20workflow.PNG)

---

## Running the Application

To run RaceDay on another computer:

1. Clone this repository.
2. Open the Visual Studio solution.
3. Install or restore the required NuGet packages.
4. Configure the SQL Server connection string.
5. Run `Update-Database` to apply migrations.
6. Set `RaceDayApp_POE` as the startup project.
7. Run the application using HTTPS.
8. Open Swagger UI.
9. Register a new Organiser or Participant account.
10. Log in and test the available endpoints.

Default roles are seeded automatically when the API starts, provided the database tables already exist.

No existing development database needs to be copied to run the API.

---

## Video Demonstration

The Part 2 demonstration video will show:

- Running the RaceDay API
- Swagger endpoint documentation
- User registration and login
- Role-based authorization
- Event management
- Participant enrollments
- Race results
- SQL Server database persistence
- Automated test execution
- GitHub Actions CI results

**Unlisted YouTube Video:** https://youtu.be/9kiV4AAdrtE

---

## Repository

GitHub Repository:

https://github.com/Baand1le/Prog-2B-POE

---

## Conclusion

RaceDay Part 2 delivers a REST API for managing race events, user accounts, event categories, participant enrollments, and race results.

The implementation demonstrates ASP.NET Core Web API development, Entity Framework Core database integration, authentication, authorization, automated testing, and Continuous Integration.

The API provides the backend foundation for the next stage of the RaceDay system.
