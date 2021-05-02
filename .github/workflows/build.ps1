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

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$root = Resolve-Path "$PSScriptRoot/../.."
$publishDir = "$root/build/Release"
$projectName = "ShowWhatProcessLocksFile"
$version = GetVersion
$installerVersion = GetInstallerVersion $version
$msbuild = FindMsBuild

Info "Version: '$version'. InstallerVersion: '$installerVersion'"

Info "Remove Publish directory `n $publishDir"
Remove-Item $publishDir  -Force  -Recurse -ErrorAction SilentlyContinue

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

# TODO: there are no processes which lock files on Github Actions executors. It makes a lot of test fail.
# Info "Run tests"
# & "$root/build/nuget/nunit.consolerunner/*/tools/nunit3-console.exe" `
#     $publishDir/net461/Test.dll `
#     --stoponerror `
#     --noresult
# CheckReturnCodeOfPreviousCommand "tests failed"
