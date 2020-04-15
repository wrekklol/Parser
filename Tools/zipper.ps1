#$projectDir = "C:\Users\Lars\source\repos\Parser\"#$args[0]
#$targetDir = "C:\Users\Lars\source\repos\Parser\bin\Release\Parser\"#$args[1]
#$targetPath = "C:\Users\Lars\source\repos\Parser\bin\Release\Parser\Parser.dll"#$args[2]

Param
(
    [string]$projectDir,
    [string]$targetDir,
    [string]$targetPath
)

$buildPath = $projectDir + "Builds\zipped.zip"
$7zipPath = "C:\Program Files\7-Zip\7z.exe"

if (-not (Test-Path -Path $7zipPath -PathType Leaf))
{
    throw "7 zip file '$7zipPath' not found"
}

Set-Alias 7zip $7zipPath

$Source = $targetDir + "\*"
$Target = $buildPath

7zip a -y -tzip $Target $Source -mx=0



$releaseVersion = "v" + [System.Reflection.Assembly]::LoadFrom($targetPath).GetName().Version.ToString()
$fileName = "Parser-" + $releaseVersion + ".zip"
$fullPath = $projectDir + "Builds\" + $fileName

if (Test-Path -Path $fullPath -PathType Leaf)
{
    Remove-Item -Path $fullPath
}

Rename-Item $buildPath $fileName

$usertoken = Get-Content -Path ".\usertoken.txt"

$bodyCreateRelease = 
@{
    "tag_name" = $releaseVersion
    "target_commitish" = "master"
    "name" = $releaseVersion
    "body" = "."
    "draft" = $false
    "prerelease" = $false
} | ConvertTo-Json

$responseCreate = try 
{ 
    Invoke-WebRequest -Headers @{"Authorization" = "token " + $usertoken} `
    -Method POST `
    -Body $bodyCreateRelease `
    -Uri https://api.github.com/repos/wrekklol/Parser/releases `
    -ContentType application/json `
    -UseBasicParsing
} 
catch [System.Net.WebException] 
{ 
    Write-Verbose "An exception was caught: $($_.Exception.Message)"
    $_.Exception.Response 
}

$jsonObj = ConvertFrom-Json $([String]::new($responseCreate.Content))
$url = ([System.Uri]('https://uploads.github.com/repos/wrekklol/Parser/releases/' + $jsonObj.id + '/assets?name=' + $fileName)).AbsoluteUri

$responseUpload = try 
{ 
    Invoke-WebRequest -Headers @{"Authorization" = "token " + $usertoken} -Method POST -InFile $fullPath -Uri $url -ContentType application/zip -UseBasicParsing
} 
catch [System.Net.WebException] 
{ 
    Write-Verbose "An exception was caught: $($_.Exception.Message)"
    $_.Exception.Response 
}



Add-Type -AssemblyName System.Windows.Forms
$global:balmsg = New-Object System.Windows.Forms.NotifyIcon
$notifpath = (Get-Process -id $pid).Path
$balmsg.Icon = [System.Drawing.Icon]::ExtractAssociatedIcon($notifpath)
$balmsg.BalloonTipIcon = [System.Windows.Forms.ToolTipIcon]::Info
$balmsg.BalloonTipText = $responseUpload.StatusCode.ToString() + " - " + $responseUpload.StatusDescription.ToString() + " " + $fileName
$balmsg.BalloonTipTitle = "Build Information"
$balmsg.Visible = $true
$balmsg.ShowBalloonTip(20000)