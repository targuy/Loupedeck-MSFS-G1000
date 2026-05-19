# Loupedeck-MSFS-G1000
Projet de plugin Loupedeck pour flight simulator pour simuler notamment un Garmin G1000.

## Structure C#

- `LoupedeckMSFSG1000.slnx` : solution principale
- `src/LoupedeckMSFSG1000` : bibliothèque C# du plugin
- `tests/LoupedeckMSFSG1000.Tests` : tests unitaires
- `spikes/LoupedeckMSFSG1000.Spikes.WaSimBidi` : spike console WASimCommander
- `docs/spikes.md` : critères et résultats attendus de Phase 0
- `docs/architecture.md` : architecture Phase 1 corrigée après validation WASim/Loupedeck

Le projet suit volontairement la Phase 0 du plan : valider les hypothèses de communication, de Dynamic Folder et de rendu avant de développer le plugin final.

## Commandes

- `dotnet build LoupedeckMSFSG1000.slnx`
- `dotnet test LoupedeckMSFSG1000.slnx`
- `dotnet run --project spikes/LoupedeckMSFSG1000.Spikes.WaSimBidi`
