# E-Learning Platform API

Full-stack backend for an E-Learning Platform built with **ASP.NET Core 8**, **Entity Framework Core**, and **SQL Server**.

## Architecture

```
ELearning.Core           → Entities, DTOs, Interfaces
ELearning.Infrastructure → DbContext, Repository, Services
ELearning.API            → Controllers, AutoMapper, DI
ELearning.Tests          → xUnit tests (InMemory DB)
SQL/                     → Raw SQL scripts (DDL, DML, Queries)
```

## Features

- **6 Tables**: Users, Courses, Lessons, Quizzes, Questions, Results
- **Repository Pattern** + **Service Layer**
- **AutoMapper** for DTO mapping (no entity exposure)
- **Password Hashing** (PBKDF2 + salt)
- **Fluent API** relationships with eager loading
- **AsNoTracking** for read performance
- **Input Validation** via Data Annotations
- **Proper HTTP status codes** (200, 201, 400, 404)
- **10 Unit Tests** covering CRUD, scoring, LINQ, exceptions

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/users/register | Register user |
| GET | /api/users/{id} | Get user |
| PUT | /api/users/{id} | Update user |
| GET | /api/courses | List courses |
| GET | /api/courses/{id} | Course detail (with lessons & quizzes) |
| POST | /api/courses | Create course |
| PUT | /api/courses/{id} | Update course |
| DELETE | /api/courses/{id} | Delete course |
| GET | /api/courses/{courseId}/lessons | Lessons by course |
| POST | /api/lessons | Create lesson |
| PUT | /api/lessons/{id} | Update lesson |
| DELETE | /api/lessons/{id} | Delete lesson |
| GET | /api/quizzes/{courseId} | Quizzes by course |
| POST | /api/quizzes | Create quiz |
| GET | /api/quizzes/{quizId}/questions | Quiz questions |
| POST | /api/questions | Add question |
| POST | /api/quizzes/{quizId}/submit | Submit quiz |
| GET | /api/results/{userId} | User results |

## Setup

1. Update connection string in `ELearning.API/appsettings.json`
2. Run migrations or execute `SQL/queries.sql` against SQL Server
3. `dotnet run --project ELearning.API`

## Tests

```bash
dotnet test
```
