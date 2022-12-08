@echo off
set SOURCE=.\Examples\Assets\Prg
set TARGET=.\Server\Assets\Prg
set LIST=%1
set DIRS=Photon Prefabs Resources Tests
set FILES=Photon.meta Prefabs.meta Resources.meta Tests.meta *.asmdef *.asmdef.meta
rem
rem /XO :: eXclude Older files.
rem
robocopy %SOURCE% %TARGET% *.* %LIST% /XO /S /XD %DIRS% /XF %FILES%
pause