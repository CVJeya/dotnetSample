# Run from TaskManagementUI folder: .\clean-install.ps1
# Deletes node_modules + package-lock so npm cannot mix Angular 21 + Angular 17 artifacts.
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

Write-Host 'Removing node_modules...'
if (Test-Path node_modules) {
  Remove-Item -Recurse -Force node_modules
}

Write-Host 'Removing package-lock.json...'
if (Test-Path package-lock.json) {
  Remove-Item -Force package-lock.json
}

Write-Host 'npm install...'
npm install

Write-Host 'Done. Run: npm start'
