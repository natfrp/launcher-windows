if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "`n`n!!! 权限不足，请在开始菜单搜索框输入 powershell，右键点击 Windows PowerShell 以管理员身份运行，然后再粘贴此命令 !!!`n" -ForegroundColor Yellow -BackgroundColor DarkGreen
    return
}

function Cleanup {
    param (
        [string]$Message
    )

    if ($Message) {
        Write-Host $Message -ForegroundColor Yellow
    }

    Write-Host "[*] 正在删除临时文件"
    Remove-Item -Path $tmpDir -Recurse -Force

    if ($wd -eq 1) {
        try {
            Remove-MpPreference -ExclusionPath $tmpDir
            Write-Host "[-] 已删除 WD 临时排除项" -ForegroundColor Green
        } catch {
            Write-Host "[!] WD 排除项删除失败" -ForegroundColor Red
        }
    }
}

Write-Host "`n`n[*] 安装已开始，请不要中断安装过程，否则可能造成 WD 排除项残留" -ForegroundColor Yellow

$tmpDir = Join-Path $env:TEMP natfrp-$(New-Guid)
mkdir $tmpDir | Out-Null

$wd = 0
try {
    if ((Get-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender" -Name "DisableLocalAdminMerge" -ErrorAction SilentlyContinue).DisableLocalAdminMerge -eq 1) {
        Write-Host "[*] 检测到 WD 被错误配置为不允许排除, 正在修复..."
        Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender" -Name "DisableLocalAdminMerge" -Value 0
        Write-Host "[+] 已修复 WD 排除项配置" -ForegroundColor Green
    }

    if ((Get-MpComputerStatus).AntivirusEnabled -and ((Get-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\SecurityHealthService" -Name "Start") -eq 4)) {
        Write-Host "[*] 检测到安全中心服务被错误禁用, 正在修复..."
        Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\SecurityHealthService" -Name "Start" -Value 2
        Write-Host "[+] 已修复安全中心服务被错误禁用" -ForegroundColor Green
        Write-Host "[*] 正在重新启动安全中心服务..."
        Restart-Service -Name "SecurityHealthService"
        Write-Host "[+] 已重新启动安全中心服务, 如出现问题请尝试重新启动设备" -ForegroundColor Green
    }

    Add-MpPreference -ExclusionPath $tmpDir
    $wd = 1
    Write-Host "[+] 已添加 WD 临时排除项" -ForegroundColor Green
} catch {
    Write-Host "[!] WD 排除项添加失败, 您的设备可能没有使用 Windows Defender, 推荐您直接下载安装包并运行" -ForegroundColor Yellow

    $x = Read-Host "  是否继续尝试通过脚本安装？(输入 y 回车继续, 其他退出)"
    if ($x -ne "y") {
        Cleanup "[-] 安装已取消, 请手动下载安装包并运行"
        return
    }
}

$pkgPath = Join-Path $tmpDir "SakuraLauncher.exe"

Write-Host "[*] 正在下载启动器安装包..."
$ProgressPreference = "SilentlyContinue"
Invoke-WebRequest -Uri "%INSTALLER_URL%" -OutFile $pkgPath

Write-Host "[*] 正在校验文件 HASH..."
if ((Get-FileHash -Path $pkgPath -Algorithm SHA256).Hash.ToUpper() -ne "%INSTALLER_MD5%") {
    Write-Host "[!] SHA256 校验失败，下载的文件可能已损坏，请重试" -ForegroundColor Yellow

    $x = Read-Host "  是否仍要尝试运行安装程序？(输入 y 回车继续, 其他退出)"
    if ($x -ne "y") {
        Cleanup "[-] 安装已取消, 请手动下载安装包并运行"
        return
    }
}

Write-Output "[*] 正在启动安装程序，请不要关闭此窗口，完成安装后将自动删除临时文件"
$ps = Start-Process -FilePath $pkgPath -PassThru
if ($null -eq $ps) {
    Cleanup "[!] 安装程序启动失败, 请手动下载安装包并运行"
    return
}
while (-not $ps.HasExited) {
    Start-Sleep -Seconds 1
    $ps.Refresh()
}

Cleanup
