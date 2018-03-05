MSBuild Automata.MiniORM.sln /t:Rebuild /p:Configuration=Debug
Nuget/nuget.exe pack Automata.MiniORM -OutputDirectory Nuget/Publish
Nuget/nuget.exe pack Automata.MiniORM.XML -OutputDirectory Nuget/Publish