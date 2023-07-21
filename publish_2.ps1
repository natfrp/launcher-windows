$version = Get-Content "SakuraLibrary\Consts.cs" | Select-String "Version = `"(.+)`";"
$version = $version.Matches[0].Groups[1].Value

$projects = "SakuraLibrary", "SakuraLauncher", "LegacyLauncher"

function Sign {
    param (
        $File
    )
    openssl dgst -sign $env:SAKURA_SIGN_KEY -sha256 -out "$File.sig" $File
}

function PackLauncher {
    param (
        $Variation,
        [String[]]$Files
    )
    New-Item -Name "launcher" -ItemType "directory" -ErrorAction SilentlyContinue

    Copy-Item -Recurse "SakuraLibrary\*" "launcher"
    Copy-Item -Recurse "$Variation\*" "launcher"

    $target = $Variation -replace "Launcher", "Update"
    Compress-Archive -Force -CompressionLevel Optimal -Path "launcher\*" -DestinationPath "$target.zip"
    Remove-Item "launcher" -Recurse
}

function PackFrpc {
    param (
        $Architecture
    )
    New-Item -Name "frpc" -ItemType "directory" -ErrorAction SilentlyContinue

    Copy-Item "sign\frpc_windows_${Architecture}_gui.exe" "frpc\frpc.exe"
    Copy-Item "sign\frpc_windows_${Architecture}_gui.exe.sig" "frpc\frpc.exe.sig"

    Compress-Archive -Force -CompressionLevel Optimal -Path "frpc\*" -DestinationPath "frpc_windows_$Architecture.zip"
    Remove-Item "frpc" -Recurse
}

try {
    Push-Location -Path "_publish"

    foreach ($i in Get-ChildItem -Path "sign") {
        if ($projects -contains $i.BaseName) {
            Move-Item -Path $i.FullName -Destination $i.BaseName
            Sign "$($i.BaseName)\$($i.Name)"
        }
        if ($i -match "^frpc_.+_gui\.exe$" -or $i -match "^SakuraFrpService_.+\.exe$") {
            Sign $i.FullName
        }
    }

    ISCC "..\setup.iss"

    # Create launcher update package
    PackLauncher "SakuraLauncher"
    PackLauncher "LegacyLauncher"

    # Create frpc update package
    PackFrpc "386"
    PackFrpc "amd64"
    PackFrpc "arm64"
}
finally {
    Pop-Location
}
