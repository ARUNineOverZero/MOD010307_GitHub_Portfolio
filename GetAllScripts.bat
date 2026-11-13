$root = "./MobileGameProject/Assets"
if (-not (Test-Path $root)) { Write-Error "Not found: $root"; return }
Get-ChildItem $root -Recurse -Filter *.cs | Sort-Object FullName | Group-Object DirectoryName | ForEach-Object {
  "`n### " + ((Resolve-Path -Relative $_.Name) -replace '\\','/')
  $_.Group | ForEach-Object {
    $p = (Resolve-Path -Relative $_.FullName) -replace '\\','/' -replace ' ','%20'
    "- [$($_.BaseName)]($p)"
  }
} | Out-File -FilePath script-links.md -Encoding utf8

Write-Host "Done! Open script-links.md"

Read-Host "Press Enter to close"