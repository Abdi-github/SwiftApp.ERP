@echo off
:: ═══════════════════════════════════════════════════════════
:: SwiftApp ERP — Solution Setup Script (Windows)
:: ═══════════════════════════════════════════════════════════
:: Run this ONCE after copying the blueprint to your PC.
:: Requires: .NET 10 SDK installed
::
:: Usage:  setup-solution.cmd
:: ═══════════════════════════════════════════════════════════

echo.
echo ╔══════════════════════════════════════════╗
echo ║   SwiftApp ERP — Solution Setup          ║
echo ║   Creating .sln and linking projects...  ║
echo ╚══════════════════════════════════════════╝
echo.

:: Create solution file
dotnet new sln -n SwiftApp.ERP --force

:: ── Source Projects ──────────────────────────────────────
echo Adding source projects...
dotnet sln add src\SwiftApp.ERP.SharedKernel\SwiftApp.ERP.SharedKernel.csproj --solution-folder src
dotnet sln add src\SwiftApp.ERP.Modules.Auth\SwiftApp.ERP.Modules.Auth.csproj --solution-folder src
dotnet sln add src\SwiftApp.ERP.Modules.MasterData\SwiftApp.ERP.Modules.MasterData.csproj --solution-folder src
dotnet sln add src\SwiftApp.ERP.Modules.Inventory\SwiftApp.ERP.Modules.Inventory.csproj --solution-folder src
dotnet sln add src\SwiftApp.ERP.Modules.Sales\SwiftApp.ERP.Modules.Sales.csproj --solution-folder src
dotnet sln add src\SwiftApp.ERP.Modules.Purchasing\SwiftApp.ERP.Modules.Purchasing.csproj --solution-folder src
dotnet sln add src\SwiftApp.ERP.Modules.Production\SwiftApp.ERP.Modules.Production.csproj --solution-folder src
dotnet sln add src\SwiftApp.ERP.Modules.Accounting\SwiftApp.ERP.Modules.Accounting.csproj --solution-folder src
dotnet sln add src\SwiftApp.ERP.Modules.Hr\SwiftApp.ERP.Modules.Hr.csproj --solution-folder src
dotnet sln add src\SwiftApp.ERP.Modules.Crm\SwiftApp.ERP.Modules.Crm.csproj --solution-folder src
dotnet sln add src\SwiftApp.ERP.Modules.QualityControl\SwiftApp.ERP.Modules.QualityControl.csproj --solution-folder src
dotnet sln add src\SwiftApp.ERP.Modules.Notification\SwiftApp.ERP.Modules.Notification.csproj --solution-folder src
dotnet sln add src\SwiftApp.ERP.WebApi\SwiftApp.ERP.WebApi.csproj --solution-folder src
dotnet sln add src\SwiftApp.ERP.WebApp\SwiftApp.ERP.WebApp.csproj --solution-folder src

:: ── Test Projects ────────────────────────────────────────
echo Adding test projects...
dotnet sln add tests\SwiftApp.ERP.SharedKernel.Tests\SwiftApp.ERP.SharedKernel.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.Modules.Auth.Tests\SwiftApp.ERP.Modules.Auth.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.Modules.MasterData.Tests\SwiftApp.ERP.Modules.MasterData.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.Modules.Inventory.Tests\SwiftApp.ERP.Modules.Inventory.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.Modules.Sales.Tests\SwiftApp.ERP.Modules.Sales.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.Modules.Purchasing.Tests\SwiftApp.ERP.Modules.Purchasing.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.Modules.Production.Tests\SwiftApp.ERP.Modules.Production.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.Modules.Accounting.Tests\SwiftApp.ERP.Modules.Accounting.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.Modules.Hr.Tests\SwiftApp.ERP.Modules.Hr.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.Modules.Crm.Tests\SwiftApp.ERP.Modules.Crm.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.Modules.QualityControl.Tests\SwiftApp.ERP.Modules.QualityControl.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.Modules.Notification.Tests\SwiftApp.ERP.Modules.Notification.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.WebApi.Tests\SwiftApp.ERP.WebApi.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.WebApp.Tests\SwiftApp.ERP.WebApp.Tests.csproj --solution-folder tests
dotnet sln add tests\SwiftApp.ERP.Architecture.Tests\SwiftApp.ERP.Architecture.Tests.csproj --solution-folder tests

:: ── Restore packages ─────────────────────────────────────
echo.
echo Restoring NuGet packages...
dotnet restore

echo.
echo ╔══════════════════════════════════════════╗
echo ║   Setup complete!                        ║
echo ║   Open SwiftApp.ERP.sln in Visual Studio ║
echo ╚══════════════════════════════════════════╝
echo.
pause
