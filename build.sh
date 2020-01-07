#!/bin/bash
dotnet build -f netstandard2.0 ParserObjects/ParserObjects.csproj --configuration Release
dotnet pack ParserObjects/ParserObjects.csproj --configuration Release --no-build --no-restore
