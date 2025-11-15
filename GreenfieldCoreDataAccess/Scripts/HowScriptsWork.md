# How scripts work

## What is this folder
This folder houses scripts used to manage the GreenfieldCore database (tables, procedures, data migrations). Scripts are discovered and applied automatically at application startup.

- Startup calls PerformDatabaseMigrations(), which runs the ScriptManager.
- ScriptManager discovers all .sql files under ScriptsRoot (recursive), orders them deterministically, and applies only those that should run.

Configure ScriptsRoot in configuration (relative to solution root), for example:
- ScriptsRoot: GreenfieldCoreDataAccess/Scripts

## Filename format and versioning
Scripts are versioned per "AppliesTo" group, inferred from the filename.

General patterns:
- Tables (non-sproc):
  - init_<TableName>_v<major>.<minor>.sql
  - <any-non-init-token>_<TableName>_v<major>.<minor>.sql
- Stored procedures (sproc):
  - init_usp_<ProcName>_v<major>.<minor>.sql
  - <any-non-init-token>_usp_<ProcName>_v<major>.<minor>.sql

Parsing rules:
- IsInit = true if the first token is init; otherwise false.
- IsSproc = true if the second token is usp.
- AppliesTo:
  - Non-sproc: AppliesTo = <TableName> (the second token)
  - Sproc: AppliesTo = usp_<ProcName> (the second token "usp" + third token as name)
- Version is the last token in the form v<major>.<minor> (e.g., v1.0, v2.3).

Examples:
- init_ScriptHistory_v1.0.sql → IsInit=true, IsSproc=false, AppliesTo=ScriptHistory, Version=1.0
- init_usp_SelectClientRoles_v1.0.sql → IsInit=true, IsSproc=true, AppliesTo=usp_SelectClientRoles, Version=1.0
- add_column_Clients_v1.1.sql → IsInit=false, IsSproc=false, AppliesTo=Clients, Version=1.1
- alter_usp_SelectClientRoles_v1.2.sql → IsInit=false, IsSproc=true, AppliesTo=usp_SelectClientRoles, Version=1.2

## When does a script run?
The decision is made by usp_ShouldScriptBeApplied using ScriptHistory:

- If IsInit = 1:
  - Runs only if no scripts exist in ScriptHistory for this AppliesTo.
- If IsInit = 0:
  - If no row exists for this AppliesTo, it does NOT run (you must provide an init first).
  - If there are rows for this AppliesTo, it runs only if:
    - p_Major > max(Major) for this AppliesTo, OR
    - p_Major == max(Major) AND p_Minor > max(Minor) for that major.

Implications:
- Versions must strictly increase per AppliesTo; duplicates are skipped.
- You can "jump" versions (e.g., 1.0 → 1.3) as long as the new version is greater than the recorded max.

Note on new major init (e.g., v2.0):
- __If you ship v2.0 as an init script for an existing AppliesTo, it will only run on fresh databases (no ScriptHistory rows for that AppliesTo). Upgraded databases will skip it because history exists. Plan your last 1.x scripts so upgraded databases end up equivalent to 2.0.__

## Dependencies (DependsOn)
You should declare dependencies so a script’s prerequisites are processed first.

Add a header line to your .sql file:
-- DependsOn: <AppliesTo1>, <AppliesTo2>, ...

Notes:
- The values must match other scripts’ AppliesTo values:
  - Tables: the table name (e.g., ScriptHistory, ClientRoles).
  - Sprocs: "usp_<ProcName>" (e.g., usp_SelectClientRoles).
- "ScriptHistory" should always be a dependency since it must exist.
- When a dependency is declared, ScriptManager will process the entire dependency group (all its ordered scripts) before the current script.

Example (from procedure scripts):
-- DependsOn: ScriptHistory, ClientRoles

## Execution order
Within a run, scripts are ordered deterministically:
1) Files related to ScriptHistory first.
2) Then by AppliesTo (alphabetically).
3) Non-sproc before sproc within a group.
4) Init before non-init within a group.
5) Then by Major, then Minor ascending.

Dependencies can pull forward prerequisite groups as needed.

## Creating a new script (first version)
1) Choose AppliesTo:
   - Table: use the table name (e.g., Clients).
   - Sproc: use usp_<ProcName> (e.g., usp_SelectClientRoles).
2) Create an init file with version v1.0 (or your chosen starting version):
   - Table: init_<TableName>_v1.0.sql
   - Sproc: init_usp_<ProcName>_v1.0.sql
3) If your script needs others first, add a DependsOn header:
   -- DependsOn: ScriptHistory, <OtherAppliesTo>
4) Implement the DDL/DML.
   - For sprocs, "create procedure if not exists ..." is fine for init.
5) Commit the script under the proper folder, e.g.:
   - Scripts/Tables/<TableName>/...
   - Scripts/Procedures/<Area>/...

## Updating to a new version
**Never edit previously applied scripts. Add a new file with a higher version.**

1) Pick the next version:
   - Minor bump for additive or backward-compatible changes (1.0 → 1.1).
   - Major bump for breaking changes (1.x → 2.0).
2) Name the file:
   - Table: <reason>_<TableName>_v<major>.<minor>.sql
   - Sproc: <reason>_usp_<ProcName>_v<major>.<minor>.sql
   - The <reason> token is any non-init label like add_column, alter, drop, data_fix, etc.
3) Add DependsOn if needed so prerequisites run first.
4) Implement changes:
   - Tables: ALTER TABLE, data migrations, etc.
   - Sprocs (MySQL/MariaDB): to change body, drop and recreate or use CREATE OR REPLACE if supported by your engine. Example pattern:
     ```mariadb
     drop procedure if exists usp_MyProc;
     delimiter $$
     create procedure usp_MyProc(...)
     begin
       -- new body
     end$$
     delimiter ;```
5) Save the script; it will run automatically on next app start if its version is greater than what’s recorded.

## Major version rollover pattern:
- Goal: new installations start at 2.0 with a clean init; existing installations upgrade through the last 1.x scripts to the same final schema.
- Steps:
  1) Ensure the last 1.x script(s) transform the schema/data to exactly match the desired 2.0 state.
     - Typical sequence: add new structures, backfill, dual-write if needed, switch reads, remove old artifacts, finalize constraints/indexes.
  2) Add an init script for 2.0 (IsInit = true), e.g.:
     - init_<TableName>_v2.0.sql or init_usp_<ProcName>_v2.0.sql
     - This script defines the 2.0 final state for fresh installations.
  3) Do not expect the 2.0 init to run on upgraded databases:
     - usp_ShouldScriptBeApplied skips init when ScriptHistory already contains any rows for that AppliesTo.
- Why this works:
  - Fresh install: only 2.0 init (and higher) runs → starts clean at 2.x.
  - Upgrade: 2.0 init is skipped but the last 1.x scripts already made the schema equal to 2.0.
- If you must "force" a 2.0 init on existing DBs (generally not recommended): you’d have to remove ScriptHistory rows for that AppliesTo or change AppliesTo (e.g., Clients2), which splits history and complicates maintenance.

## Types of updates (guidance)
- Init:
  - First script for an AppliesTo. Must be unique; only runs when none exist for that AppliesTo.
- Minor (backward-compatible):
  - Add columns with defaults/nullability compatible with existing rows.
  - Add non-breaking indexes, constraints, or sproc body changes that don’t break callers.
  - Data backfills that don’t change semantics for existing code.
- Major (breaking):
  - Drop/rename columns, change data types incompatibly, rename tables.
  - Change sproc signatures or behavior in a way that breaks callers.
  - Plan multistep migrations: often a minor to prepare, a major for the actual break, and a minor to clean up.
- Data migrations:
  - Use a dedicated AppliesTo if it’s cross-cutting (e.g., DataMigration_Billing) with its own init and DependsOn for the affected tables/procs.
  - Otherwise, attach to a concrete table AppliesTo and use DependsOn to ensure order.

## Examples

##### Initial ScriptHistory (must exist first):
- Scripts/Tables/ScriptHistory/init_ScriptHistory_v1.0.sql
  - Creates ScriptHistory table and:
    - usp_RecordScriptExecution
    - usp_ShouldScriptBeApplied

##### New table:
- init_Clients_v1.0.sql
- add_index_Clients_v1.1.sql

##### New sproc:
- init_usp_SelectClientRoles_v1.0.sql
  -- DependsOn: ScriptHistory, ClientRoles
  create procedure if not exists usp_SelectClientRoles(...) ...

##### Update sproc body (minor):
- alter_usp_SelectClientRoles_v1.1.sql
  -- DependsOn: ScriptHistory, ClientRoles
  drop procedure if exists usp_SelectClientRoles;
  create procedure usp_SelectClientRoles(...) ...

##### Breaking change to Clients (major):
- migrate_phase1_Clients_v1.9.sql  -- add new column, backfill
- breaking_change_Clients_v2.0.sql -- switch consumers, drop old column(s)

##### Major rollout with new init at 2.0 (Clients):
- 1.9 — migrate_phase1_Clients_v1.9.sql
  - Add new columns/structures, backfill data.
- 1.10 — migrate_phase2_Clients_v1.10.sql
  - Switch reads/writes to new structures, keep old in place temporarily.
- 1.11 — finalize_Clients_v1.11.sql
  - Drop deprecated columns, finalize constraints and indexes.
  - Resulting schema equals the desired 2.0 state.
- 2.0 — init_Clients_v2.0.sql (IsInit = true)
  - Creates the final 2.0 schema for fresh installs only.
  - Upgraded databases skip this (history exists) but are already equivalent due to v1.11.

## Troubleshooting
- Non-init script not running: ensure an init exists and has been recorded for the AppliesTo.
- Script keeps re-running: check filename version; it must be strictly greater than the recorded version.
- Dependency not respected: verify DependsOn values match the target AppliesTo exactly (tables use their name; sprocs use usp_<ProcName>).
- Scripts not found: verify ScriptsRoot is correctly configured and paths are correct.
