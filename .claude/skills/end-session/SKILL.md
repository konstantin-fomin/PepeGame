---
name: end-session
description: End-of-session workflow for PepeIdle. Invoke when the user says the session is ending. Update project memory if needed, append the daily log in K:\OBSIDIAN\Projects\PepeIdle\Daily\, commit meaningful project changes in K:\Downloads\PepeIdle\PepeIdle, then report.
---

# End-of-Session Workflow

When invoked, execute the steps below in order. Do not fabricate progress.

---

## Step 1 — Review what actually changed

Check the real project state before writing anything:

- `git -C "K:/Downloads/PepeIdle/PepeIdle" status --short`
- `git -C "K:/Downloads/PepeIdle/PepeIdle" log --oneline -10`
- Review the files changed in this session

Use repo reality over memory if they differ.

---

## Step 2 — Update project memory only if needed

### CONTEXT.md
Update `K:\OBSIDIAN\Projects\PepeIdle\CONTEXT.md` if the current task, next step, stage, or active constraints changed enough that a new chat would start from the wrong place without it.

### MEMORY.md
Update `K:\OBSIDIAN\Projects\PepeIdle\MEMORY.md` by going through these four checks:

**1. Архитектурные решения** — Was any non-obvious code decision made that future-me would redo wrong without knowing it? → append to "Архитектурные решения".

**2. Паттерны разработки** — Was a technique used for the second time, or found to be the right way to do something in this project? → append to "Паттерны разработки".

**3. Tooling & Workflow** — Was anything non-obvious discovered about Unity, C#, or process? Did something require a workaround that will recur? → append to "Tooling & Workflow".

**4. Что пробовали и не сработало** — Did anything fail, get abandoned, or require a workaround? → append: what was tried, why it failed, what was done instead.

If none of the four triggered → skip MEMORY.md.

Do not overwrite existing content. Append to specific sections only.

---

## Step 3 — Append the daily log

Create or append to `K:\OBSIDIAN\Projects\PepeIdle\Daily\YYYY-MM-DD.md` using today's actual date.

If the file already exists, append a new session block. If not, create it.

Use this format:

```markdown
---
title: YYYY-MM-DD
tags:
  - daily
  - pepeidle
date: YYYY-MM-DD
---

# YYYY-MM-DD

## Session — HH:MM

### Сделано
- [what was actually done]

### Решения
- [decisions made, if any]

### Следующие шаги
- [most concrete next step]

### Файлы изменены
- `Assets/Scripts/...` — [what changed]
```

If the file already exists with frontmatter and heading, append only a new `Session` block.

---

## Step 4 — Commit meaningful project changes

Stage and commit meaningful changes in `K:\Downloads\PepeIdle\PepeIdle`:

- `git -C "K:/Downloads/PepeIdle/PepeIdle" status --short`

Commit rules:
- Imperative summary line, ≤72 chars
- Body bullets when useful
- Do not commit if the worktree is clean
- Do not commit: `Library/`, `Temp/`, `obj/`, `Logs/`, `.vs/`
- Stage specific files, not `-A`

Template:
```text
Add CurrencyManager with click and idle income

- Double precision for large numbers
- OnCurrencyChanged event for UI binding
- Idle ticks via coroutine every 0.1s
```

If there is nothing to commit, report that and continue.

---

## Step 5 — Close out the session

Report:

```text
✓ Context updated: [yes/no — what changed]
✓ Memory updated: [yes/no — what changed]
✓ Daily log: K:\OBSIDIAN\Projects\PepeIdle\Daily\YYYY-MM-DD.md
✓ Commit: [hash or "none"] — [message or reason]
```
