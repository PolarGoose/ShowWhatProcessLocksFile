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
$isArm64Host = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq 'Arm64'

Info "Version: '$version'. InstallerVersion: '$installerVersion'"

foreach ($platform in @('x64', 'ARM64')) {
  if ($platform -eq 'x64' -or $isArm64Host) {
    Info "Run $platform tests"
    dotnet test --nologo `
                --configuration Release `
                -verbosity:minimal `
                /property:Platform=$platform `
                --logger:"console;verbosity=normal" `
                "$root/src/Test/Test.csproj"
    CheckReturnCodeOfPreviousCommand "$platform tests failed"
  }

  $runtime = "win-$($platform.ToLowerInvariant())"

  Info "Publish $platform Native AOT executable"
  dotnet publish --nologo `
                 --configuration Release `
                 --runtime $runtime `
                 -verbosity:minimal `
                 /property:Platform=$platform `
                 /property:PublishAot=true `
                 /property:PublishTrimmed=true `
                 /property:TrimMode=full `
                 /property:DebugType=None `
                 /property:Version=$version `
                 "$root/src/App/ShowWhatProcessLocksFile.csproj"
  CheckReturnCodeOfPreviousCommand "$platform publish failed"

  Info "Build $platform installer"
  dotnet build --nologo `
               --configuration Release `
               -verbosity:minimal `
               /property:Platform=$platform `
               /property:DebugType=None `
               /property:InstallerVersion=$installerVersion `
               "$root/src/Installer/Installer.wixproj"
  CheckReturnCodeOfPreviousCommand "$platform installer build failed"

  Info "Create $platform zip archive from msi installer"
  Compress-Archive -Force -Path "$buildDir/$platform/Release/Installer/ShowWhatProcessLocksFile.msi" -DestinationPath "$buildDir/ShowWhatProcessLocksFile_$platform.zip"
}
