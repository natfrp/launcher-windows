$version = Get-Content "SakuraLibrary\Consts.cs" | Select-String "Version = `"(.+)`";"
$version = $version.Matches[0].Groups[1].Value

$projects = "SakuraLibrary", "SakuraLauncher", "LegacyLauncher", "SakuraFrpService"
$libraryFiles = Get-ChildItem "SakuraLibrary\bin\Release" | ForEach-Object { $_.Name }

Remove-Item -Path "_publish" -Recurse
New-Item -Name _publish -ItemType "directory" -ErrorAction SilentlyContinue

try {
    Push-Location -Path "_publish"
    
    Remove-Item -Path "sign" -Recurse -ErrorAction SilentlyContinue
    New-Item -Name "sign" -ItemType "directory" -ErrorAction SilentlyContinue

    foreach ($name in $projects) {
        Remove-Item -Path $name -Recurse -ErrorAction SilentlyContinue
        New-Item -Name $name -ItemType "directory" -ErrorAction SilentlyContinue
    
        Get-ChildItem -Path "..\$name\bin\Release" | Where-Object {
            $_.Name -notmatch "\.(xml|pdb)$" -and (
                $name -eq "SakuraLibrary" -or $libraryFiles -notcontains $_.Name
            )
        } | Copy-Item -Recurse -Destination $name
    
        Move-Item -Path "$name\$name.exe", "$name\$name.dll" -Destination "sign" -ErrorAction SilentlyContinue
    }

    Copy-Item "..\bin\frpc_windows_*" "sign"

    Compress-Archive -Force -CompressionLevel Optimal -Path "sign\*" -DestinationPath "sign_$version.zip"
}
finally {
    Pop-Location
}
