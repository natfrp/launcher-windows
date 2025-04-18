#include <windows.h>
#include <intrin.h> 

// Build with `gcc -shared -o cpuid.dll cpu_checker.cpp -s -m32 -Wl,--kill-at`

extern "C" __declspec(dllexport) DWORD __stdcall ECX() {
    int cpuInfo[4];
    __cpuidex(cpuInfo, 1, 0);
    return (DWORD)cpuInfo[2];
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}