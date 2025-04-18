#include <windows.h>
#include <intrin.h>

extern "C" __declspec(dllexport) DWORD __stdcall ECX() {
    int cpuInfo[4];
    __cpuidex(cpuInfo, 1, 0);
    return (DWORD)cpuInfo[2];
}
