# 自动检查并安装 .NET Runtime

param(
    [int]$Version = 8,
    [ValidateSet("WindowsDesktop", "AspNetCore", "NETCore")]
    [string]$Type = "WindowsDesktop",
    [switch]$Verbose
)

function Get-RuntimePattern {
    <#
    .SYNOPSIS
    根据版本和类型获取运行时模式匹配字符串
    
    .PARAMETER Version
    .NET 版本号
    
    .PARAMETER Type
    Runtime 类型
    #>
    param(
        [int]$Version,
        [string]$Type
    )
    
    switch ($Type) {
        "WindowsDesktop" { return "Microsoft\.WindowsDesktop\.App.*$Version\.\d+\.\d+" }
        "AspNetCore" { return "Microsoft\.AspNetCore\.App.*$Version\.\d+\.\d+" }
        "NETCore" { return "Microsoft\.NETCore\.App.*$Version\.\d+\.\d+" }
        default { return "Microsoft\.WindowsDesktop\.App.*$Version\.\d+\.\d+" }
    }
}

function Get-InstallPaths {
    <#
    .SYNOPSIS
    根据类型获取安装路径
    
    .PARAMETER Type
    Runtime 类型
    #>
    param(
        [string]$Type
    )
    
    switch ($Type) {
        "WindowsDesktop" { 
            return @(
                "${env:ProgramFiles}\dotnet\shared\Microsoft.WindowsDesktop.App",
                "${env:ProgramFiles(x86)}\dotnet\shared\Microsoft.WindowsDesktop.App"
            )
        }
        "AspNetCore" { 
            return @(
                "${env:ProgramFiles}\dotnet\shared\Microsoft.AspNetCore.App",
                "${env:ProgramFiles(x86)}\dotnet\shared\Microsoft.AspNetCore.App"
            )
        }
        "NETCore" { 
            return @(
                "${env:ProgramFiles}\dotnet\shared\Microsoft.NETCore.App",
                "${env:ProgramFiles(x86)}\dotnet\shared\Microsoft.NETCore.App"
            )
        }
        default { 
            return @(
                "${env:ProgramFiles}\dotnet\shared\Microsoft.WindowsDesktop.App",
                "${env:ProgramFiles(x86)}\dotnet\shared\Microsoft.WindowsDesktop.App"
            )
        }
    }
}

function Test-DotNetRuntime {
    <#
    .SYNOPSIS
    检查指定版本的 .NET Runtime 是否已安装
    
    .DESCRIPTION
    通过多种方法检查指定版本和类型的 .NET Runtime 的安装状态
    
    .PARAMETER Version
    .NET 版本号 (7, 8, 9)
    
    .PARAMETER Type
    Runtime 类型 (WindowsDesktop, AspNetCore, NETCore)
    
    .PARAMETER Verbose
    显示详细的检查信息
    
    .RETURNS
    返回布尔值，True 表示已安装，False 表示未安装
    #>
    param(
        [int]$Version,
        [string]$Type,
        [switch]$Verbose
    )
    
    $isInstalled = $false
    $checkMethods = @()
    
    # 方法1: 检查 dotnet --list-runtimes 命令
    try {
        $runtimes = & dotnet --list-runtimes 2>$null
        $runtimePattern = Get-RuntimePattern -Version $Version -Type $Type
        if ($runtimes -and $runtimes -match $runtimePattern) {
            $isInstalled = $true
            $checkMethods += "dotnet --list-runtimes: 找到 .NET $Version $Type Runtime"
        }
        else {
            $checkMethods += "dotnet --list-runtimes: 未找到 .NET $Version $Type Runtime"
        }
    }
    catch {
        $checkMethods += "dotnet --list-runtimes: 命令执行失败"
    }
    
    # 方法2: 检查注册表
    $registryPaths = @(
        "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*",
        "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*"
    )
    
    $dotnetFound = $false
    foreach ($path in $registryPaths) {
        $installedApps = Get-ItemProperty -Path $path -ErrorAction SilentlyContinue
        $dotnetApps = $installedApps | Where-Object { 
            $displayName = $_.DisplayName
            switch ($Type) {
                "WindowsDesktop" {
                    $displayName -like "*Microsoft Windows Desktop Runtime - $Version*" -or
                    $displayName -like "*Microsoft .NET Desktop Runtime - $Version*" -or
                    $displayName -like "*Microsoft .NET $Version Desktop Runtime*"
                }
                "AspNetCore" {
                    $displayName -like "*Microsoft ASP.NET Core Runtime - $Version*" -or
                    $displayName -like "*Microsoft .NET $Version ASP.NET Core Runtime*"
                }
                "NETCore" {
                    $displayName -like "*Microsoft .NET Core Runtime - $Version*" -or
                    $displayName -like "*Microsoft .NET $Version Core Runtime*"
                }
            }
        }
        
        if ($dotnetApps) {
            $dotnetFound = $true
            $checkMethods += "注册表检查: 找到 .NET $Version $Type Runtime"
            break
        }
    }
    
    if (-not $dotnetFound) {
        $checkMethods += "注册表检查: 未找到 .NET $Version $Type Runtime"
    }
    
    # 方法3: 检查安装目录
    $installPaths = Get-InstallPaths -Type $Type
    
    $pathFound = $false
    foreach ($path in $installPaths) {
        if (Test-Path $path) {
            $versionDirs = Get-ChildItem -Path $path -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -like "$Version.*" }
            if ($versionDirs) {
                $pathFound = $true
                $checkMethods += "安装目录检查: 找到 .NET $Version $Type Runtime 在 $path"
                break
            }
        }
    }
    
    if (-not $pathFound) {
        $checkMethods += "安装目录检查: 未找到 .NET $Version $Type Runtime"
    }
    
    # 综合判断
    if ($dotnetFound -or $pathFound) {
        $isInstalled = $true
    }
    
    # 显示结果
    if ($Verbose) {
        Write-Host "=== .NET $Version $Type Runtime 检查结果 ===" -ForegroundColor Cyan
        foreach ($method in $checkMethods) {
            Write-Host $method
        }
        Write-Host ""
    }
    
    if ($isInstalled) {
        Write-Host ".NET $Version $Type Runtime 已安装" -ForegroundColor Green
        return $true
    }
    else {
        Write-Host ".NET $Version $Type Runtime 未安装" -ForegroundColor Red
        return $false
    }
}

function Get-DownloadUrl {
    <#
    .SYNOPSIS
    根据版本和类型获取下载链接
    
    .PARAMETER Version
    .NET 版本号
    
    .PARAMETER Type
    Runtime 类型
    #>
    param(
        [int]$Version,
        [string]$Type
    )

    $url = "https://aka.ms/dotnet/$Version.0/dotnet-runtime-win-x64.exe"
    if ($Type -eq "WindowsDesktop") {
        $url = "https://aka.ms/dotnet/$Version.0/windowsdesktop-runtime-win-x64.exe"
    }
    elseif ($Type -eq "AspNetCore") {
        $url = "https://aka.ms/dotnet/$Version.0/aspnetcore-runtime-win-x64.exe"
    }
    return $url
}

function Install-DotNetRuntime {
    <#
    .SYNOPSIS
    安装指定版本的 .NET Runtime
    
    .DESCRIPTION
    下载并安装指定版本和类型的 .NET Runtime
    
    .PARAMETER Version
    .NET 版本号 (7, 8, 9)
    
    .PARAMETER Type
    Runtime 类型 (WindowsDesktop, AspNetCore, NETCore)
    #>
    param(
        [int]$Version,
        [string]$Type
    )
    
    Write-Host "开始安装 .NET $Version $Type Runtime..." -ForegroundColor Yellow
    
    # 获取下载链接
    $downloadUrl = Get-DownloadUrl -Version $Version -Type $Type
    $installerPath = "$env:TEMP\dotnet-$Version-$Type-runtime.exe"
    
    try {
        # 下载安装程序
        Write-Host "正在下载 .NET $Version $Type Runtime..." -ForegroundColor Yellow
        Invoke-WebRequest -Uri $downloadUrl -OutFile $installerPath -UseBasicParsing
        
        # 安装
        Write-Host "正在安装 .NET $Version $Type Runtime..." -ForegroundColor Yellow
        Start-Process -FilePath $installerPath -ArgumentList "/quiet", "/norestart" -Wait
        
        # 清理临时文件
        if (Test-Path $installerPath) {
            Remove-Item $installerPath -Force
        }
        
        Write-Host ".NET $Version $Type Runtime 安装完成" -ForegroundColor Green
        
        # 重新检查安装状态
        if (Test-DotNetRuntime -Version $Version -Type $Type) {
            Write-Host "安装验证成功" -ForegroundColor Green
        }
        else {
            Write-Host "安装验证失败，请手动检查" -ForegroundColor Red
        }
        
    }
    catch {
        Write-Host "安装失败: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# 主程序
Write-Host "=== .NET Runtime 安装工具 ===" -ForegroundColor Cyan
Write-Host "检查版本: $Version, 类型: $Type" -ForegroundColor Yellow
Write-Host ""

$isInstalled = Test-DotNetRuntime -Version $Version -Type $Type -Verbose:$Verbose

if (-not $isInstalled) {
    Write-Host ""
    Install-DotNetRuntime -Version $Version -Type $Type
}

Write-Host ""
Write-Host "检查完成" -ForegroundColor Cyan
