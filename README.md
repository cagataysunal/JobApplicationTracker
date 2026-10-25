# job-application-tracker

This is a WIP job application tracker built as a showcase project for my portfolio AND real use.

## Stack

- **API:** ASP.NET Core 10 minimal APIs, EF Core 10, SQL Server
- **UI:** Blazor WebAssembly (standalone)
- **Tests:** xUnit, WebApplicationFactory, Testcontainers

## Setting up

1. Copy `.env.example` as `.env` and set your own SQL Server password.
   Use the same password in `JobTracker.Api/appsettings.Development.json`
   (or better, `dotnet user-secrets`).
2. Start the database:

   ```bash
   docker compose up -d
   ```

   > **Apple Silicon:** SQL Server images are x64 only and do not start under QEMU.
   > Run them through Rosetta with at least 4 GB of VM memory:
   >
   > ```bash
   > softwareupdate --install-rosetta --agree-to-license
   > colima start --vz-rosetta --memory 4
   > ```
   >
   > With Docker Desktop instead, enable **Settings → General → "Use Rosetta for
   > x86_64/amd64 emulation on Apple Silicon"**.

3. Create the schema:

   ```bash
   dotnet tool restore
   dotnet ef database update -p JobTracker.Api
   ```

4. Run the API (http://localhost:5250) and the UI (http://localhost:5260):

   ```bash
   dotnet run --project JobTracker.Api --launch-profile http
   dotnet run --project JobTracker.Web --launch-profile http
   ```

## Tests

Docker must be running. Integration tests start a throwaway SQL Server container.

```bash
dotnet test
```
