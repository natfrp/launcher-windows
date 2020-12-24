$version = Get-Content SakuraLibrary\AssemblyBase.cs | Select-String "AssemblyVersion\(`"(.+)`"\)";
$version = $version.Matches[0].Groups[1].Value;

function DeployVariation {
    param (
        $Variation
    )

    $target = "${Variation}_v$version";
    
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

    Compress-Archive -Force -CompressionLevel Optimal -Path $target -DestinationPath "$Variation.zip";

    Remove-Item -Path "$target\frpc.*", "$target\Updater.*" -ErrorAction SilentlyContinue;
    Compress-Archive -Force -CompressionLevel Optimal -Path "$target\*" -DestinationPath "$($Variation -replace "Launcher", "Update").zip";
}

Set-Location _publish;

DeployVariation SakuraLauncher
DeployVariation LegacyLauncher

# Create frpc update package
New-Item -Name frpc -ItemType "directory" -ErrorAction SilentlyContinue;
Copy-Item "sign\frpc_windows_386.exe" "frpc\frpc.exe";
openssl dgst -sign $env:SAKURA_SIGN_KEY -sha256 -out "frpc\frpc.exe.sig" "frpc\frpc.exe";
Compress-Archive -Force -CompressionLevel Optimal -Path "frpc\*" -DestinationPath "frpc_windows_386.zip";
Remove-Item "frpc" -Recurse;
