$dllPath = "C:\Users\Lars\source\repos\Parser\bin\Release\Parser.dll";
$a  = [System.Reflection.Assembly]::LoadFrom($dllPath);
$p = "Parser-v" + $a.GetName().Version.ToString() + ".zip";

Rename-Item C:\Users\Lars\source\repos\Parser\Tools\zipped.zip $p