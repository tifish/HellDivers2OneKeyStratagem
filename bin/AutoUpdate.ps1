# Wait for HellDivers2OneKeyStratagem.exe to exit
$process = Get-Process -Name "HellDivers2OneKeyStratagem" -ErrorAction SilentlyContinue
if ($process) {
    $process.WaitForExit()
}

# Check if there are any command line arguments
if ($args.Count -eq 0) {
    Write-Host "No command line arguments provided. Exiting..."
    Exit
}

# Get the first command line argument as the download URL
$downloadUrl = $args[0]
# Download HellDivers2OneKeyStratagem.7z to system temp directory
$packPath = "$env:TEMP\HellDivers2OneKeyStratagem.7z"
Invoke-WebRequest -Uri $downloadUrl -OutFile $packPath

# Check if $packPath exists
if (-not (Test-Path $packPath)) {
    Write-Host "$packPath file not found. Exiting..."
    Exit
}

# Delete all directories and files except Settings directory
Get-ChildItem -Path $PSScriptRoot `
| Where-Object { $_.Name -ne "Settings" -and $_.Name -ne "7Zip" } `
| Remove-Item -Recurse -Force

# Extract HellDivers2OneKeyStratagem.7z in to $PSScriptRoot
& {
    7Zip\7za.exe x $packPath -o"$PSScriptRoot" -x!7Zip -y
} -ErrorAction SilentlyContinue

# Start HellDivers2OneKeyStratagem.exe
Start-Process -FilePath "$PSScriptRoot\HellDivers2OneKeyStratagem.exe"
