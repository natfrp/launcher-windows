@echo off
set "VS_PATH="
if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\VC\Auxiliary\Build\vcvarsall.bat" (
    set "VS_PATH=%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\VC\Auxiliary\Build\vcvarsall.bat"
) else if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvarsall.bat" (
    set "VS_PATH=%ProgramFiles%\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvarsall.bat"
) else if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" (
    set "VS_PATH=%ProgramFiles%\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat"
)

if "%VS_PATH%"=="" (
    echo VS2022 not found.
    exit /b 1
)
call "%VS_PATH%" x86

del cpuid.dll

cl /c /GS- /Gy /Zl cpuid.cpp

link /NODEFAULTLIB /NOENTRY /DLL /EXPORT:ECX ^
    /INCREMENTAL:NO ^
    /MANIFESTUAC:NO ^
    /NOCOFFGRPINFO ^
    /PDBSTRIPPED ^
    /NOLOGO ^
    /DYNAMICBASE:NO ^
    cpuid.obj /OUT:cpuid.dll

del cpuid.obj
del cpuid.exp
del cpuid.lib

PAUSE
