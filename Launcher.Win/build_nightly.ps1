param (
    [Parameter(Mandatory = $true)][string]$buildNumber
)

$version = "1.0.0.$buildNumber"

# Find vsvarsall and run it, then inherit all variables
$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
$visualStudioPath = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
"VisualStudio Path: $visualStudioPath"

$baseUrl = "https://nightlies.opentemple.de/windows";

if ($visualStudioPath)
{
    $path = join-path $visualStudioPath 'Common7\Tools\VsDevCmd.bat'
    if (test-path $path)
    {
        cmd /s /c """$path"" $args && set" | where {
            $_ -match '(\w+)=(.*)'
        } | foreach {
            $null = new-item -force -path "Env:\$( $Matches[1] )" -value $Matches[2]
        }
    }
}

# find the appropriate version of the Visual C++ libs by taking the first
$msvcRedistBaseDir = "$visualStudioPath\VC\Redist\MSVC"
$msvcRedistVersion = Get-ChildItem -Directory $msvcRedistBaseDir -Filter 14.* | Sort-Object -Property { $_.Name } -Descending | Select-Object -first 1
"Latest MSVC Redist Version: $msvcRedistVersion"

[string[]]$platforms = "x64", "x86"

$root = [IO.Path]::Combine($PSScriptRoot, "..")

[string]$manifestTemplate = [IO.Path]::Combine($PSScriptRoot, "MsixPackage/AppxManifest.xml")
[string]$appInstallerTemplate = [IO.Path]::Combine($PSScriptRoot, "MsixPackage/OpenTemple.appinstaller")

function SignFile ([string] $path) {
    if (-not (Test-Path 'env:CODE_SIGNING_PFX') -or -not (Test-Path 'env:CODE_SIGNING_PW'))
    {
        "Skipping signing step because CODE_SIGNING_PFX or CODE_SIGNING_PW is not defined"
        return
    }

    $decodedKey = [System.Convert]::FromBase64String($env:CODE_SIGNING_PFX)
    [io.file]::WriteAllBytes('OpenTemple.pfx', $decodedKey)

    SignTool sign /fd SHA256 /a /f OpenTemple.pfx /p $env:CODE_SIGNING_PW $path

    Remove-Item "OpenTemple.pfx"
}

Push-Location $root

$zipPlatforms = @{};
$msixPlatforms = @{};
$buildInfo = @{
    "buildNumber" = $buildNumber;
    "commitId" = "$env:GITHUB_SHA";
    "buildTime" = (Get-Date).ToUniversalTime();
};

try
{
    Remove-Item -Recurse dist -ErrorAction Ignore
    mkdir dist/windows
    $bundleMapping = "[Files]`n";

    #
    # Publish the desired platforms
    #
    foreach ($platform in $platforms)
    {
        Remove-Item -Recurse dist/$platform -ErrorAction Ignore

        #
        # This will actually build / publish the project for the respective platform!
        #
        dotnet publish -c Release -o dist/$platform -r win-$platform Launcher.Win

        #
        # Copy over the VC++ runtime files needed by the platform (which differs by platform)
        #
        switch ($platform)
        {
            "x86" {
                $redistDir = "$msvcRedistVersion\x86\Microsoft.VC143.CRT"
                $redistFiles = "msvcp140.dll", "msvcp140_1.dll", "msvcp140_2.dll", "msvcp140_codecvt_ids.dll", "vcruntime140.dll"
            }
            "x64" {
                $redistDir = "$msvcRedistVersion\x64\Microsoft.VC143.CRT"
                $redistFiles = "msvcp140.dll", "msvcp140_1.dll", "msvcp140_2.dll", "msvcp140_codecvt_ids.dll", "vcruntime140.dll", "vcruntime140_1.dll"
            }
            Default {
                throw "Can't handle VC++ redistributable for platform $platform"
            }
        }
        foreach ($file in $redistFiles)
        {
            Copy-Item "$redistDir\$file" dist/$platform
        }

        #
        # For debugging, also add a build.json to the folder
        #
        $buildInfo | ConvertTo-Json -Depth 100 | Out-File -Encoding UTF8NoBOM "$root/dist/$platform/build.json"

        #
        # Update the AppManifest's version and platform
        #
        [xml]$xmlDoc = Get-Content $manifestTemplate
        $xmlDoc.Package.Identity.ProcessorArchitecture = $platform
        $xmlDoc.Package.Identity.Version = $version

        # Write out the App manifest into the build directory
        $xmlDoc.Save("$root/dist/$platform/AppxManifest.xml")

        # Copy over the images
        $images = [IO.Path]::Combine($PSScriptRoot, "MsixPackage/Images")
        Copy-Item -Recurse $images "$root/dist/$platform/Images"

        # Update PRI stuff
        makepri new /of "$root/dist/$platform/resources.pri" /pr "$root/dist/$platform" /cf "$PSScriptRoot\MsixPackage\priconfig.xml"

        $msixOutPath = "dist/windows/OpenTemple_$platform.msix"

        ### Make the MSIX package
        makeappx pack /d dist/$platform /p $msixOutPath

        SignFile $msixOutPath

        $bundleMapping += """$msixOutPath"" ""OpenTemple_$platform.msix""`n";

        # Write out a record for the website
        $msixPlatforms[$platform] = @{
            "url" = "$baseUrl/OpenTemple_$platform.msix";
            "sizeInBytes" = (Get-Item $msixOutPath).Length;
            "sha256Hash" = (Get-FileHash $msixOutPath -Algorithm SHA256).Hash;
        };

        ### Make the ZIP package
        $zipOutPath = "dist/windows/OpenTemple_$platform.zip"
        Compress-Archive -Path "dist\$platform\*" -DestinationPath $zipOutPath
        $zipPlatforms[$platform] = @{
            "url" = "$baseUrl/OpenTemple_$platform.zip";
            "sizeInBytes" = (Get-Item $zipOutPath).Length;
            "sha256Hash" = (Get-FileHash $zipOutPath -Algorithm SHA256).Hash;
        };
    }

    # Make the MSIX bundle
    # NOTE: This is a flat bundle since we are sideloading and aren't restricted by the app store
    Set-Content -Path dist/MsixBundleMapping.txt -Value $bundleMapping
    makeappx bundle /fb /f dist/MsixBundleMapping.txt /bv $version /p dist/windows/OpenTemple.msixbundle

    SignFile dist/windows/OpenTemple.msixbundle

    #
    # Create an appinstaller file from the template and fill out the version number
    #
    [xml]$xmlDoc = Get-Content $appInstallerTemplate
    $xmlDoc.AppInstaller.Version = $version
    $xmlDoc.AppInstaller.MainBundle.Version = $version
    $xmlDoc.Save("$root/dist/windows/OpenTemple.appinstaller")

    #
    # Also write out a JSON file to be used by the static site generator
    #
    $jsonInfo = @{
        "buildInfo" = $buildInfo;
        "msix" = @{
            "appInstallerUrl" = "$baseUrl/OpenTemple.appinstaller";
            "platforms" = $msixPlatforms;
        };
        "zip" = $zipPlatforms
    }
    $jsonInfo | ConvertTo-Json -Depth 100 | Out-File -Encoding UTF8NoBOM "$root/dist/windows/info.json"

}
finally
{
    Pop-Location
}
