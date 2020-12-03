#!/bin/bash
dotnet build ParserObjects.sln --configuration Release
dotnet pack ParserObjects/ParserObjects.csproj --configuration Release --no-build --no-restore
