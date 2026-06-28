param(
    [string]$Project = "..\parking_booking.csproj",
    [switch]$Recreate
)

$ErrorActionPreference = "Stop"

if ($Recreate) {
    dotnet run --project $Project -- --seed --recreate
} else {
    dotnet run --project $Project -- --seed
}
