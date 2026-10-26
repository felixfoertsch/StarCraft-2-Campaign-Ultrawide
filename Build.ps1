param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build'))
$ErrorActionPreference='Stop'
if(-not [Environment]::Is64BitOperatingSystem){throw 'Build on 64-bit Windows.'}
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if(!(Test-Path -LiteralPath $compiler)){throw 'The .NET Framework C# compiler is required.'}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$arguments=@('/nologo','/codepage:65001','/target:winexe','/platform:x64','/optimize+','/define:SC2_TESTS',('/out:'+(Join-Path $OutputDirectory 'Campaign Ultrawide.exe')),'/reference:System.Windows.Forms.dll','/reference:System.Drawing.dll','/reference:System.Web.Extensions.dll','/reference:System.Core.dll')
$icon=Join-Path $PSScriptRoot 'Assets\App.ico'
$arguments+=('/win32icon:'+$icon)
$arguments+=('/resource:'+$icon+',Application.Icon')
$arguments+=(Join-Path $PSScriptRoot 'Launcher.cs')
$arguments+=@(Get-ChildItem (Join-Path $PSScriptRoot 'Engine') -Filter *.cs | ForEach-Object {$_.FullName})
$arguments+=@(Get-ChildItem (Join-Path $PSScriptRoot 'Tests') -Filter *.cs | ForEach-Object {$_.FullName})
& $compiler @arguments
if($LASTEXITCODE -ne 0){throw 'Compilation failed.'}
Write-Output (Join-Path $OutputDirectory 'Campaign Ultrawide.exe')
