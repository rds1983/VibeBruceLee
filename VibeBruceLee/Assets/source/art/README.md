# Asset loading pipeline

This folder holds the source for the T-004 test card. The exported PNG lives in `../../art/`.

Regenerate from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File VibeBruceLee/Assets/source/art/generate-test-card.ps1
```

The script uses `System.Drawing` on Windows and produces a 64x48 PNG with no antialiasing. Replace this bootstrap pipeline with the real asset toolchain in T-005.
