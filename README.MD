# User Task Management API

A backend Web API build with ASP.NET Core, EF Core and SQL Server.

The project provides user authentication, role-base authorization, project management and task management with ownership-base access control.

# Index

[Tech Stack](#tech-stack)

[Main Features](#main-features)

[Architecture](#architecture)

[Database Design](#database-design)

[API Endpoints](#api-endpoints)

[Authentication Flow](#authentication-flow)

[Authorization Rules](#authorization-rules)

[How to Run Locally](#how-to-run-on-local)

[Demo Accounts](#demo-accounts)


## Tech Stack
- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- Refresh Token
- Swagger

## Main Features
- User registration and login 
- JWT access token authentication
- Refresh token rotation
- Logout 
- Role-base authorization: Admin/User
- Project CRUD
- Task CRUD inside projects
- Ownership-based authorization
- Soft delete
- Global query filters
- Global exception handling
- Standardized API response format
- Validation using DTOs and Data Annotaions
- Basic logging

## Architecture
The project follows a simple layered architecture

- Controllers handle HTTP request and response.
- Services contain business logic and authorization checks.
- DTOs are used to separate API contracts from database entities.
- EF Core DbContext handles database access.
- Middleware handles global exception responses.

## Database Design
Database is created by using and mapping models (ORM - EF Core) to generate entity at DbContext. After that using migrations feature to create database

Users
- Id
- Name
- Email
- Description
- Age
- PasswordHash
- Role
- RefreshToken
- RefreshTokenExpiresAt
- CreatedAt
- UpdatedAt
- Deleted

Projects
- Id
- Name
- Description
- OwnerId
- CreatedAt
- UpdatedAt
- Deleted

TaskItems
- Id
- Title
- Description
- Status
- DueDate
- ProjectId
- CreatedAt
- UpdatedAt
- Deleted

Relationship:

Users 1 ─── n Projects

Projects 1 ─── n TaskItems

## API Endpoints


### Auth

| Method | Endpoint | Description |
|---|---|---|
| POST | /api/Auth/register | Register new user |
| POST | /api/Auth/login | Login and get tokens |
| POST | /api/Auth/refresh-token | Refresh access token |
| POST | /api/Auth/logout | Logout current user |
| GET | /api/Auth/me | Get current user |

### Users

| Method | Endpoint | Role |
|---|---|---|
| GET | /api/User | Admin |
| GET | /api/User/{id} | Admin or owner |
| POST | /api/User | Admin |
| PUT | /api/User/{id} | Admin or owner |
| PATCH | /api/User/{id}/role | Admin |
| DELETE | /api/User/{id} | Admin |
| PATCH | /api/User/{id}/restore | Admin |
| GET | /api/User/deleted | Admin |

### Projects

| Method | Endpoint | Role |
|---|---|---|
| GET | /api/Projects | Authenticated |
| GET | /api/Projects/{id} | Owner or Admin |
| POST | /api/Projects | Authenticated |
| PUT | /api/Projects/{id} | Owner or Admin |
| DELETE | /api/Projects/{id} | Owner or Admin |

### Tasks

| Method | Endpoint | Role |
|---|---|---|
| GET | /api/projects/{projectId}/tasks | Project owner or Admin |
| GET | /api/projects/{projectId}/tasks/{taskId} | Project owner or Admin |
| POST | /api/projects/{projectId}/tasks | Project owner or Admin |
| PUT | /api/projects/{projectId}/tasks/{taskId} | Project owner or Admin |
| DELETE | /api/projects/{projectId}/tasks/{taskId} | Project owner or Admin |

## Authentication Flow

The API uses JWT-based authentication with access tokens and refresh tokens.

1. A user registers a new account through `POST /api/auth/register`.
2. A user logs in through `POST /api/auth/login` using email and password.
3. If the credentials are valid, the server returns an access token and a refresh token.
4. The client sends the access token in the `Authorization` header (or sends the access token in swagger UI: Authorize button) when calling protected APIs.

```http
Authorization: Bearer <access_token>
```

5. Protected endpoints such as users, projects and tasks require a valid access token.
6. When the access token expires, the client can send the refresh token to `POST /api/auth/refresh-token` to receive a new access token and refresh token.
7. When the user logs out through `POST /api/auth/logout`, the refresh token is removed from the database and can no longer be used.
8. The `GET /api/auth/me` endpoint returns the current authenticated user based on the user ID stored in the JWT token and the latest user data from the database.

Access tokens are used for authentication and authorization when calling protected APIs. Refresh tokens are only used to request new tokens and are not used to access protected resources directly.

## Authorization Rules

### User

- Can view and update their own profile.
- Cannot view the full user list.
- Cannot access projects owned by other users.
- Cannot access tasks inside projects owned by other users.

### Admin

- Can view and manage all users.
- Can update user roles.
- Can view and manage all projects.
- Can view and manage all tasks.

## How to run on local
### 1. Clone repository
Select folder in your local and open command line in this folder. After that, follow the below command:

```bash
git clone https://github.com/DTPGH/UserApi.git
cd UserApi
```

### 2. Configure connection string
Update connection strings at file `appsettings.jon` by following on your SQL Server local

```
"ConnectionStrings": {
  "DefaultConnection": "Server=<your-server-name>;Database=<your-db-name>;User Id=sa;Password=<your-sa-pass>;TrustServerCertificate=True;"
}
```

Note:
- `<your-server-name>` is named sql server on your local
- `<your-db-name>` can name it whatever you like 
- `<your-sa-pass>` is password login in your SQL Server


### 3. Apply migrations
```
dotnet ef database update
```

Note: when you apply migrations successfully, the database has only 3 samples of users, so you should create more data for schema dbo.Users, dbo.Projects ,dbo.TaskItems
through APIs

But if your want role is Admin, you should edit table data on field `Role` in your database. Change by "Admin" to use as admin access

### 4. Run the API
```
dotnet run
```

### 5. Open Swagger on your browser
```
https://localhost:<port>/swagger
```

## Demo Accounts
You can follow the flow test:

```text
1. Register user A
2. Login user A
3. View user A information detail
4. Create/View/Update/Delete project of user A
5. Create task inside project of A and View/Update/Delete
---
6. Register user B
7. Login user B
8. User B cannot access project A
---
9. Register user Admin
10. Into database,change the field role: 'Admin' manually 
11. Login admin
12. Admin can access all projects/tasks
13. Admin updates user role
14. Logout / refresh token flow
```

## Access to FE-Blazor
You can get UI of this backend project with frontend project through this link: https://github.com/DTPGH/UserApi-Blazor-fe
