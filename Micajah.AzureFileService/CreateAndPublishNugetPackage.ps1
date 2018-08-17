Remove-Item –path bin\Debug\package\* –recurse
..\NuGet.exe pack . -OutputDirectory bin\Debug\package -IncludeReferencedProjects -Symbols -NonInteractive -Properties Configuration=Debug
Dir .\bin\Debug\package\*.symbols.nupkg | rename-item -newname { [io.path]::ChangeExtension($_.name, "nupkg1") }
Remove-Item –path bin\Debug\package\*.nupkg
Dir .\bin\Debug\package\*.nupkg1 | rename-item -newname { [io.path]::ChangeExtension($_.name, "") }
Dir .\bin\Debug\package\*.symbols | rename-item -newname { [io.path]::ChangeExtension($_.name, "nupkg") }
..\Nuget.exe sources Add -Name "Micajah" -Source "https://micajah.pkgs.visualstudio.com/_packaging/Micajah/nuget/v3/index.json"
..\Nuget.exe push -Source "Micajah" -ApiKey VSTS .\bin\Debug\package\*.nupkg