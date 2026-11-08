# Population Control (Timberborn 1.x)

Choose **who** leaves a district when you migrate.

The base game sends the youngest first. Its order is: willing workers, then unemployed, then homeless, then **youngest**.
That's why your 10-day-old workers kept leaving. This mod lets you change that for each district.

## Where to find it

* **District Center panel → "Migration filters"** (click *Show*).
* **Beaver or bot panel → "Never migrate"** checkbox.

## Options (per district, for the beavers *leaving* that district)

| Option | What it does |
|---|---|
| Use for manual migration | The 1 / 10 / ∞ buttons in the migration panel follow your filters. **On** by default. |
| Use for automatic migration | The game's automatic population balancing also follows your filters. **Off** by default, so vanilla auto-migration is untouched. |
| Adults / Children / Bots tabs | Each group has its own rules. |
| Send first: Youngest / Oldest | Age order (raw age in days). |
| Min age / Max age | Only citizens inside this age range may leave. "off" = no limit. Shift-click steps by 10. |
| Workers | *Unemployed first* (game default), *Ignore jobs* (pure age order), or *Never send employed*. |
| Send homeless first | Game default is on. Turn it off for a pure age order. |
| Copy to all districts | Copies this district's settings to every other district. |

**Strict filters.** If nobody matches, nobody is sent. If fewer match than you asked for (e.g. you clicked 10), only
the matching ones go, and a notice tells you how many.

**Example (your case):** in the main city's District Center, set Adults → *Send first: Oldest*, *Workers: Ignore jobs*,
*Min age: 40*. Clicking **10** now sends the ten oldest adults aged 40+ and never sends young workers.

## Safe by default

A district you haven't touched behaves exactly like the base game: the mod hands control straight back to the game's own code.
Settings are stored only for districts you changed, and pins only for pinned citizens.
Pinned citizens are skipped by manual **and** automatic migration. Pins survive a kit growing into an adult.
Contaminated beavers are not affected: the game's separate "contaminated" migration row still works as normal.

## Requirements

* Harmony mod (`Harmony` id).

## Building

```
dotnet build -c Release
```
Set `-p:TimberbornPath=...` if the game isn't in `C:\games\Timberborn.v1.1\Timberborn`. The finished mod folder is in `dist\PopulationControl`.
