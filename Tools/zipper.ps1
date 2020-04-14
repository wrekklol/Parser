$projectDir = "C:\Users\Lars\source\repos\Parser\"#$args[0]
$targetDir = "C:\Users\Lars\source\repos\Parser\bin\Release\Parser\"#$args[1]
$targetPath = "C:\Users\Lars\source\repos\Parser\bin\Release\Parser\Parser.dll"#$args[2]

$buildPath = $projectDir + "Builds\zipped.zip"
$7zipPath = "$env:ProgramFiles\7-Zip\7z.exe"



if (-not (Test-Path -Path $7zipPath -PathType Leaf)) 
{
    throw "7 zip file '$7zipPath' not found"
}

Set-Alias 7zip $7zipPath

$Source = $targetDir
$Target = $buildPath

7zip a -y -tzip $Target $Source -mx=9



$a  = [System.Reflection.Assembly]::LoadFrom($targetPath);
$p = "Parser-v" + $a.GetName().Version.ToString() + ".zip";

Rename-Item $buildPath $p