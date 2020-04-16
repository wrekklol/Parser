#$projectDir = "C:\Users\Lars\source\repos\Parser\"#$args[0]
#$targetDir = "C:\Users\Lars\source\repos\Parser\bin\Release\Parser\"#$args[1]
#$targetPath = "C:\Users\Lars\source\repos\Parser\bin\Release\Parser\Parser.dll"#$args[2]

Param
(
    [string]$projectDir,
    [string]$targetDir,
    [string]$targetPath
)

Add-Type -AssemblyName System.Windows.Forms

# Create build
$releaseVersion = "v" + [System.Reflection.Assembly]::LoadFrom($targetPath).GetName().Version.ToString()
$fileName = "Parser-" + $releaseVersion + ".zip"
$buildPath = $projectDir + "Builds\" + $fileName
$buildFromPath = ($targetDir + "\*")
$7zipPath = "C:\Program Files\7-Zip\7z.exe"

if (Test-Path -Path $buildPath -PathType Leaf)
{
    Remove-Item -Path $buildPath
}
if (-not (Test-Path -Path $7zipPath -PathType Leaf))
{
    throw "7 zip file '$7zipPath' not found"
}
if (-not (Test-Path -Path $buildFromPath -PathType Leaf))
{
    throw "build '$buildFromPath' not found"
}

Set-Alias 7zip $7zipPath
7zip a -y -tzip $buildPath $buildFromPath -mx=0



# Create release
$usertoken = "token " + (Get-Content -Path ".\Tools\usertoken.txt").Trim()
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
    Invoke-WebRequest -Headers @{"Authorization" = $usertoken} `
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

$global:balmsg = New-Object System.Windows.Forms.NotifyIcon
$notifpath = (Get-Process -id $pid).Path
$balmsg.Icon = [System.Drawing.Icon]::ExtractAssociatedIcon($notifpath)
$balmsg.BalloonTipIcon = [System.Windows.Forms.ToolTipIcon]::Info
$balmsg.BalloonTipText = $responseCreate.StatusCode.ToString() + " - " + $responseCreate.StatusDescription.ToString() + " " + $fileName
$balmsg.BalloonTipTitle = "Release Create Info"
$balmsg.Visible = $true
$balmsg.ShowBalloonTip(20000)



# Upload release
$jsonObj = ConvertFrom-Json $($responseCreate.Content.ToString())
$url = ([System.Uri]('https://uploads.github.com/repos/wrekklol/Parser/releases/' + $jsonObj.id + '/assets?name=' + $fileName)).AbsoluteUri

$responseUpload = try 
{
    Invoke-WebRequest -Headers @{"Authorization" = $usertoken} -Method POST -InFile $buildPath -Uri $url -ContentType application/zip -UseBasicParsing
}
catch [System.Net.WebException] 
{
    Write-Verbose "An exception was caught: $($_.Exception.Message)"
    $_.Exception.Response 
}

$global:balmsg = New-Object System.Windows.Forms.NotifyIcon
$notifpath = (Get-Process -id $pid).Path
$balmsg.Icon = [System.Drawing.Icon]::ExtractAssociatedIcon($notifpath)
$balmsg.BalloonTipIcon = [System.Windows.Forms.ToolTipIcon]::Info
$balmsg.BalloonTipText = $responseUpload.StatusCode.ToString() + " - " + $responseUpload.StatusDescription.ToString() + " " + $fileName
$balmsg.BalloonTipTitle = "Release Upload Info"
$balmsg.Visible = $true
$balmsg.ShowBalloonTip(20000)