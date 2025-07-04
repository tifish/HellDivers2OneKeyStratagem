$appName = "HellDivers2OneKeyStratagem"

# Wait for .exe to exit
$process = Get-Process -Name $appName -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "Waiting for $appName to exit..."
    $process.WaitForExit()
}

# Check if there are any command line arguments
if ($args.Count -eq 0) {
    Write-Host "No command line arguments provided. Exiting..."
    Pause
    Exit
}

# Get the first command line argument as the download URL
$downloadUrl = $args[0]
# Download .7z to system temp directory
$packPath = "$env:TEMP\$appName.7z"

# Try to download from multiple mirrors
$mirrors = @(
    $downloadUrl,
    $downloadUrl -replace "^https://github.com/", "https://ghfast.top/https://github.com/",
    $downloadUrl -replace "^https://github.com/", "https://gh-proxy.com/github.com/"
)

$downloaded = $false
foreach ($url in $mirrors) {
    Write-Host "Trying to download update from $url..."
    try {
        Invoke-WebRequest -Uri $url -OutFile $packPath -ErrorAction Stop
        $downloaded = $true
        break
    }
    catch {
        Write-Host "Failed to download $url. Error: $($_.Exception.Message)"
    }
}

if (-not $downloaded) {
    Write-Host "All download attempts failed. Exiting..."
    Pause
    Exit
}

# Check if $packPath exists
if (-not (Test-Path $packPath)) {
    Write-Host "Failed to download $downloadUrl. Exiting..."
    Pause
    Exit
}

# Delete old Libs directory
Remove-Item -Recurse -Force -Path "$PSScriptRoot\Libs"

# Copy 7za.exe to temporary directory
$sevenZipTmp = "$env:TEMP\7za.exe"
Copy-Item -Path "$PSScriptRoot\7Zip\7za.exe" -Destination $sevenZipTmp -Force

# Extract .7z in to $PSScriptRoot
& "$sevenZipTmp" x $packPath -o"$PSScriptRoot" -y

# Remove 7za.exe from temporary directory
Remove-Item -Force -Path $sevenZipTmp

# Delete downloaded pack file
Write-Host "Cleaning up temporary files..."
Remove-Item -Force -Path $packPath

# Start .exe
Write-Host "Starting $appName..."
if ($args.Count -gt 1) {
    Start-Process -FilePath "$PSScriptRoot\$appName.exe" -ArgumentList $args[1..$args.Length]
}
else {
    Start-Process -FilePath "$PSScriptRoot\$appName.exe"
}
