Function Info($msg) {
  Write-Host -ForegroundColor DarkGreen "`nINFO: $msg`n"
}

Function Error($msg) {
  Write-Host -ForegroundColor Red "`n`nERROR: $msg"
  exit 1
}

Function CheckReturnCodeOfPreviousCommand($msg) {
  if(-Not $?) {
    Error "${msg}. Error code: $LastExitCode"
  }
}

Function GetVersion() {
  try { $commitTag = git describe --exact-match --tags HEAD 2> $null } catch { }
  if(-Not $?) {
    Info "The commit is not tagged. Use 'v0.0-dev' as a tag instead"
    $commitTag = "v0.0-dev"
  }
  return "$($commitTag.Substring(1))"
}

Function GetInstallerVersion($version) {
  return $version.Split("-")[0];
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$root = Resolve-Path "$PSScriptRoot"
$buildDir = "$root/build"
$version = GetVersion
$installerVersion = GetInstallerVersion $version

Info "Version: '$version'. InstallerVersion: '$installerVersion'"

Info "Run tests"
dotnet test --nologo `
            --configuration Release `
            -verbosity:minimal `
            --logger:"console;verbosity=normal" `
            $root/src/Test/Test.csproj
CheckReturnCodeOfPreviousCommand "tests failed"

Info "Publish"
dotnet publish --nologo `
               --configuration Release `
               --runtime win-x64 `
               -verbosity:minimal `
               /property:Platform=x64 `
               /property:PublishAot=true `
               /property:PublishTrimmed=true `
               /property:TrimMode=full `
               /property:DebugType=None `
               /property:Version=$version `
               $root/src/App/ShowWhatProcessLocksFile.csproj
CheckReturnCodeOfPreviousCommand "publish failed"

Info "Build installer"
dotnet build --nologo `
             --configuration Release `
             -verbosity:minimal `
             /property:Platform=x64 `
             /property:DebugType=None `
             /property:InstallerVersion=$installerVersion `
             $root/src/Installer/Installer.wixproj
CheckReturnCodeOfPreviousCommand "installer build failed"

Info "Create zip archive from msi installer"
Compress-Archive -Force -Path "$buildDir/x64/Release/Installer/ShowWhatProcessLocksFile.msi" -DestinationPath "$buildDir/ShowWhatProcessLocksFile.zip"
