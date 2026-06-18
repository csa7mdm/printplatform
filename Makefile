.PHONY: dev test migrate seed logs clean build

dev:
	docker compose up --build

build:
	dotnet build -c Release

test:
	dotnet test -c Release --logger "console;verbosity=normal"

migrate:
	docker compose exec api dotnet ef database update --project src/PrintPlatform.Infrastructure --startup-project src/PrintPlatform.API

seed:
	docker compose exec api dotnet run --project src/PrintPlatform.API -- --seed

logs:
	docker compose logs -f api

clean:
	docker compose down -v
	find . -type d -name bin -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name obj -exec rm -rf {} + 2>/dev/null || true
