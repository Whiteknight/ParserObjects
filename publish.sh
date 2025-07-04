#!/bin/bash

# Build and Pack the solution. 
dotnet build ParserObjects.sln --configuration Release
dotnet pack ParserObjects/ParserObjects.csproj --configuration Release --no-build --no-restore

# Get the current version number
POVERSION=$(cat ParserObjects/ParserObjects.csproj | sed -n -e 's/.*<Version>\(.*\)<\/Version>.*/\1/p')
echo Pushing version $POVERSION

# Push the packages
nuget push ParserObjects/bin/Release/ParserObjects.$POVERSION.nupkg -Source https://www.nuget.org/api/v2/package
nuget push ParserObjects/bin/Release/ParserObjects.$POVERSION.snupkg -Source https://www.nuget.org/api/v2/package
