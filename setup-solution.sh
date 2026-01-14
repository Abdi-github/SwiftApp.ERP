#!/usr/bin/env bash
# ═══════════════════════════════════════════════════════════
# SwiftApp ERP — Solution Setup Script (Linux/macOS)
# ═══════════════════════════════════════════════════════════
set -euo pipefail

echo ""
echo "╔══════════════════════════════════════════╗"
echo "║   SwiftApp ERP — Solution Setup          ║"
echo "║   Creating .sln and linking projects...  ║"
echo "╚══════════════════════════════════════════╝"
echo ""

dotnet new sln -n SwiftApp.ERP --force

# Source projects
for proj in src/SwiftApp.ERP.*/SwiftApp.ERP.*.csproj; do
    dotnet sln add "$proj" --solution-folder src
done

# Test projects
for proj in tests/SwiftApp.ERP.*/SwiftApp.ERP.*.csproj; do
    dotnet sln add "$proj" --solution-folder tests
done

echo ""
echo "Restoring NuGet packages..."
dotnet restore

echo ""
echo "╔══════════════════════════════════════════╗"
echo "║   Setup complete!                        ║"
echo "║   Open SwiftApp.ERP.sln in Visual Studio ║"
echo "╚══════════════════════════════════════════╝"
