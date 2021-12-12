Function Info($msg) {
    Write-Host -ForegroundColor DarkGreen "`nINFO: $msg`n"
}

Function Error($msg) {
    Write-Host `n`n
    Write-Error $msg
    exit 1
}

Function CheckReturnCodeOfPreviousCommand($msg) {
    if(-Not $?) {
        Error "${msg}. Error code: $LastExitCode"
    }
}

Function GetVersion() {
    $gitCommand = Get-Command -Name git

    $nearestTag = & $gitCommand describe --exact-match --tags HEAD
    if(-Not $?) {
        Info "The commit is not tagged. Use 'v0.0-dev' as a version instead"
        $nearestTag = "v0.0-dev"
    }

    $commitHash = & $gitCommand rev-parse --short HEAD
    CheckReturnCodeOfPreviousCommand "Failed to get git commit hash"

    return "$($nearestTag.Substring(1))-$commitHash"
}

Function GetInstallerVersion($version) {
    return $version.Split("-")[0];
}

Function FindMsBuild() {
    $vswhereCommand = Get-Command -Name "${Env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

    $msbuild = `
        & $vswhereCommand `
            -latest `
            -requires Microsoft.Component.MSBuild `
            -find MSBuild\**\Bin\MSBuild.exe `
          | select-object -first 1

    if(!$msbuild)
    {
        Error "Can't find MsBuild"
    }

    Info "MsBuild found: `n $msbuild"
    return $msbuild
}

Function RemoveFileIfExists($fileName) {
    Info "Remove '$fileName'"
    Remove-Item $fileName  -Force  -Recurse -ErrorAction SilentlyContinue
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$root = Resolve-Path "$PSScriptRoot/../.."
$buildDir = "$root/build"
$publishDir = "$buildDir/Release"
$projectName = "ShowWhatProcessLocksFile"
$version = GetVersion
$installerVersion = GetInstallerVersion $version
$msbuild = FindMsBuild

Info "Version: '$version'. InstallerVersion: '$installerVersion'"

Info "Build project"
& $msbuild `
    /property:RestorePackagesConfig=true `
    /property:MSBuildWarningsAsMessages=NU1503 `
    /property:Configuration=Release `
    /property:DebugType=None `
    /property:Version=$version `
    /property:InstallerVersion=$installerVersion `
    /target:"restore;build" `
    $root/$projectName.sln
CheckReturnCodeOfPreviousCommand "build failed"

RemoveFileIfExists "$publishDir/${projectName}.msi.zip"
Info "Create zip archive from msi installer"
Compress-Archive -Path "$publishDir/$projectName.msi" -DestinationPath "$publishDir/${projectName}.msi.zip"

# Skip running tests if the build script is run on Github Actions.
# There are no processes which lock files on Github Actions executors. It makes a lot of test fail.
if ($null -eq $env:GITHUB_ACTIONS) {
    & "$buildDir/nuget/nunit.consolerunner/*/tools/nunit3-console.exe" `
           "$publishDir/net461/Test.dll" `
           --stoponerror `
           --labels=Before `
           --noheader `
           --noresult
    CheckReturnCodeOfPreviousCommand "tests failed"
}
