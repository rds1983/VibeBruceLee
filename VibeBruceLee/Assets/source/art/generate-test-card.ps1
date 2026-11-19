[CmdletBinding()]
param(
    [string]$OutputPath = (Join-Path $PSScriptRoot '..\..\art\test-card.png')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$outputDirectory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

$width = 64
$height = 48
$bitmap = New-Object System.Drawing.Bitmap($width, $height)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)

$yellow = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 216, 0))
$cyan = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 0, 216, 216))
$magenta = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 216, 0, 216))
$white = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 255, 255))

try {
    $graphics.Clear([System.Drawing.Color]::Black)
    $graphics.FillRectangle($yellow, 0, 0, $width, 2)
    $graphics.FillRectangle($yellow, 0, $height - 2, $width, 2)
    $graphics.FillRectangle($yellow, 0, 0, 2, $height)
    $graphics.FillRectangle($yellow, $width - 2, 0, 2, $height)

    for ($y = 8; $y -lt 40; $y += 8) {
        for ($x = 8; $x -lt 56; $x += 8) {
            $brush = if ((($x / 8) + ($y / 8)) % 2 -eq 0) { $cyan } else { $magenta }
            $graphics.FillRectangle($brush, $x, $y, 8, 8)
        }
    }

    $graphics.FillRectangle($white, 24, 16, 16, 16)
    $graphics.FillRectangle($cyan, 28, 20, 8, 8)

    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
}
finally {
    $yellow.Dispose()
    $cyan.Dispose()
    $magenta.Dispose()
    $white.Dispose()
    $graphics.Dispose()
    $bitmap.Dispose()
}

Write-Host "Wrote $OutputPath"
