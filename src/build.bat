@echo off
setlocal enabledelayedexpansion

:: Configuration
set SOLUTION=CoffeeShops.sln
set CONFIGURATION=Release
set OUTPUT_DIR=..\deploy
set MAIN_PROJECT=CoffeeShops

:: Cleaning previous build
echo Cleaning previous build...
dotnet clean %SOLUTION% -c %CONFIGURATION%

:: Restoring packages
echo Restoring packages...
dotnet restore %SOLUTION%

:: Building components separately
echo Building components...
for %%p in (
    "CoffeeShops.Domain",
    "CoffeeShops.DTOs",
    "CoffeeShops.DataAccess",
    "CoffeeShops.Services",
    "%MAIN_PROJECT%"
) do (
    echo Building: %%~p
    dotnet build "%%~p" -c %CONFIGURATION% --no-restore
)

:: Creating deployment directory
echo Creating deployment directory...
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%\components"

:: Copying components
echo Copying components...
for %%p in (
    "CoffeeShops.DTOs\bin\%CONFIGURATION%\net8.0\*.dll",
    "CoffeeShops.Domain\bin\%CONFIGURATION%\net8.0\*.dll",
    "CoffeeShops.DataAccess\bin\%CONFIGURATION%\net8.0\*.dll",
    "CoffeeShops.Services\bin\%CONFIGURATION%\net8.0\*.dll"
) do (
    copy /y "%%~p" "%OUTPUT_DIR%\components\"
)

copy "CoffeeShops\bin\%CONFIGURATION%\net8.0\swagger*.json" "%OUTPUT_DIR%\"
copy "CoffeeShops\bin\%CONFIGURATION%\net8.0\*.xml" "%OUTPUT_DIR%\"
copy "CoffeeShops\appsettings.json" "%OUTPUT_DIR%\"


:: Publishing the main application
echo Publishing the main application...
dotnet publish "%MAIN_PROJECT%" -c %CONFIGURATION% -o "%OUTPUT_DIR%" --no-restore

:: Copying configuration files
echo Copying configuration files...
if exist "config.json" copy "config.json" "%OUTPUT_DIR%\"
if not exist "%OUTPUT_DIR%\logs" mkdir "%OUTPUT_DIR%\logs"

:: Creating run script
echo Creating run script...
echo @echo off > "%OUTPUT_DIR%\run.bat"
echo set COMPONENTS_PATH=%~dp0components >> "%OUTPUT_DIR%\run.bat"
echo CoffeeShops.exe >> "%OUTPUT_DIR%\run.bat"

endlocal
