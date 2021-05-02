Param([Parameter(Mandatory=$true)] [string] $targetFolder)

Function ExtractFileFromArchive($archiveName, $fileName, $dstDirectory) {
    Add-Type -Assembly System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($archiveName)
    if (!$zip) {
        Write-Error "Can't open '$archiveName' as a zip archive"
    }

    $handleExe = $zip.GetEntry("handle.exe")
    if (!$handleExe) {
        Write-Error "'$archiveName' zip archive doesn't contain '$fileName' file"
    }

    New-Item -Force -ItemType "directory" $dstDirectory > $null
    [IO.Compression.ZipFileExtensions]::ExtractToFile($handleExe, "$dstDirectory/$fileName")
    $zip.Dispose()
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$archive = "$targetFolder\handle.zip"
New-Item -Force -ItemType "directory" $targetFolder > $null
Write-Host "Download Handle.zip from the web into `n '$archive'"
Invoke-WebRequest `
    -Uri https://download.sysinternals.com/files/Handle.zip `
    -OutFile $archive

Write-Host "Extract 'handle.exe' from `n '$archive' into `n '$targetFolder'"
ExtractFileFromArchive $archive "handle.exe" $targetFolder

Write-Host "Remove '$archive'"
Remove-Item $archive
