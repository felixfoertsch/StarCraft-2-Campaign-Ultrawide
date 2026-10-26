param([switch]$SkipUi)
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
$exe=& (Join-Path $project 'Build.ps1')
$modes=@('--self-test')
if(!$SkipUi){$modes+='--smoke-test'}
foreach($mode in $modes){
    $process=Start-Process -FilePath $exe -ArgumentList $mode -WindowStyle Hidden -PassThru
    if(!$process.WaitForExit(90000)){$process.Kill();throw ('Test timed out: '+$mode)}
    $report=Join-Path (Split-Path -Parent $exe) $(if($mode -eq '--self-test'){'self-test-results.txt'}else{'layout-test-results.txt'})
    if(Test-Path -LiteralPath $report){Get-Content -LiteralPath $report -Encoding UTF8}
    if($process.ExitCode -ne 0){throw ('Test failed: '+$mode+' (exit '+$process.ExitCode+')')}
}
'All requested tests passed.'
