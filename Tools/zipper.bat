@echo off

del %cd%\zipped.zip

"C:\Program Files\7-Zip\7z" a -y -tzip %2 %1 -mx5

::PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& {Start-Process PowerShell -ArgumentList '-NoProfile -ExecutionPolicy Bypass -File ""%cd%\zipper_rename.ps1""' -Verb RunAs}"