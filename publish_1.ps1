$release_dir = Resolve-Path "bin\Release";

$version = Get-Content SakuraLibrary\AssemblyBase.cs | Select-String "AssemblyVersion\(`"(.+)`"\)";
$version = $version.Matches[0].Groups[1].Value;

function PrepareDir {
    param (
        $Dir
    )

    Remove-Item -Path $Dir -Recurse -ErrorAction SilentlyContinue;
    New-Item -Name $Dir -ItemType "directory" -ErrorAction SilentlyContinue;
}

function PrepareVariation {
    param (
        $Variation
    )

    $target = "${Variation}_v$version";
    
    PrepareDir $target;
    Get-ChildItem -Path $release_dir | Where-Object {
        $_.Name -eq "$Variation.exe.config" -or (
            $_.Extension -match "\.(dll)$" -and
            $_.Name -ne "SakuraLibrary.dll" -and
            $_.Name -ne "libsodium-64.dll"
        ) -or (
            $_.Extension -match "\.(config)$" -and
            $_.Name -notmatch "Launcher\.exe\.config$"
        )
    } | Copy-Item -Destination $target;
}

New-Item -Name _publish -ItemType "directory" -ErrorAction SilentlyContinue;
Set-Location _publish;

# Prepare 2 variations
PrepareVariation SakuraLauncher
PrepareVariation LegacyLauncher

# Prepare files to be signed
PrepareDir "sign";
Get-ChildItem -Path $release_dir | Where-Object {
    $_.Extension -match "\.exe$" -or
    $_.Name -eq "SakuraLibrary.dll"
} | Copy-Item -Destination sign;

Copy-Item "frpc_*.exe" sign -ErrorAction SilentlyContinue;

Compress-Archive -Force -CompressionLevel Optimal -Path sign -DestinationPath "sign_$version.zip";
