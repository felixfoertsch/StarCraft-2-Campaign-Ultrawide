$ErrorActionPreference='Stop'
$exe=Join-Path (Split-Path -Parent $PSScriptRoot) 'build\Campaign Ultrawide.exe'
$p=Start-Process -FilePath $exe -ArgumentList '--self-test' -PassThru
if(!$p.WaitForExit(90000)){$p.Kill();throw 'Native tests timed out.'}
Get-Content (Join-Path (Split-Path -Parent $exe) 'self-test-results.txt')
if($p.ExitCode -ne 0){throw 'Native tests failed.'}
