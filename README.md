# LogisticsFlow

LogisticsFlow is a backend application for managing logistics orders and their lifecycle. The solution is built with .NET and follows a layered architecture designed to keep business rules, application logic, infrastructure concerns, and HTTP endpoints clearly separated.

The project currently supports order creation and retrieval, including the items associated with each order, and is structured to evolve toward asynchronous processing, observability, cloud deployment, and distributed architecture patterns.

## Architecture

The solution is organized into four main projects:

```text
LogisticsFlow.Api
        |
        v
LogisticsFlow.Application
        |
        v
LogisticsFlow.Domain

LogisticsFlow.Infrastructure
        |
        v
LogisticsFlow.Domain
```

### Domain

Contains the core business model and abstractions.

Responsibilities include:

- domain entities;
- order lifecycle rules;
- enums and domain types;
- repository contracts.

Main entities:

```text
Order
└── OrderItems
```

`Order` acts as the aggregate root and is responsible for maintaining the consistency of its items and lifecycle.

### Application

Contains the application use cases and contracts used by the API.

The Application layer coordinates the execution of business operations without depending directly on infrastructure implementations.

Example flow:

```text
CreateOrderRequest
        |
        v
CreateOrderUsecase
        |
        v
OrderEntity
        |
        v
IOrdersRepository
```

### Infrastructure

Contains persistence and external infrastructure implementations.

Current responsibilities include:

- Entity Framework Core;
- SQL Server persistence;
- repository implementations;
- entity configurations;
- database migrations.

### Api

Exposes the application through ASP.NET Core Minimal APIs.

Endpoint definitions are separated from the application bootstrap to keep `Program.cs` focused on dependency registration and middleware configuration.

## Technology Stack

- .NET 10
- ASP.NET Core
- Minimal APIs
- Entity Framework Core
- SQL Server 2022
- Docker
- Docker Compose

## Domain Model

An order contains:

- customer identifier;
- destination;
- current status;
- creation date;
- dispatch date;
- one or more items.

Example:

```json
{
  "customerId": 123,
  "destination": "Rio de Janeiro",
  "items": [
    {
      "sku": "NOTEBOOK-001",
      "quantity": 2
    },
    {
      "sku": "MOUSE-001",
      "quantity": 5
    }
  ]
}
```

The order lifecycle is modeled through explicit status transitions.

```text
Created
   |
   v
Dispatched
   |
   v
Completed
```

Orders may also be cancelled according to the rules defined by the domain.

## Persistence

SQL Server runs in a Docker container and uses a named volume for data persistence.

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: logistics-sqlserver
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD}
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

volumes:
  sqlserver_data:
```

Sensitive values are not stored directly in versioned configuration files.

Docker-specific environment variables are loaded from a local `.env` file, while the API connection string can be configured using .NET User Secrets during local development.

## Entity Framework Core

Database schema changes are managed through Entity Framework Core migrations.

The main relationship is:

```text
Orders
   |
   | 1:N
   v
OrderItems
```

Entity identifiers use SQL Server `uniqueidentifier` columns generated with:

```sql
NEWSEQUENTIALID()
```

This allows GUID-based identifiers while reducing the index fragmentation associated with fully random GUID generation.

### Create a migration

From the solution root:

```bash
dotnet ef migrations add MigrationName \
  --project src/LogisticsFlow.Infrastructure \
  --startup-project src/LogisticsFlow.Api
```

### Apply migrations

```bash
dotnet ef database update \
  --project src/LogisticsFlow.Infrastructure \
  --startup-project src/LogisticsFlow.Api
```

## Running Locally

### Requirements

Make sure the following tools are installed:

- .NET 10 SDK
- Docker
- Docker Compose
- Entity Framework Core CLI

Install the EF Core CLI if necessary:

```bash
dotnet tool install --global dotnet-ef
```

### Configure SQL Server

Create a `.env` file in the repository root:

```env
MSSQL_SA_PASSWORD=your-strong-password
MSSQL_DATABASE=LogisticsFlow
```

The `.env` file is ignored by Git and must not be committed.

Start SQL Server:

```bash
docker compose up -d
```

Check the container status:

```bash
docker compose ps
```

### Configure the API connection string

Navigate to the API project:

```bash
cd src/LogisticsFlow.Api
```

Configure the connection string using .NET User Secrets:

```bash
dotnet user-secrets set \
  "ConnectionStrings:LogisticsFlowDbStringConnection" \
  "Server=localhost,1433;Database=LogisticsFlow;User Id=sa;Password=your-strong-password;TrustServerCertificate=True"
```

Configured secrets can be inspected with:

```bash
dotnet user-secrets list
```

### Apply database migrations

From the solution root:

```bash
dotnet ef database update \
  --project src/LogisticsFlow.Infrastructure \
  --startup-project src/LogisticsFlow.Api
```

### Start the API

```bash
cd src/LogisticsFlow.Api

dotnet watch run
```

## API

### Create an order

```http
POST /orders
```

Request:

```json
{
  "customerId": 123,
  "destination": "Rio de Janeiro",
  "items": [
    {
      "sku": "NOTEBOOK-001",
      "quantity": 2
    }
  ]
}
```

The order and all associated items are persisted as a single aggregate.

A successful request returns `201 Created` with the generated order identifier.

### Retrieve orders

The API supports retrieving orders together with their associated items.

Additional query and lifecycle endpoints are being added as the domain evolves.

## Planned Evolution

The next application capabilities include:

```text
GET  /orders
GET  /orders/{id}

POST /orders/{id}/dispatch
POST /orders/{id}/complete
POST /orders/{id}/cancel
```

The architecture is also prepared to evolve with:

- domain validation and lifecycle rules;
- standardized error responses with Problem Details;
- unit tests;
- integration tests;
- Testcontainers;
- API containerization;
- Redis caching;
- asynchronous processing with AWS SQS;
- .NET background workers;
- retry and circuit breaker policies;
- Dead Letter Queues;
- idempotent message processing;
- Transactional Outbox;
- OpenTelemetry;
- centralized logs, metrics, and distributed tracing;
- GitHub Actions CI/CD;
- AWS ECR;
- AWS ECS/Fargate;
- Infrastructure as Code with Terraform;
- load testing and horizontal scaling.

## Design Principles

LogisticsFlow is designed around a few core principles:

- business rules remain independent from infrastructure concerns;
- application use cases depend on abstractions rather than concrete implementations;
- persistence details are isolated in the Infrastructure layer;
- HTTP contracts are kept separate from domain entities;
- order items are managed as part of the `Order` aggregate;
- sensitive configuration is kept outside source control;
- architectural complexity is introduced only when required by the application's behavior.

The goal is to keep the codebase maintainable while allowing the system to evolve toward a scalable and observable cloud-native architecture.
