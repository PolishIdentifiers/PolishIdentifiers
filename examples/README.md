# Examples

This folder contains one console project intended as the fast-start example for the package.

The project is organized by identifier and integration subject:

1. `Nip/` contains the NIP examples
2. `Pesel/` contains the PESEL examples
3. `Regon/` contains the REGON examples
4. `JsonConverter/` contains the `System.Text.Json` example

`Program.Main()` still calls three top-level methods in order: `TryParse()`, `Parse()`, and `Validate()`.
Each of those methods delegates to the identifier-specific files so one run shows the main public entry points while keeping the code grouped by folder.
