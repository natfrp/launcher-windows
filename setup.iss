#define AppName "SakuraFrp 启动器"
#define AppVersion ""
#define RealVersion GetVersionNumbersString("_publish\SakuraLibrary\SakuraLibrary.dll")

#define MainExecutable "SakuraLauncher.exe"

#define LibraryNameNet ".NET Framework 4.8"

[Setup]
; Basics
AppId=SakuraFrpLauncher
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} v{#RealVersion}
AppCopyright=Copyright © iDea Leaper 2020-2023

AppPublisher=SakuraFrp
AppPublisherURL=https://www.natfrp.com/
AppSupportURL=https://www.natfrp.com/

VersionInfoVersion={#RealVersion}

; Wizard
WizardStyle=modern
LicenseFile=LICENSE
ShowComponentSizes=yes
AlwaysShowDirOnReadyPage=yes
UninstallDisplayName={#AppName}
ArchitecturesAllowed=x86 x64 arm64
ArchitecturesInstallIn64BitMode=x64 arm64

DefaultDirName={autopf}\SakuraFrpLauncher
DefaultGroupName={#AppName}

DisableDirPage=yes
DisableProgramGroupPage=yes

; Output
OutputDir=bin
OutputBaseFilename=SakuraLauncher

; Compression
Compression=lzma2/ultra64
SolidCompression=yes
LZMANumBlockThreads=32
LZMAUseSeparateProcess=yes

[Languages]
Name: "ChineseSimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Types]
Name: "default"; Description: "默认设置";
Name: "custom"; Description: "自定义"; Flags: iscustom;

[Components]
Name: "frpc"; Description: "frpc"; Types: default custom; Flags: fixed
Name: "frpc\x86"; Description: "frpc (32 位)"; Check: IsX86; Types: default custom; Flags: exclusive fixed
Name: "frpc\x64"; Description: "frpc (64 位)"; Check: IsX64; Types: default custom; Flags: exclusive fixed
Name: "frpc\arm64"; Description: "frpc (ARM64, 实验性)"; Check: IsARM64; Types: default custom; Flags: exclusive fixed

Name: "launcher"; Description: "核心服务"; Types: default custom; Flags: fixed
Name: "launcher\x86"; Description: "核心服务 (32 位)"; Check: IsX86; Types: default custom; Flags: exclusive fixed
Name: "launcher\x64"; Description: "核心服务 (64 位)"; Check: IsX64; Types: default custom; Flags: exclusive fixed
Name: "launcher\arm64"; Description: "核心服务 (ARM64, 实验性)"; Check: IsARM64; Types: default custom; Flags: exclusive fixed
Name: "launcher\service"; Description: "安装为系统服务"; Flags: dontinheritcheck
Name: "launcher\service\webui"; Description: "初始化 Web UI (仅限高级用户)"; Flags: dontinheritcheck

Name: "launcher_ui"; Description: "用户界面"; Types: default custom
Name: "launcher_ui\wpf"; Description: "WPF 界面"; Types: default; Flags: exclusive
Name: "launcher_ui\legacy"; Description: "传统界面 (不推荐)"; Types: custom; Flags: exclusive

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Components: "launcher_ui"; Flags: checkedonce

[Files]
Source: "_publish\sign\frpc_windows_386_gui.exe"; DestDir: "{app}"; DestName: "frpc.exe"; Flags: ignoreversion; Components: "frpc\x86"
Source: "_publish\sign\frpc_windows_386_gui.exe.sig"; DestDir: "{app}"; DestName: "frpc.exe.sig"; Flags: ignoreversion; Components: "frpc\x86"
Source: "_publish\sign\frpc_windows_amd64_gui.exe"; DestDir: "{app}"; DestName: "frpc.exe"; Flags: ignoreversion; Components: "frpc\x64"
Source: "_publish\sign\frpc_windows_amd64_gui.exe.sig"; DestDir: "{app}"; DestName: "frpc.exe.sig"; Flags: ignoreversion; Components: "frpc\x64"
Source: "_publish\sign\frpc_windows_arm64_gui.exe"; DestDir: "{app}"; DestName: "frpc.exe"; Flags: ignoreversion; Components: "frpc\arm64"
Source: "_publish\sign\frpc_windows_arm64_gui.exe.sig"; DestDir: "{app}"; DestName: "frpc.exe.sig"; Flags: ignoreversion; Components: "frpc\arm64"

Source: "_publish\sign\SakuraFrpService_386.exe"; DestDir: "{app}"; DestName: "SakuraFrpService.exe"; Flags: ignoreversion; Components: "launcher\x86"
Source: "_publish\sign\SakuraFrpService_386.exe.sig"; DestDir: "{app}"; DestName: "SakuraFrpService.exe.sig"; Flags: ignoreversion; Components: "launcher\x86"
Source: "_publish\sign\SakuraFrpService_amd64.exe"; DestDir: "{app}"; DestName: "SakuraFrpService.exe"; Flags: ignoreversion; Components: "launcher\x64"
Source: "_publish\sign\SakuraFrpService_amd64.exe.sig"; DestDir: "{app}"; DestName: "SakuraFrpService.exe.sig"; Flags: ignoreversion; Components: "launcher\x64"
Source: "_publish\sign\SakuraFrpService_arm64.exe"; DestDir: "{app}"; DestName: "SakuraFrpService.exe"; Flags: ignoreversion; Components: "launcher\arm64"
Source: "_publish\sign\SakuraFrpService_arm64.exe.sig"; DestDir: "{app}"; DestName: "SakuraFrpService.exe.sig"; Flags: ignoreversion; Components: "launcher\arm64"

Source: "_publish\SakuraLibrary\*"; DestDir: "{app}"; Flags: ignoreversion; Components: "launcher_ui"
Source: "_publish\SakuraLauncher\*"; DestDir: "{app}"; Flags: ignoreversion; Components: "launcher_ui\wpf"
Source: "_publish\LegacyLauncher\*"; DestDir: "{app}"; Flags: ignoreversion; Components: "launcher_ui\legacy"

[Icons]
; Start Menu
Name: "{group}\{#AppName}"; Filename: "{app}\SakuraLauncher.exe"; Components: "launcher_ui\wpf"
Name: "{group}\{#AppName}"; Filename: "{app}\LegacyLauncher.exe"; Components: "launcher_ui\legacy"

Name: "{group}\访问 SakuraFrp 管理面板"; Filename: "https://www.natfrp.com/user/"

Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"

; Desktop Icon
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\SakuraLauncher.exe"; Components: "launcher_ui\wpf"; Tasks: "desktopicon"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\LegacyLauncher.exe"; Components: "launcher_ui\legacy"; Tasks: "desktopicon"

[Run]
; Service
Filename: "{app}\SakuraFrpService.exe"; Parameters: "--install"; StatusMsg: "正在安装系统服务..."; Components: "launcher\service"

; Post Install Actions
Filename: "{app}\SakuraLauncher.exe"; Description: "{cm:LaunchProgram,{#AppName}}"; Components: "launcher_ui\wpf"; Flags: nowait postinstall skipifsilent
Filename: "{app}\LegacyLauncher.exe"; Description: "{cm:LaunchProgram,{#AppName}}"; Components: "launcher_ui\legacy"; Flags: nowait postinstall skipifsilent

; WebUI
Filename: "{app}\SakuraFrpService.exe"; Description: "初始化 Web UI"; Components: "launcher\service\webui"; Flags: postinstall; Parameters: "--init-webui"
Filename: "{sys}\sc.exe"; Description: "启动系统服务"; Components: "launcher\service"; Flags: postinstall runhidden; Parameters: "start SakuraFrpService"

[UninstallRun]
Filename: "{sys}\sc.exe"; Parameters: "stop SakuraFrpService"; RunOnceId: "RemoveService-Stop"; Flags: runhidden
Filename: "{app}\SakuraFrpService.exe"; Parameters: "--uninstall"; RunOnceId: "RemoveService-Uninstall"

[UninstallDelete]
; 2.0 service installation logs
Type: files; Name: "{app}\InstallUtil.InstallLog"
Type: files; Name: "{app}\SakuraFrpService.InstallLog"

; Possible update leftovers
Type: files; Name: "{app}\*.del"

Type: dirifempty; Name: "{app}"

[Code]
var
	requiresRestart: Boolean;
	downloadPage: TDownloadWizardPage;

	installNet: Boolean;

function TryInstall(const Name, File, Args: String): String;
var
	resultCode: Integer;
	outputPage: TOutputProgressWizardPage;
begin
	outputPage := CreateOutputProgressPage('安装运行环境', '正在安装 ' + Name);
	outputPage.ProgressBar.Style := npbstMarquee;
	outputPage.ProgressBar.Visible := True;
	outputPage.Show;

	if not Exec(ExpandConstant('{tmp}\' + File), Args, '', SW_SHOW, ewWaitUntilTerminated, resultCode) then
	begin
		Result := Name + ' 安装失败: ' + SysErrorMessage(resultCode);
	end else begin
		case resultCode of
			0: ;
			1641, 3010: requiresRestart := True;
			else begin
				Result := Name + ' 安装失败: 错误代码 ' + IntToStr(resultCode);
			end;
		end;
	end;

	outputPage.Hide;
end;

//// Install Events ///////////////////////////////////////////

procedure InitializeWizard;
var
	version: Cardinal;
begin
	downloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), nil);

	installNet := (not RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', version)) or (version < 528040);
end;

function UpdateReadyMemo(const Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
begin
	Result := '';

	if installNet and WizardIsComponentSelected('launcher_ui') then
		Result := Result + '运行环境 (需要联网下载):' + Newline + Space + '{#LibraryNameNet}' + Newline + NewLine;

	if MemoUserInfoInfo <> '' then
		Result := Result + MemoUserInfoInfo + Newline + NewLine;

	if MemoDirInfo <> '' then
		Result := Result + MemoDirInfo + Newline + NewLine;

	if MemoTypeInfo <> '' then
		Result := Result + MemoTypeInfo + Newline + NewLine;

	if MemoComponentsInfo <> '' then
		Result := Result + MemoComponentsInfo + Newline + NewLine;

	if MemoGroupInfo <> '' then
		Result := Result + MemoGroupInfo + Newline + NewLine;

	if MemoTasksInfo <> '' then
		Result := Result + MemoTasksInfo + Newline + NewLine;
end;

function NextButtonClick(const CurPageID: Integer): Boolean;
var
	retry: Boolean;
begin
	Result := True;
	if (CurPageID = wpSelectComponents) then begin
		if not WizardIsComponentSelected('launcher_ui') then begin
			if WizardIsComponentSelected('launcher\service\webui') then
				Result := SuppressibleMsgBox('您选择了不使用原生界面、只启用 Web UI, Web UI 仅推荐高级用户使用'+#13#10+'请确认您理解此选择的含义和 Web UI 的配置方法, 否则请勾选一个 "用户界面"', mbError, MB_OKCANCEL, IDCANCEL) = IDOK
			else begin
				SuppressibleMsgBox('请至少选择一个用户界面', mbError, MB_OK, IDOK);
				Result := False;
			end;
		end;
	end else if (CurPageID = wpReady) and installNet and WizardIsComponentSelected('launcher_ui') then begin
		try
			downloadPage.Show;
			downloadPage.Clear;

			downloadPage.Add('https://download.microsoft.com/download/6/e/4/6e483240-dd87-40cd-adf4-0c47f5695b49/NDP481-Web.exe', 'dotnet.exe', 'a9e29f446af0db54a4f20de0749db25907fde06999c2e25e9eca52528dce3142');

			retry := True;
			while retry do begin
				retry := False;
				try
					downloadPage.Download;
				except
					if GetExceptionMessage = SetupMessage(msgErrorDownloadAborted) then
						Result := False
					else case SuppressibleMsgBox(AddPeriod(GetExceptionMessage), mbError, MB_RETRYCANCEL, IDCANCEL) of
						IDRETRY: retry := True;
						IDCANCEL: Result := False;
					end;
				end;
			end;
		finally
			downloadPage.Hide;
		end;
	end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
	if installNet then
		Result := TryInstall('{#LibraryNameNet}', 'dotnet.exe', '/passive /norestart /showrmui /showfinalerror');
end;

function NeedRestart(): Boolean;
begin
	Result := requiresRestart;
end;

//// Uninstall Events ///////////////////////////////////////////

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
	if (CurUninstallStep = usPostUninstall) and (MsgBox('是否删除启动器配置文件? 下列文件夹将会被删除: '
		+#13#10+'    %AppData%\SakuraLauncher'
		+#13#10+'    %AppData%\LegacyLauncher'
		+#13#10+'    %AppData%\SakuraFrpService'
		+#13#10+'    %ProgramData%\SakuraFrpService', mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES) then
	begin
		DelTree(ExpandConstant('{userappdata}\SakuraLauncher'), True, True, True);
		DelTree(ExpandConstant('{userappdata}\LegacyLauncher'), True, True, True);
		DelTree(ExpandConstant('{userappdata}\SakuraFrpService'), True, True, True);
		DelTree(ExpandConstant('{commonappdata}\SakuraFrpService'), True, True, True);
	end;
end;
