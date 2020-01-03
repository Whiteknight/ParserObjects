dotnet build -f netstandard2.0 ParserObjects/ParserObjects.csproj --configuration Release
if ERRORLEVEL 1 GOTO :error

dotnet pack ParserObjects/ParserObjects.csproj --configuration Release --no-build --no-restore
if ERRORLEVEL 1 GOTO :error

goto :done

:error
echo Build FAILED

:done