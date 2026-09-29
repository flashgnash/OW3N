# OW3N.Tests

Automated test suite for OW3N / Ordis (xUnit, targeting .NET 9).

## Running

```bash
# Via the flake (recommended — provides the pinned SDK, node and sass):
nix run .#test

# Directly with the .NET SDK on your PATH:
dotnet test OW3N.Tests/OW3N.Tests.csproj -p:SkipWebAssets=true
```

`SkipWebAssets=true` skips the app's npm/sass web-asset build — the tests only exercise
managed code (services + models), so they don't need the compiled CSS. In sandboxed
environments where native app-host generation is unavailable, also pass `-p:UseAppHost=false`
(the `nix run .#test` wrapper does this for you).

CI runs the suite on every pull request and before the build/publish job on `master`
(see `.github/workflows/dotnet.yml`).

## Layout

- **`Unit/`** — fast, dependency-free tests for pure logic:
  - `BuffFormulaTests` — parsing, evaluation and stacked application of buff effect formulae.
  - `BuffSerializationTests` — the `BuffConverter` (current + legacy JSON formats).
  - `StatBlockTests` — `StatBlock` / `StatListConverter` JSON mapping.
  - `ModelSerializationTests`, `PlayerCharacterModelTests` — JSON-backed model properties,
    saved-roll parsing and `IValidatableObject` validation.
  - `DiscordServiceTests` — webhook payloads (via a stubbed `HttpClient`).
  - `MiscUnitTests` — `Gauge` lookup and `LiveUpdateService` events.
- **`Integration/`** — service tests against a real EF Core database (in-memory SQLite):
  - `CampaignServiceTests`, `BuffServiceTests`, `PlayerCharacterServiceTests`, `SchemaSmokeTests`.
- **`TestSupport/`** — shared helpers: `SqliteContextFactory` (an in-memory
  `IDbContextFactory<OrdisContext>`), `StubHttpMessageHandler`, and `Seed` data helpers.

## Why SQLite (in-memory) rather than PostgreSQL?

Production runs on **PostgreSQL** (Npgsql). The integration tests deliberately run against an
**in-memory SQLite** database instead. This is a considered trade-off, made explicit here so the
choice is not mistaken for an oversight.

**Reasons for SQLite in-memory:**

- **Isolation.** Each test constructs its own `SqliteContextFactory`, and therefore its own
  private database that lives only for the duration of that test. Tests cannot see each other's
  rows, cannot collide on primary keys, and are order-independent — no shared server to reset
  between runs.
- **Hermetic, zero-setup CI and local runs.** No PostgreSQL server, Docker container, connection
  string or credentials are required. `nix run .#test` and the CI `test` job need nothing beyond
  the .NET SDK, which keeps the feedback loop simple and reliable.
- **Speed.** An in-memory database plus `Database.EnsureCreated()` builds the schema from the EF
  model in milliseconds per test, so the whole suite runs in seconds.
- **Same EF Core code paths.** The app accesses the database *exclusively* through EF Core and the
  `IDbContextFactory<OrdisContext>` abstraction (see `Services/`). Swapping the provider underneath
  the context still exercises the same LINQ translation, entity mapping, relationship/cascade and
  change-tracking behaviour that production relies on — which is the bulk of the data-access logic
  worth testing.

**The trade-off (fidelity gaps), stated honestly:**

SQLite is not PostgreSQL, so provider-specific behaviour is *not* faithfully exercised:

- **Provider-specific SQL.** The clearest case in this codebase is
  `BuffService.SearchTemplatesAsync`, whose name filter uses `EF.Functions.ILike` — a
  PostgreSQL-only function with no SQLite translation. The tests therefore only cover its
  owner/campaign scoping (they call it with an empty query so the `ILike` branch is skipped); the
  case-insensitive `%query%` matching itself is **not** verified here. This is flagged with a
  `NOTE:` in `Integration/BuffServiceTests.cs`.
- **Schema source.** The test schema comes from `EnsureCreated()` against the current EF model,
  **not** from the Npgsql migrations under `Migrations/`. So these tests validate the model, not the
  migration history (which additionally contains Postgres-specific column types).
- **Type/collation nuances.** Case-insensitive string comparisons, `decimal`/`float` precision,
  and `DateTime` handling can differ subtly between the two providers.

**Conclusion.** For a suite whose job is fast, isolated, hermetic verification in CI, in-memory
SQLite is the right default, and the app's exclusive use of the EF Core abstraction means most
data-access logic is exercised faithfully. Where PostgreSQL-specific behaviour genuinely matters
(currently just `ILike` search), the gap is called out explicitly rather than hidden. If that
behaviour ever needs a guarantee, it should be covered by a separate, opt-in **PostgreSQL-backed**
run — e.g. via [Testcontainers](https://dotnet.testcontainers.org/) or the disposable Postgres
stack already provided by `nix run .#test-serve` — layered *alongside* (not replacing) the fast
SQLite suite.

## Notes / observations surfaced while writing tests

Some tests characterise current behaviour that looks unintended. They are marked with `NOTE:`
comments so they can be revisited without silently locking in a bug:

- `PlayerCharacterService.GetLatestRollAsync` orders by `Timestamp` **ascending** and takes the
  first, so it returns the *earliest* roll despite its name.
- `BuffService.SearchTemplatesAsync` name filtering uses `EF.Functions.ILike` (PostgreSQL-only),
  so only the owner/campaign scoping is covered by the SQLite-backed tests (see above).
