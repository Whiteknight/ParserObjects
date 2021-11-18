#!/bin/bash
cd ParserObjects/bin/Release
nuget push ParserObjects.$1.nupkg -Source https://www.nuget.org/api/v2/package
nuget push ParserObjects.$1.snupkg -Source https://www.nuget.org/api/v2/package
