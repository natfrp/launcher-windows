$version = Get-Content SakuraLibrary\AssemblyBase.cs | Select-String "AssemblyVersion\(`"(.+)`"\)";
$version = $version.Matches[0].Groups[1].Value;

function DeployVariation {
    param (
        $Variation
    )

    $target = "${Variation}_$version";
    
    $files = Get-ChildItem -Path "sign" | Where-Object {
        $_.Name -eq "$Variation.exe" -or (
            $_.Extension -match "\.(exe)$" -and
            $_.Name -notmatch "Launcher\.exe$" -and
            $_.Name -notmatch "^frpc_"
        ) -or (
            $_.Extension -match "\.(dll)$"
        )
    };
    $files | Copy-Item -Destination $target;
    $files | ForEach-Object -Process {
        openssl dgst -sign $env:SAKURA_SIGN_KEY -sha256 -out "$target\$($_.Name).sig" $_.FullName
    };

    Copy-Item "sign\frpc_windows_386.exe" "$target\frpc.exe";
    openssl dgst -sign $env:SAKURA_SIGN_KEY -sha256 -out "$target\frpc.exe.sig" "sign\frpc_windows_386.exe";
}

Set-Location _publish;

DeployVariation SakuraLauncher
DeployVariation LegacyLauncher
