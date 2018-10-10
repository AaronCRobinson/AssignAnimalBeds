@echo off
SET "ProjectName=LayRightHere"
SET "SolutionDir=C:\Users\robin\Desktop\Games\RimWorld Modding\Source\LayRightHere\Source"
@echo on

xcopy /S /Y "%SolutionDir%\..\About\*" "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\%ProjectName%\About\"
xcopy /S /Y "%SolutionDir%\..\Defs\*" "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\%ProjectName%\Defs\"
xcopy /S /Y "%SolutionDir%\..\Patches\*" "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\%ProjectName%\Patches\"