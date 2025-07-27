# Travel Management API (Database First)

This RESTful API is built using ASP.NET Core and Entity Framework Core (Database First approach). It is designed to manage client trips, countries, and travel reservations based on the database schema provided.

## Features
- Retrieve list of trips ordered by start date
- Pagination support using `page` and `pageSize` query parameters
- Relational data: Clients, Trips, Countries (many-to-many via `Country_Trip`)
- View clients and their assigned trips
- Display only future trips for registration

## Database Schema
This project uses the following tables:
- `Client`
- `Trip`
- `Country`
- `Client_Trip` (join table)
- `Country_Trip` (join table)

All entities are generated using EF Core Database First approach (`Scaffold-DbContext`).

## Tech Stack
- C# (.NET 8)
- ASP.NET Core Web API
- Entity Framework Core (Database First)
- SQL Server

## API Endpoints
| Method | Endpoint       | Description                                 |
|--------|----------------|---------------------------------------------|
| GET    | `/api/trips`   | Get list of trips (sorted, paginated)       |
| GET    | `/api/clients` | Get clients and their trip registrations    |
| POST   | `/api/clients/{id}/trips` | Assign client to a trip         |

## Getting Started
1. Clone this repo
2. Update your `appsettings.json` with your connection string
3. Run the project: `dotnet run`
4. Open Swagger UI: `https://localhost:{port}/swagger`

## Notes
- Default `pageSize` is set to 10
- Only upcoming trips are shown for assignment
- Validation for PESEL, dates, and availability is applied

## Future Enhancements
- Authentication and user roles
- Country-based filtering
- Trip cancellation logic
