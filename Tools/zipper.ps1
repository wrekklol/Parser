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



$a  = [System.Reflection.Assembly]::LoadFrom($targetPath);
$p = "Parser-v" + $a.GetName().Version.ToString() + ".zip";
$fullPath = $projectDir + "Builds\" + $p

if (Test-Path -Path $fullPath -PathType Leaf)
{
    Remove-Item -Path $fullPath
}

Rename-Item $buildPath $p

Read-Host -Prompt "Press Enter to exit"