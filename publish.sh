#!/bin/bash
cd ParserObjects/bin/Release
dotnet nuget push ParserObjects.$1.nupkg --source https://www.nuget.org/api/v2/package
dotnet nuget push ParserObjects.$1.snupkg --source https://www.nuget.org/api/v2/package
