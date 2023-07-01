$projects = "SakuraLibrary", "SakuraLauncher", "LegacyLauncher", "SakuraFrpService"
$libraryFiles = Get-ChildItem "SakuraLibrary\bin\Release" | ForEach-Object { $_.Name }

Remove-Item -Path "_publish" -Recurse -ErrorAction SilentlyContinue
New-Item -Name "_publish" -ItemType "directory" -ErrorAction SilentlyContinue

try {
    Push-Location -Path "_publish"

    Remove-Item -Path "sign" -Recurse -ErrorAction SilentlyContinue
    New-Item -Name "sign" -ItemType "directory" -ErrorAction SilentlyContinue
    New-Item -Name "debug" -ItemType "directory" -ErrorAction SilentlyContinue

    foreach ($name in $projects) {
        Remove-Item -Path $name -Recurse -ErrorAction SilentlyContinue
        New-Item -Name $name -ItemType "directory" -ErrorAction SilentlyContinue

        Get-ChildItem -Path "..\$name\bin\Release" | Where-Object {
            $_.Name -notmatch "\.(xml|pdb)$" -and (
                $name -eq "SakuraLibrary" -or $libraryFiles -notcontains $_.Name
            )
        } | Copy-Item -Recurse -Destination $name

        Get-ChildItem -Path "..\$name\bin\Release" | Where-Object {
            $_.Name -match "\.pdb$"
        } | Copy-Item -Destination "debug"

        Move-Item -Path "$name\$name.exe", "$name\$name.dll" -Destination "sign" -ErrorAction SilentlyContinue
    }

    Compress-Archive -Force -CompressionLevel Optimal -Path "debug\*" -DestinationPath "DebugSymbols.zip"
    Compress-Archive -Force -CompressionLevel Optimal -Path "sign\*" -DestinationPath "sign.zip"
}
finally {
    Pop-Location
}
