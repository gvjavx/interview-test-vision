# C# assignment

Two small ASP.NET Core (.NET 10) apps that run on Linux:

- `RestApi` - CRUD API for todos on port 5000
- `WebSocketApp` - chat room over WebSocket on port 5001, with a test page at `/`

Both keep their data in memory, so a restart clears it.

## Run with Docker

Needs Docker (on WSL: Docker Desktop with WSL integration turned on for the distro).

```sh
docker compose up -d --build
```

The containers run as a non-root user and restart unless stopped. Stop them with
`docker compose down`.

## Run without Docker

Needs the .NET 10 SDK (`sudo apt-get install dotnet-sdk-10.0` on Ubuntu 24.04).

```sh
cd RestApi && dotnet run --urls http://0.0.0.0:5000
cd WebSocketApp && dotnet run --urls http://0.0.0.0:5001
```

## Endpoints

RestApi:

| Method | Path | Result |
| --- | --- | --- |
| GET | `/health` | 200 when the app is up |
| GET | `/api/todos` | all todos |
| GET | `/api/todos/{id}` | one todo, 404 if missing |
| POST | `/api/todos` | create, body `{"title": "...", "isDone": false}`; 400 if the title is blank or over 200 characters |
| PUT | `/api/todos/{id}` | replace, same body as POST |
| DELETE | `/api/todos/{id}` | 204, or 404 if missing |

WebSocketApp:

- `GET /health` - 200 when the app is up
- `/ws` - WebSocket endpoint. Every text message is sent to all connected clients,
  including the sender. Messages over 4096 bytes close the connection.
- `/` - chat page; open it in two tabs to see messages go both ways

## Smoke tests

With both apps running:

```sh
./RestApi/smoke-test.sh          # http://localhost:5000 by default
./WebSocketApp/smoke-test.py     # localhost:5001 by default
```

Each prints one line per check and exits non-zero if any check fails.
