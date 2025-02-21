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

    try { $tag = & $gitCommand describe --exact-match --tags HEAD 2> $null } catch { }
    if(-Not $?) {
        Info "The commit is not tagged. Use 'v0.0-dev' as a version instead"
        $tag = "v0.0-dev"
    }

    $commitHash = & $gitCommand rev-parse --short HEAD
    CheckReturnCodeOfPreviousCommand "Failed to get git commit hash"

    return "$($tag.Substring(1))-$commitHash"
}

Function GetInstallerVersion($version) {
    return $version.Split("-")[0];
}

Function RemoveFileIfExists($fileName) {
    Info "Remove '$fileName'"
    Remove-Item $fileName  -Force  -Recurse -ErrorAction SilentlyContinue
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$root = Resolve-Path "$PSScriptRoot"
$buildDir = "$root/build"
$publishDir = "$buildDir/Release/Installer"
$version = GetVersion
$installerVersion = GetInstallerVersion $version

Info "Version: '$version'. InstallerVersion: '$installerVersion'"

Info "Build project"
dotnet build `
  --nologo `
  --configuration Release `
  -verbosity:minimal `
  /property:DebugType=None `
  /property:Version=$version `
  /property:InstallerVersion=$installerVersion `
  "$root/ShowWhatProcessLocksFile.sln"
CheckReturnCodeOfPreviousCommand "build failed"

Info "Run tests"
dotnet test `
  --nologo `
  --no-build `
  --configuration Release `
  -verbosity:minimal `
  --logger:"console;verbosity=normal" `
  $root/ShowWhatProcessLocksFile.sln
CheckReturnCodeOfPreviousCommand "tests failed"

Info "Create zip archive from msi installer"
Compress-Archive -Force -Path "$publishDir/ShowWhatProcessLocksFile.msi" -DestinationPath "$publishDir/ShowWhatProcessLocksFile.msi.zip"
