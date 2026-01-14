# ══════════════════════════════════════════════════════════════
# SwiftApp ERP — Makefile
# ══════════════════════════════════════════════════════════════
# Usage:  make help
# ══════════════════════════════════════════════════════════════

.DEFAULT_GOAL := help
SHELL := /bin/bash

# ── Project paths ─────────────────────────────────────────────
SOLUTION      := SwiftApp.ERP.slnx
WEBAPI        := src/SwiftApp.ERP.WebApi
WEBAPP        := src/SwiftApp.ERP.WebApp
SHARED_KERNEL := src/SwiftApp.ERP.SharedKernel

# ── Docker Compose files ─────────────────────────────────────
DC            := docker compose
DC_PROD       := docker compose -f compose.prod.yaml --env-file .env.prod

# ── Colors ────────────────────────────────────────────────────
CYAN  := \033[36m
GREEN := \033[32m
YELLOW:= \033[33m
RED   := \033[31m
RESET := \033[0m
BOLD  := \033[1m

# ══════════════════════════════════════════════════════════════
#  HELP
# ══════════════════════════════════════════════════════════════

.PHONY: help
help: ## Show this help
	@printf "\n$(BOLD)$(CYAN)SwiftApp ERP$(RESET) — Swiss Watch Manufacturing & Retail ERP\n\n"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | \
		awk 'BEGIN {FS = ":.*?## "}; {printf "  $(CYAN)%-22s$(RESET) %s\n", $$1, $$2}'
	@printf "\n"

# ══════════════════════════════════════════════════════════════
#  BUILD & RESTORE
# ══════════════════════════════════════════════════════════════

.PHONY: restore build clean publish

restore: ## Restore NuGet packages
	dotnet restore $(SOLUTION)

build: ## Build the entire solution
	dotnet build $(SOLUTION)

clean: ## Clean all build artifacts
	dotnet clean $(SOLUTION)
	find . -type d \( -name bin -o -name obj \) -exec rm -rf {} + 2>/dev/null || true

publish-api: ## Publish WebApi for production (Release)
	dotnet publish $(WEBAPI) -c Release -o ./publish/webapi

publish-app: ## Publish WebApp for production (Release)
	dotnet publish $(WEBAPP) -c Release -o ./publish/webapp

publish: publish-api publish-app ## Publish both projects for production

# ══════════════════════════════════════════════════════════════
#  RUN LOCALLY (apps on host, infra in Docker)
# ══════════════════════════════════════════════════════════════

.PHONY: infra infra-down run-api run-app run watch-api watch-app

infra: ## Start infrastructure only (Postgres, pgAdmin, Mailpit, Seq)
	$(DC) up -d postgres pgadmin mailpit seq
	@printf "\n$(GREEN)Infrastructure started:$(RESET)\n"
	@printf "  pgAdmin   → http://localhost:5050  (admin@swiftapp.ch / admin)\n"
	@printf "  Mailpit   → http://localhost:8028\n"
	@printf "  Seq       → http://localhost:8081\n"
	@printf "  Postgres  → localhost:5432\n\n"

infra-down: ## Stop infrastructure containers
	$(DC) stop postgres pgadmin mailpit seq

run-api: ## Run WebApi locally (no watch)
	dotnet run --project $(WEBAPI)

run-app: ## Run WebApp/Blazor locally (no watch)
	dotnet run --project $(WEBAPP)

watch-api: ## Run WebApi with hot-reload (dotnet watch)
	dotnet watch run --project $(WEBAPI)

watch-app: ## Run WebApp/Blazor with hot-reload (dotnet watch)
	dotnet watch run --project $(WEBAPP)

# ══════════════════════════════════════════════════════════════
#  DOCKER — DEVELOPMENT
# ══════════════════════════════════════════════════════════════

.PHONY: up down restart rebuild logs logs-api logs-app ps

up: ## Start ALL dev services (Docker Compose)
	$(DC) up -d
	@printf "\n$(GREEN)All services started:$(RESET)\n"
	@printf "  WebApi    → http://localhost:5000  (Swagger: /swagger)\n"
	@printf "  WebApp    → http://localhost:5001  (Blazor: /app)\n"
	@printf "  pgAdmin   → http://localhost:5050\n"
	@printf "  Mailpit   → http://localhost:8028\n"
	@printf "  Seq       → http://localhost:8081\n\n"

down: ## Stop all dev containers
	$(DC) down

restart: ## Restart all dev containers
	$(DC) restart

rebuild: ## Rebuild and restart dev containers
	$(DC) up -d --build

logs: ## Tail logs for all dev services
	$(DC) logs -f

logs-api: ## Tail WebApi logs
	$(DC) logs -f webapi

logs-app: ## Tail WebApp logs
	$(DC) logs -f webapp

logs-db: ## Tail Postgres logs
	$(DC) logs -f postgres

ps: ## Show running containers
	$(DC) ps

# ══════════════════════════════════════════════════════════════
#  DOCKER — PRODUCTION
# ══════════════════════════════════════════════════════════════

.PHONY: prod-up prod-down prod-rebuild prod-logs prod-ps

prod-up: ## Start production stack (requires .env.prod)
	@test -f .env.prod || (printf "$(RED)ERROR: .env.prod not found.$(RESET)\n" && exit 1)
	$(DC_PROD) up -d --build
	@printf "\n$(GREEN)Production stack started.$(RESET)\n"

prod-down: ## Stop production stack
	$(DC_PROD) down

prod-rebuild: ## Rebuild and restart production stack
	$(DC_PROD) up -d --build --force-recreate

prod-logs: ## Tail production logs
	$(DC_PROD) logs -f

prod-ps: ## Show production containers
	$(DC_PROD) ps

# ══════════════════════════════════════════════════════════════
#  DATABASE & EF CORE MIGRATIONS
# ══════════════════════════════════════════════════════════════

.PHONY: db-update db-migrate db-rollback db-status db-script db-reset db-seed db-fresh db-remove-last

db-update: ## Apply all pending EF migrations
	dotnet ef database update --project $(WEBAPI)

db-migrate: ## Create a new migration (usage: make db-migrate NAME=AddSalesOrderTable)
	@test -n "$(NAME)" || (printf "$(RED)ERROR: NAME is required. Usage: make db-migrate NAME=AddSalesOrderTable$(RESET)\n" && exit 1)
	dotnet ef migrations add $(NAME) --project $(WEBAPI)

db-rollback: ## Rollback last migration
	dotnet ef database update 0 --project $(WEBAPI)

db-status: ## Show pending migrations
	dotnet ef migrations list --project $(WEBAPI)

db-script: ## Generate SQL script for all migrations
	dotnet ef migrations script --project $(WEBAPI) -o migrations.sql

db-seed: ## Seed the database (run WebApi briefly to trigger auto-seed)
	@printf "$(CYAN)Seeding database via WebApi startup...$(RESET)\n"
	cd $(WEBAPI) && dotnet run -- --seed-only 2>&1 & PID=$$!; \
		sleep 15 && kill $$PID 2>/dev/null; \
		printf "$(GREEN)Database seeded (check logs for details).$(RESET)\n"

db-reset: ## Drop and recreate database (DESTRUCTIVE!)
	@printf "$(RED)WARNING: This will DROP the database and re-apply all migrations.$(RESET)\n"
	@read -p "Continue? [y/N] " confirm && [ "$$confirm" = "y" ] || exit 1
	dotnet ef database drop --force --project $(WEBAPI)
	dotnet ef database update --project $(WEBAPI)
	@printf "$(YELLOW)Note: Run 'make run-api' or 'make up' to auto-seed data on startup.$(RESET)\n"

db-fresh: ## Drop DB, re-migrate, and seed (DESTRUCTIVE!)
	@printf "$(RED)WARNING: This will DROP the database, re-apply migrations, and seed data.$(RESET)\n"
	@read -p "Continue? [y/N] " confirm && [ "$$confirm" = "y" ] || exit 1
	dotnet ef database drop --force --project $(WEBAPI)
	dotnet ef database update --project $(WEBAPI)
	@printf "$(GREEN)Database reset. Seed data will be applied on next app startup.$(RESET)\n"

db-remove-last: ## Remove the last migration (not yet applied)
	dotnet ef migrations remove --project $(WEBAPI)

# ══════════════════════════════════════════════════════════════
#  TESTING
# ══════════════════════════════════════════════════════════════

.PHONY: test test-unit test-arch test-cover test-module

test: ## Run all tests
	dotnet test $(SOLUTION)

test-unit: ## Run all module unit tests (exclude architecture)
	dotnet test $(SOLUTION) --filter "FullyQualifiedName~Modules"

test-arch: ## Run architecture boundary tests only
	dotnet test tests/SwiftApp.ERP.Architecture.Tests

test-cover: ## Run tests with code coverage report
	dotnet test $(SOLUTION) --collect:"XPlat Code Coverage" --results-directory ./coverage
	@printf "\n$(GREEN)Coverage results in ./coverage/$(RESET)\n"

test-module: ## Run tests for a single module (usage: make test-module MOD=Sales)
	@test -n "$(MOD)" || (printf "$(RED)ERROR: MOD is required. Usage: make test-module MOD=Sales$(RESET)\n" && exit 1)
	dotnet test tests/SwiftApp.ERP.Modules.$(MOD).Tests

# ══════════════════════════════════════════════════════════════
#  CODE QUALITY & FORMATTING
# ══════════════════════════════════════════════════════════════

.PHONY: format lint outdated

format: ## Format all C# files (dotnet format)
	dotnet format $(SOLUTION)

lint: ## Run format check (CI-friendly, no changes)
	dotnet format $(SOLUTION) --verify-no-changes

outdated: ## Check for outdated NuGet packages
	dotnet list $(SOLUTION) package --outdated

# ══════════════════════════════════════════════════════════════
#  UTILITIES
# ══════════════════════════════════════════════════════════════

.PHONY: swagger health count secrets

swagger: ## Open Swagger UI in browser
	@xdg-open http://localhost:5000/swagger 2>/dev/null || open http://localhost:5000/swagger 2>/dev/null || printf "Open http://localhost:5000/swagger\n"

webapp-open: ## Open Blazor WebApp in browser
	@xdg-open http://localhost:5001/app 2>/dev/null || open http://localhost:5001/app 2>/dev/null || printf "Open http://localhost:5001/app\n"

pgadmin-open: ## Open pgAdmin in browser
	@xdg-open http://localhost:5050 2>/dev/null || open http://localhost:5050 2>/dev/null || printf "Open http://localhost:5050\n"

seq-open: ## Open Seq log viewer in browser
	@xdg-open http://localhost:8081 2>/dev/null || open http://localhost:8081 2>/dev/null || printf "Open http://localhost:8081\n"

health: ## Check health endpoints of running apps
	@printf "$(CYAN)WebApi:$(RESET)  " && curl -sf http://localhost:5000/health && printf "\n" || printf "$(RED)DOWN$(RESET)\n"
	@printf "$(CYAN)WebApp:$(RESET)  " && curl -sf http://localhost:5001/health && printf "\n" || printf "$(RED)DOWN$(RESET)\n"

count: ## Count lines of C# code (src + tests)
	@printf "$(CYAN)Source:$(RESET)  " && find src -name '*.cs' | xargs wc -l | tail -1
	@printf "$(CYAN)Tests:$(RESET)   " && find tests -name '*.cs' | xargs wc -l | tail -1
	@printf "$(CYAN)Files:$(RESET)   " && find src tests -name '*.cs' | wc -l

secrets: ## Generate a random JWT secret (copy to .env.prod)
	@printf "$(CYAN)JWT_SECRET=$(RESET)" && openssl rand -base64 64 | tr -d '\n' && printf "\n"

# ══════════════════════════════════════════════════════════════
#  CLEANUP
# ══════════════════════════════════════════════════════════════

.PHONY: docker-prune nuke

docker-prune: ## Remove unused Docker images and volumes
	docker system prune -f
	docker volume prune -f

nuke: ## Full reset: stop containers, remove volumes, clean build (DESTRUCTIVE!)
	@printf "$(RED)WARNING: This will destroy ALL containers, volumes, and build artifacts.$(RESET)\n"
	@read -p "Continue? [y/N] " confirm && [ "$$confirm" = "y" ] || exit 1
	$(DC) down -v
	$(DC_PROD) down -v 2>/dev/null || true
	find . -type d \( -name bin -o -name obj -o -name publish -o -name coverage \) -exec rm -rf {} + 2>/dev/null || true
	@printf "$(GREEN)Everything cleaned.$(RESET)\n"
