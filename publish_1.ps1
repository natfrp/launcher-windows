$compile_version = "bin\Release\net48"

$version = Get-Content "SakuraLibrary\Consts.cs" | Select-String "Version = `"(.+)`";"
$version = $version.Matches[0].Groups[1].Value

$projects = "SakuraLibrary", "SakuraLauncher", "LegacyLauncher"
$libraryFiles = Get-ChildItem "SakuraLibrary\$compile_version" | ForEach-Object { $_.Name }

Remove-Item -Path "_publish" -Recurse -ErrorAction Stop
New-Item -Name "_publish" -ItemType "directory"

try {
    Push-Location -Path "_publish"

    New-Item -Name "sign" -ItemType "directory"
    New-Item -Name "debug" -ItemType "directory"

    Copy-Item ..\..\launcher\release\SakuraFrpService_*.exe -Destination "sign"

    foreach ($name in $projects) {
        Remove-Item -Path $name -Recurse -ErrorAction SilentlyContinue
        New-Item -Name $name -ItemType "directory" -ErrorAction SilentlyContinue

        Get-ChildItem -Path "..\$name\$compile_version" | Where-Object {
            $_.Name -notmatch "\.(xml|pdb)$" -and (
                $name -eq "SakuraLibrary" -or $libraryFiles -notcontains $_.Name
            )
        } | Copy-Item -Recurse -Destination $name

        Get-ChildItem -Path "..\$name\$compile_version" | Where-Object {
            $_.Name -match "\.pdb$"
        } | Copy-Item -Destination "debug"

        Move-Item -Path "$name\$name.exe", "$name\$name.dll" -Destination "sign" -ErrorAction SilentlyContinue
    }

    Compress-Archive -Force -CompressionLevel Optimal -Path "debug\*" -DestinationPath "DebugSymbols.zip"
    Compress-Archive -Force -CompressionLevel Optimal -Path "sign\*" -DestinationPath "sign_$version.zip"

    Remove-Item "sign", "debug" -Recurse
    New-Item -Name "sign" -ItemType "directory"

    Copy-Item ..\..\frp\release\frpc_windows_*_gui.exe -Destination "sign"
} finally {
    Pop-Location
}
