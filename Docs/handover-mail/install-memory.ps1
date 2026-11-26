# 노트북 Claude 메모리 사본을 이 PC(Windows)의 Claude Code 프로젝트 메모리로 복사한다.
# 저장소 루트(2025_GAME4_TEAM-ColoSSAME-)에서 실행:
#   powershell -ExecutionPolicy Bypass -File Docs\handover-mail\install-memory.ps1
# 기존 파일은 덮어쓰지 않는다.
$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$src = Join-Path $root "Docs\handover-mail\memory"
foreach ($dir in @($root, (Join-Path $root "Main_Project"))) {
    $key = ($dir -replace '[^A-Za-z0-9]', '-')
    $dest = Join-Path $env:USERPROFILE ".claude\projects\$key\memory"
    New-Item -ItemType Directory -Force -Path $dest | Out-Null
    Get-ChildItem -Path $src -Filter *.md | ForEach-Object {
        $target = Join-Path $dest $_.Name
        if (-not (Test-Path $target)) { Copy-Item $_.FullName $target }
    }
    Write-Host "메모리 복사: $dest"
}
