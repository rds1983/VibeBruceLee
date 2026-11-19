# Design documentation

Project documentation for VibeBruceLee. Setup and build instructions live in the root `README.md`; the roadmap lives in `TASKS.md`.

| Document | Contents |
| -------- | -------- |
| `overview.md` | Pitch, goals, scope, pillars, success criteria |
| `reference.md` | Verified facts about the 1984 game and how we interpret them |
| `gameplay.md` | Remake rules: controls, combat, scoring, progression, modes |
| `assets.md` | Asset folders, key rules, loading API, diagnostics, provenance |
| `chambers.md` | Chamber JSON format, coordinates, validation rules, samples |
| `debug.md` | Debug overlay, keys, slow motion, frame stepping, cheat flags |
| `architecture.md` | Solution layout, host, toolchain, assets, testing |
| `decisions/` | Architecture decision records |

Planned, in roadmap order:

| Document | Owner task |
| -------- | ---------- |
| `tuning.md` | T-055 |
| `testing.md` | T-060 |

Keep these documents in sync with the code. If behavior and documentation disagree, fix one of them in the same change.
