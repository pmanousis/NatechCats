# Cat API Service

This is an ASP.NET Core Web API project that fetches cat images from the Cats as a Service (CaaS) API and stores them in a Microsoft SQL Server database using Entity Framework Core.

## Prerequisites

Before you begin, ensure you have the following installed:

* [.NET 8 SDK](https://dotnet.microsoft.com/download)
* [Docker](https://www.docker.com/get-started/)
* [JetBrains Rider](https://www.jetbrains.com/rider/) (or any other compatible IDE)

## Setting up the Database with Docker

1.  **Navigate to the Project Directory:**
    * Open your terminal or command prompt and navigate to the directory containing the `compose.yaml` file.

2.  **Start the SQL Server Container:**
    * Run the following command:
        ```docker compose up -d```
    * This will start a SQL Server container using the configuration defined in `compose.yaml`.

3.  **Verify Container Status:**
    * Run `docker ps` to ensure the sql server container is running.

## Building and Running the Application

1.  **Open the Project in JetBrains Rider:**
    * Open the project in JetBrains Rider.
    * Install the following nugets:
      * Microsoft.EntityFrameworkCore.SqlServer
      * Microsoft.EntityFrameworkCore.Tools
      * Swashbuckle.AspNetCore
      * Newtonsoft.Json

2.  **Database Migration:**
    * Open the terminal within Rider.
    * Run the following Entity Framework Core migration commands:
      * ```dotnet ef migrations add InitialCreate``` and
      * ```dotnet ef database update```
    * This will create the database and tables in your SQL Server instance.

3.  **Run the Application:**
    * Run the ASP.NET Core Web API project from Rider.

4.  **Access Swagger UI:**
    * Open your web browser and navigate to `https://localhost:<port>/swagger` (replace `<port>` with the port number shown in the Rider console).

## API Endpoints

* **POST /api/cats/fetch:**
    * Fetches at most 25 cats that have breeds from https://api.thecatapi.com/
* **GET /api/cats/{id}:**
    * Gets a specific cat with a given id from our DB
* **GET /api/cats:**
    * Gets a number of cats, paginated from our DB. You may apply a criterion (having a specific tag) or not.

## Configuration

* **appsettings.json:**
    * Contains the database connection string. Make sure to update the password if you changed the default from compose.yaml.

## Unit tests
You need to add the following nugets:
* xunit
* Microsoft.EntityFrameworkCore.InMemory
* Moq
* Microsoft.NET.Test.Sdk

The file containing the tests is under Tests folder and tests the CatsController code.