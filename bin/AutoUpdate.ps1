# Wait for HellDivers2OneKeyStratagem.exe to exit
$process = Get-Process -Name "HellDivers2OneKeyStratagem" -ErrorAction SilentlyContinue
if ($process) {
    $process.WaitForExit()
}

# Download HellDivers2OneKeyStratagem.zip to system temp directory
$downloadUrl = "https://hdokgh.213453245.xyz/HellDivers2OneKeyStratagem.zip"
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
