#!/usr/bin/env bash
# Codespace ilk oluşturulduğunda bir kez çalışır.
set -e
cd "$(dirname "$0")/.."

echo "==> React arayüzü derleniyor"
(cd frontend && npm ci && npm run build)

echo "==> .NET paketleri indiriliyor ve proje derleniyor"
dotnet restore MiniDpp.sln
dotnet build MiniDpp.csproj --no-restore
