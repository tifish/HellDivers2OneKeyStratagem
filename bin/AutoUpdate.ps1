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
# Download HellDivers2OneKeyStratagem.zip to system temp directory
$zipPath = "$env:TEMP\HellDivers2OneKeyStratagem.zip"
Invoke-WebRequest -Uri $downloadUrl -OutFile $zipPath

# Check if $zipPath exists
if (-not (Test-Path $zipPath)) {
    Write-Host "Zip file not found. Exiting..."
    Exit
}

# Delete all directories and files except Settings directory
Get-ChildItem -Path $PSScriptRoot `
| Where-Object { $_.Name -ne "Settings" } `
| Remove-Item -Recurse -Force

# Extract HellDivers2OneKeyStratagem.zip in system temp directory to $PSScriptRoot
Expand-Archive -Path $zipPath -DestinationPath $PSScriptRoot -Force

# Start HellDivers2OneKeyStratagem.exe
Start-Process -FilePath "$PSScriptRoot\HellDivers2OneKeyStratagem.exe"
