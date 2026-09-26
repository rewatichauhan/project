# GitHub Gists API

A lightweight ASP.NET Core API that fetches public GitHub Gists for any GitHub user. Supports pagination and in-memory caching.

## Endpoint
##dummy update
```
GET /{username}?page={page}&perPage={perPage}
```

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `username` | Yes | — | Any GitHub username |
| `page` | No | 1 | Page number |
| `perPage` | No | 30 | Results per page (max 100) |

### Examples

```
# With Docker (port 8080)
GET http://localhost:8080/octocat?page=1&perPage=10
GET http://localhost:8080/torvalds?page=2&perPage=5

# Without Docker (port 5183)
GET http://localhost:5183/octocat?page=1&perPage=10
GET http://localhost:5183/torvalds?page=2&perPage=5
```

---

## Prerequisites

Choose the option that matches how you want to run the API:

| | Docker | Without Docker |
|---|---|---|
| Required | [Docker Desktop](https://www.docker.com/products/docker-desktop/) | [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) |

---

## Run without Docker

**1. Clone or download this repository**

```bash
git clone <repo-url>
cd api/apiproject
```

**2. Start the API**

```bash
dotnet run
```

The API starts on **http://localhost:5183** by default.

**Test it:**

```bash
curl "http://localhost:5183/octocat?page=1&perPage=5"
```

**PowerShell:**

```powershell
Invoke-RestMethod "http://localhost:5183/octocat?page=1&perPage=5"
```

**Run the tests:**

```bash
cd ../apiproject.Tests
dotnet test
```

---

## Run with Docker

### Option 1 — Pull and run a pre-built image

> Replace `your-registry/gists-api` with the actual image name if it has been pushed to a registry.

```bash
docker run -d --name gists-api -p 8080:8080 your-registry/gists-api
```

### Option 2 — Build and run from source

**1. Clone or download this repository**

```bash
git clone <repo-url>
cd api/apiproject
```

**2. Build the image**

```bash
docker build -t gists-api .
```

**3. Run the container**

```bash
docker run -d --name gists-api -p 8080:8080 gists-api
```

The API is now available at **http://localhost:8080**

---

## Test it

**Browser or curl:**

```bash
curl "http://localhost:8080/octocat?page=1&perPage=5"
```

**PowerShell:**

```powershell
Invoke-RestMethod "http://localhost:8080/octocat?page=1&perPage=5"
```

You should receive a JSON array of gist objects.

---

## Stop and remove the container

```bash
docker rm -f gists-api
```

---

## Project structure

```
api/
├── apiproject/
│   ├── Program.cs          # API logic — endpoint, pagination, caching
│   ├── apiproject.csproj   # Project file
│   ├── Dockerfile          # Multi-stage Docker build
│   └── .dockerignore       # Excludes bin/ and obj/ from the build context
└── apiproject.Tests/
    ├── GistsApiTests.cs    # NUnit integration tests
    └── apiproject.Tests.csproj
```

---

## How the Docker image is built

The Dockerfile uses a **two-stage build**:

1. **Build stage** — uses the full .NET SDK image to restore packages and publish the app
2. **Runtime stage** — uses the smaller ASP.NET runtime image (~220 MB vs ~600 MB) and copies only the published output

The container runs as a **non-root user** (`appuser`) for security. Port **8080** is used because ports below 1024 require root privileges in Linux.
