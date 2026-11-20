# AGENTS.md - Agentic Development Workflow

This file tells the implementing agent how work happens in this repository. Read it at the start of every session, together with `README.md`, `TASKS.md`, and the current `Design/` docs.

## Ground rules

- **`TASKS.md` is a living document.** You are expected to split, reorder, rewrite, add, or remove tasks as you learn more. It is a map, not a contract.
- **Documentation lives in `Design/`.** Keep `README.md` short and user-facing. Design, architecture, balance, level references, and decisions belong in `Design/`.
- **All assets are original.** Create art, audio, and fonts yourself. The original game is a behavioral and visual reference only; never rip or redistribute its sprites, audio, code, or fonts.
- **Ask before installing software.** That includes editors, SDKs, CLI tools, and non-stack NuGet packages. Packages required by the declared stack (MonoGame, FontStashSharp, XNAssets, xUnit) are pre-approved. Prefer reproducible scripted asset generation over one-off manual edits.
- **Stay in scope.** Do one task at a time. Do not refactor unrelated code or commit unrelated work. If you spot something else, record it in `TASKS.md`.
- **Never fake results.** Do not claim a test, build, or playtest passed unless you actually ran it. If something is blocked, say so and mark the task `[!]` with the reason.

## Repository map

```
Design/                   Design and technical docs, plus decision records
Tools/                    Build-time tools, including the asset generator
VibeBruceLee/             Game project and source
VibeBruceLee/Assets/      Runtime assets: art, audio, fonts, levels, licenses
VibeBruceLee.Tests/       xUnit tests
VibeBruceLee.slnx         Solution
README.md                 Short project overview
TASKS.md                  Living roadmap
AGENTS.md                 This workflow
```

Useful commands:

```sh
dotnet build VibeBruceLee.slnx
dotnet test VibeBruceLee.slnx
dotnet run --project VibeBruceLee
dotnet run --project Tools/VibeBruceLee.AssetGen -- VibeBruceLee/Assets
```

## The task loop

Follow these steps for every task, in order.

1. **Get the next task.**
   Read `TASKS.md` and pick the first unblocked `[ ]` task in the current milestone, unless the developer names a different task. Read the related `Design/` docs and code first. If the task is vague or larger than a focused change, rewrite or split it before starting. Mark it `[~]` and add a one-line note about the plan. Ask the developer if the priority or scope is ambiguous.

2. **Do it.**
   Implement the smallest coherent change that satisfies the task's acceptance criteria. Keep the code consistent with the surrounding style. Update or add tests as you go. Run the game when the task affects behavior the tests cannot see.

3. **Test thoroughly.**
   - Build: `dotnet build VibeBruceLee.slnx` (no new warnings).
   - Automated: `dotnet test VibeBruceLee.slnx` with all tests green.
   - New tests: pure logic must be covered; use fixed timesteps and seeded randomness.
   - Manual: play through the affected flow, including edge cases and failure paths.
   - Assets: verify assets load from a clean output folder.
   Report the exact commands you ran and their results.

4. **Update the docs.**
   Update `Design/` in the same change: mechanics, architecture, chamber data, tuning, debug tools. Add an ADR under `Design/decisions/` for any non-obvious choice. Only touch `README.md` if the user-facing setup or status changed. Add any follow-up work you discovered to `TASKS.md`.

5. **Ask the developer whether the task is finished.**
   Do not mark the task `[x]` yet. Send a short confirmation request containing:
   - the task ID and title,
   - what changed and why,
   - how it was tested, with results,
   - which docs were updated,
   - risks, tradeoffs, and open questions,
   - a direct question: "Is this task finished? Should I mark it done and commit?"
   Wait for an answer.

6. **Only after explicit confirmation: finish the task.**
   Explicit confirmation means a clear approval such as "approved", "looks good, commit", or "yes, done". Silence, an unrelated reply, or "ok" without context is not confirmation; ask again. If the developer requests changes, keep the task `[~]`, make the changes, retest, and return to step 5.
   After confirmation:
   - mark the task `[x]` and add a row to the Done log in `TASKS.md`,
   - commit the task as one logical commit (code, tests, docs, assets together),
   - push to the remote.

## TASKS.md maintenance

- Keep IDs stable once used in a commit or doc.
- Prefer small tasks with measurable acceptance criteria over large vague ones.
- Move discovered work into the Backlog first, then promote it into a milestone.
- If a task is obsolete, remove it or rewrite it; do not leave stale instructions.
- Keep the legend, current milestone, and Done log accurate.
- A task is never "done" because code exists; it is done when tested, documented, and confirmed.

## Testing policy

- The build and full test suite must be green before asking for confirmation.
- Gameplay logic should be testable without a window: keep physics, AI, level parsing, scoring, and state machines independent of MonoGame types.
- Use deterministic time steps and seeded RNG. Flaky tests are bugs; fix or delete them.
- Manual playtest checklist for gameplay tasks:
  - controls respond and nothing sticks,
  - death, respawn, and game over work,
  - collision has no tunneling or stuck states,
  - enemies cannot stun-lock or become helpless,
  - no visual glitches at integer scales, windowed and fullscreen.
- If a test cannot run in the environment, report it explicitly; never mark the task done.

## Documentation policy

- `Design/README.md` - index of docs.
- `Design/overview.md` - pitch, scope, non-goals.
- `Design/gameplay.md` - controls, rules, scoring, frame data.
- `Design/reference.md` - facts about the original game and how we interpret them.
- `Design/architecture.md` - project structure, update loop, systems, data flow.
- `Design/chambers.md` - chamber layouts and hazards.
- `Design/assets.md` - pipeline, palette, naming, provenance, regeneration.
- `Design/testing.md` - test strategy and manual checklists.
- `Design/tuning.md` - final balance values.
- `Design/debug.md` - overlays, cheats, and dev shortcuts.
- `Design/decisions/NNNN-title.md` - architecture decision records.

Docs must match the code. If behavior and docs disagree, fix one of them in the same change.

## Git rules

- One task per commit. Suggested message: `T-021: Add chop and running kick`.
- Never commit before explicit confirmation.
- Run the build and tests again immediately before committing.
- Check `git status` and stage only files related to the task.
- Never force-push, rewrite published history, or delete work you did not create.
- If a push is rejected, rebase on the remote, rerun the tests, and push again.
- Do not commit secrets, local save files, IDE caches, or build output.

## Assets and licensing

- Keep editable sources for generated assets in the repository, plus the exported runtime files. Art and audio are generated by `Tools/VibeBruceLee.AssetGen`; change the generator, not the committed PNG/WAV files.
- Third-party fonts, sounds, or tools need a license compatible with MIT distribution. Store the license text in `VibeBruceLee/Assets/licenses/`.
- Document how to regenerate every asset in `Design/assets.md`; a clean checkout must reproduce the build.
- Do not use trademarked logos or copied cover art.

## Communication style

- Be concise and concrete. Report what you did, what you verified, and what remains.
- Ask early when requirements are ambiguous; batch related questions instead of interrupting repeatedly.
- Surface blockers and tradeoffs rather than silently working around them.
- When asking for confirmation, make the decision easy: summarize, then ask one clear question.

## Definition of Done

A task is done only when all of these are true:

- [ ] The behavior matches the task's acceptance criteria.
- [ ] The build is warning-free and all tests pass.
- [ ] New logic has tests; affected flows were manually played.
- [ ] `Design/` docs are updated and accurate.
- [ ] Follow-up work is recorded in `TASKS.md`.
- [ ] The developer explicitly confirmed completion.
- [ ] `TASKS.md` is updated and the task is committed and pushed.
