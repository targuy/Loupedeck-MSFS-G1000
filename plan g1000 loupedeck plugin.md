# PLAN — G1000 Loupedeck Plugin v2.0

> Document de référence pour Claude Code  
> Statut : Prêt pour implémentation  
> Mai 2026

-----

## 1. Vision du projet

Plugin C# natif pour Loupedeck Live et CT — **un seul binaire, deux comportements** — qui fournit :

1. **Un Dynamic Folder G1000** — simulation complète du Garmin G1000 NXi sur MSFS 2024 avec softkeys contextuels dynamiques, affichage temps réel des états, et gestion bidirectionnelle des événements sim.
1. **Une bibliothèque d’actions MSFS** — remplacement du plugin MSFS générique officiel Loupedeck (cassé sur les versions récentes), exposant les contrôles standard comme des actions mappables librement par l’utilisateur.

-----

## 2. Contraintes techniques non négociables

|Contrainte             |Détail                                                                            |
|-----------------------|----------------------------------------------------------------------------------|
|SDK                    |Logi Actions SDK 4 (C# .NET 8) — obligatoire pour Dynamic Folders et BitmapBuilder|
|Devices cibles         |Loupedeck Live **et** Loupedeck CT — **un seul plugin**                           |
|Compatibilité logiciel |Dernière version logiciel Loupedeck (6.x)                                         |
|Sim                    |Microsoft Flight Simulator 2024                                                   |
|Avionics cibles        |Garmin G1000 NXi (WorkingTitle, inclus MSFS 2024)                                 |
|Communication sim      |WASimCommander — toutes les données (L:Vars, H:Events, K:Events, A:Vars via RPN)  |
|Lifecycle sim          |SimConnect — uniquement événements système (AircraftLoaded, SimStart, Pause)      |
|Dépendances utilisateur|WASimCommander WASM dans dossier Community MSFS (gratuit, open source)            |
|Dépendances payantes   |**Aucune**                                                                        |
|Langage                |C# — Node.js écarté (Dynamic Folders absent SDK Node.js beta)                     |

-----

## 3. Architecture générale

```
┌──────────────────────────────────────────────────────────────┐
│                   Plugin C# (Logi Actions SDK4)              │
│                                                              │
│  ┌─────────────────────┐   ┌──────────────────────────────┐ │
│  │  G1000 Dynamic      │   │  MSFS Action Library         │ │
│  │  Folder (4 pages)   │   │  (PluginDynamicCommands)     │ │
│  └──────────┬──────────┘   └──────────────────────────────┘ │
│             │                                                │
│  ┌──────────▼──────────────────────────────────────────────┐ │
│  │              G1000StateManager                          │ │
│  │  Miroir état sim · publie ChangeEvents → redraws       │ │
│  └──────────┬──────────────────────────────────────────────┘ │
│             │                                                │
│  ┌──────────▼──────────────────────────────────────────────┐ │
│  │  DeviceAdapter (interface commune)                      │ │
│  │   LiveAdapter          CTAdapter                        │ │
│  │   B5-B8 navigation     B5-B8 + roue + wheel screen     │ │
│  └──────────┬──────────────────────────────────────────────┘ │
│             │                                                │
│  ┌──────────▼──────────────────────────────────────────────┐ │
│  │              SimLayer                                   │ │
│  │   WASimCommander (données)   SimConnect (lifecycle)     │ │
│  └─────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────┘
                          │ USB
           ┌──────────────┴──────────────┐
           │                             │
    Loupedeck Live              Loupedeck CT
    (détection auto)            (détection auto)
           │                             │
           └──────────────┬──────────────┘
                          │
               ┌──────────▼──────────┐
               │     MSFS 2024       │
               │  G1000 NXi WT v2    │
               │  WASimCmd WASM      │
               └─────────────────────┘
```

-----

## 4. Structure du projet C#

```
G1000LoupedeckPlugin/
│
├── G1000LoupedeckPlugin.csproj
├── Plugin.cs                          ← Point d'entrée, détection device, enregistrement
│
├── Adapters/
│   ├── IDeviceAdapter.cs              ← Interface commune Live/CT
│   ├── LiveAdapter.cs                 ← Navigation B5-B8 uniquement
│   └── CTAdapter.cs                   ← Navigation B5-B8 + roue droite/gauche + wheel screen
│
├── Sim/
│   ├── SimLayer.cs                    ← Façade unique vers le sim
│   ├── WaSimClient.cs                 ← Wrapper WASimCommander
│   ├── SimConnectClient.cs            ← Lifecycle events uniquement
│   └── SimVariables.cs                ← Constantes RPN / L:Vars / H:Events
│
├── State/
│   ├── G1000StateManager.cs           ← Miroir état G1000, abonnements WASimCmd
│   ├── G1000State.cs                  ← Modèle de données
│   └── StateChangeEvent.cs            ← Événement sur changement de valeur
│
├── G1000/
│   ├── G1000Folder.cs                 ← Dynamic Folder principal
│   ├── Pages/
│   │   ├── G1000Page.cs               ← Classe de base : Id, Name, Color, ColorDim
│   │   ├── PfdSoftkeysPage.cs         ← Page PFD — bleu
│   │   ├── MfdSoftkeysPage.cs         ← Page MFD — vert
│   │   ├── AutopilotPage.cs           ← Page AP — amber
│   │   └── ComNavPage.cs              ← Page COM/NAV — cyan
│   ├── Controls/
│   │   ├── SoftkeyButton.cs           ← Bouton LCD softkey label dynamique
│   │   ├── DualRingEncoder.cs         ← Encodeur inner/outer toggle sur clic
│   │   └── StateButton.cs             ← Bouton ON/OFF avec couleur LED
│   └── Rendering/
│       ├── G1000Renderer.cs           ← BitmapBuilder helpers
│       └── G1000Theme.cs              ← Couleurs pages, fonts, états
│
├── MsfsLibrary/
│   ├── MsfsActionLoader.cs            ← Chargement et enregistrement depuis JSON
│   └── actions.json                   ← Définitions extensibles par la communauté
│
├── Video/
│   └── FrameStreamer.cs               ← Render-loop bitmap → LCD / wheel screen
│
└── Resources/
    ├── icons/
    └── fonts/
```

-----

## 5. Système de couleurs des pages

Chaque page a une couleur identitaire utilisée à trois endroits simultanément :

1. **Fond des boutons LCD** — teinté avec la couleur de la page
1. **LED du bouton physique de navigation** — même couleur, pleine intensité si page active, 30% si inactive
1. **Wheel screen CT** — fond de couleur plein + nom de la page centré

```csharp
// G1000Theme.cs
public static class G1000Theme {

    public static readonly PageTheme PFD = new(
        Name:     "PFD",
        Color:    new BitmapColor(0, 85, 255),    // Bleu #0055FF
        ColorDim: new BitmapColor(0, 25, 76),     // Bleu 30%
        ButtonId: PhysicalButton.B5
    );

    public static readonly PageTheme MFD = new(
        Name:     "MFD",
        Color:    new BitmapColor(0, 204, 68),    // Vert #00CC44
        ColorDim: new BitmapColor(0, 61, 20),     // Vert 30%
        ButtonId: PhysicalButton.B6
    );

    public static readonly PageTheme Autopilot = new(
        Name:     "AUTOPILOT",
        Color:    new BitmapColor(255, 179, 0),   // Amber #FFB300
        ColorDim: new BitmapColor(76, 54, 0),     // Amber 30%
        ButtonId: PhysicalButton.B7
    );

    public static readonly PageTheme ComNav = new(
        Name:     "COM / NAV",
        Color:    new BitmapColor(0, 204, 255),   // Cyan #00CCFF
        ColorDim: new BitmapColor(0, 61, 76),     // Cyan 30%
        ButtonId: PhysicalButton.B8
    );

    // Softkeys G1000
    public static readonly BitmapColor SoftkeyActive   = new(0, 255, 101);  // Vert G1000 #00FF65
    public static readonly BitmapColor SoftkeyInactive = new(26, 26, 26);   // Gris foncé
    public static readonly BitmapColor SoftkeyText     = BitmapColor.White;

    // États AP
    public static readonly BitmapColor ApOn   = new(0, 255, 101);   // Vert
    public static readonly BitmapColor ApArm  = new(255, 179, 0);   // Amber
    public static readonly BitmapColor ApOff  = new(26, 26, 26);    // Gris

    // Power OFF
    public static readonly BitmapColor PowerOff = BitmapColor.Black;
}

public record PageTheme(string Name, BitmapColor Color, BitmapColor ColorDim, PhysicalButton ButtonId);
```

### Mise à jour LED navigation à chaque changement de page

```csharp
void OnPageChanged(G1000Page newPage) {
    foreach (var theme in G1000Theme.AllPages) {
        var isActive = theme == newPage.Theme;
        _adapter.SetButtonLedColor(
            theme.ButtonId,
            isActive ? theme.Color : theme.ColorDim
        );
    }
}
```

-----

## 6. DeviceAdapter — Spécification

### Interface commune

```csharp
public interface IDeviceAdapter {
    void SetButtonLedColor(PhysicalButton button, BitmapColor color);
    void SetAllButtonsBrightness(int percent);        // 0 = power off
    void OnPageButtonPressed(Action<G1000Page> handler);
    void Initialize(G1000Folder folder);
}
```

### LiveAdapter

```csharp
public class LiveAdapter : IDeviceAdapter {
    // B5 → PFD, B6 → MFD, B7 → AP, B8 → COM/NAV
    // Pas de wheel screen
    // Pas de roue
}
```

### CTAdapter

```csharp
public class CTAdapter : IDeviceAdapter {

    private int _currentPageIndex = 0;
    private int _currentRepIndex  = 0;
    private readonly G1000Page[] _pages = { PFD, MFD, Autopilot, ComNav };

    // Roue DROITE → page suivante (cycle 0→1→2→3→0)
    void OnWheelRight(int delta) {
        _currentPageIndex = (_currentPageIndex + 1) % 4;
        _currentRepIndex  = 0;  // reset représentation au changement de page
        NavigateTo(_pages[_currentPageIndex]);
        RefreshWheelScreen();
    }

    // Roue GAUCHE → représentation précédente (Phase 4 — stub pour l'instant)
    void OnWheelLeft(int delta) {
        // Phase 4 : cycle entre représentations de la page courante
        // Pour l'instant : no-op
    }

    void RefreshWheelScreen() {
        var page = _pages[_currentPageIndex];
        using var bmp = new BitmapBuilder(WheelScreenWidth, WheelScreenHeight);

        // Fond couleur plein
        bmp.FillRectangle(0, 0, bmp.Width, bmp.Height, page.Theme.Color);

        // Nom de page centré, police aviation
        bmp.DrawText(
            page.Theme.Name,
            color: BitmapColor.White,
            fontSize: 20,
            fontStyle: FontStyle.Bold,
            horizontalAlignment: HorizontalAlignment.Center,
            verticalAlignment: VerticalAlignment.Center
        );

        // Indicateur de représentation (Phase 4) — dots en bas
        // DrawRepresentationDots(bmp, _currentRepIndex, page.RepresentationCount);

        _device.SetWheelScreen(bmp.ToImage());
    }
}
```

-----

## 7. Spikes de faisabilité — À implémenter en premier

Créer dans `G1000LoupedeckPlugin.Spikes/` — programmes de test isolés.

### Spike S1+S3 — Bidirectionnel WASimCommander (PRIORITÉ 1)

**Objectif :** Valider le pattern complet : envoyer un H:Event depuis .NET ET recevoir un callback sur changement d’état MSFS.

**Cas de test :** Autopilote AP_MASTER.

```csharp
var client = new WASimCommander.Client.WASimClient(0x4C4F4749);
await client.connectSimulator();

// Subscribe — callback sur changement uniquement
client.registerDataRequest(new DataRequest {
    requestId  = 1,
    nameOrCode = "(A:AUTOPILOT MASTER, bool)",
    period     = UpdatePeriod.Changed
});
client.OnDataReceived += (req, val) =>
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] AP Master: {val.doubleVal}");

// Envoyer H:Event
Console.WriteLine("Sending AP_MASTER toggle...");
client.executeCalculatorCode("(>K:AP_MASTER)");

// Attendre callback → mesurer latence
Console.ReadLine();
```

**Critère de succès :** Callback reçu < 100ms dans les deux sens (Loupedeck→sim ET cockpit souris→Loupedeck).

**Questions à documenter :**

- Latence réelle mesurée
- Comportement si WASM absent (message d’erreur ?)
- Délai connexion initiale WASimCommander

-----

### Spike S2 — Dynamic Folder + BitmapBuilder + LED couleur (PRIORITÉ 1)

**Objectif :** Valider affichage texte dynamique sur LCD + contrôle couleur LED bouton physique.

**Cas de test :**

1. Dynamic Folder avec 1 bouton LCD qui affiche texte coloré mis à jour chaque seconde
1. Appui bouton B5 → LED passe au bleu, B6 → LED passe au vert (test SetButtonLedColor)

```csharp
// Test LED couleur par bouton
_device.SetButtonLed(PhysicalButton.B5, new BitmapColor(0, 85, 255));   // Bleu
_device.SetButtonLed(PhysicalButton.B6, new BitmapColor(0, 204, 68));   // Vert
_device.SetButtonLed(PhysicalButton.B7, new BitmapColor(255, 179, 0));  // Amber
_device.SetButtonLed(PhysicalButton.B8, new BitmapColor(0, 204, 255)); // Cyan
```

**Si SetButtonLedColor non disponible (fallback) :** LED uniquement ON/OFF blanc — les fonds de pages LCD restent colorés, seuls les boutons physiques ne peuvent pas être colorés individuellement.

-----

### Spike S2b — CT Wheel Screen (PRIORITÉ 2, CT requis)

**Objectif :** Valider que `SetWheelScreen()` accepte un bitmap BitmapBuilder et mesurer la latence de mise à jour.

**Cas de test :** Afficher fond bleu + texte “PFD” sur le wheel screen, puis changer vers fond vert + “MFD” après 2 secondes.

**Questions à documenter :**

- Dimensions exactes du wheel screen en pixels
- API exacte : `SetWheelScreen()` / `SetWheelImage()` / autre
- Latence de mise à jour perçue
- La roue gauche et droite sont-elles deux events distincts ?

-----

### Spike S5 — Render-loop 20fps (PRIORITÉ 2)

**Objectif :** Mesurer le débit réel de push de bitmaps sur bouton LCD et wheel screen CT.

**Cas de test :** Jauge circulaire SkiaSharp animée (aiguille qui tourne) en boucle à 20fps.

**Métriques à collecter :** frame time moyen/max, drops, CPU%, memory sur 60s.

**Critère de succès :** 20fps stable, CPU < 10%, pas de memory leak.

-----

## 8. Dynamic Folder G1000 — Layout complet

### 8.1 Loupedeck Live — Boutons physiques (fixes toutes pages)

```
B1 = Home (réservé Loupedeck — ne pas toucher)
B2 = DIRECT-TO
B3 = ENT
B4 = CLR
B5 = → Page PFD       (LED Bleu)
B6 = → Page MFD       (LED Vert)
B7 = → Page AP        (LED Amber)
B8 = → Page COM/NAV   (LED Cyan)
```

### 8.2 Page 1 — PFD Softkeys (fond bleu #0055FF teinté)

```
LCD 0..11 = Softkeys PFD 1..12 (labels dynamiques L:Vars NXi)

Encodeurs :
  E1 = BARO       outer: ±1 hPa    inner: toggle STD
  E2 = HDG Bug    outer: ±1°       inner: ±10° ou SYNC (clic)
  E3 = CRS        outer: ±1°       inner: ±10°
  E4 = NAV1       outer: MHz       inner: kHz   (clic toggle, label LCD adjacent)
  E5 = COM1       outer: MHz       inner: kHz   (clic toggle, label LCD adjacent)
  E6 = RANGE MFD  outer: zoom +    inner: zoom -
```

### 8.3 Page 2 — MFD Softkeys (fond vert #00CC44 teinté)

```
LCD 0..11 = Softkeys MFD 1..12 (labels dynamiques)

Encodeurs :
  E1 = FMS Outer  navigation menus MFD
  E2 = FMS Inner  navigation listes MFD
  E3 = NAV2       outer: MHz       inner: kHz
  E4 = COM2       outer: MHz       inner: kHz
  E5 = ALT        outer: ±100ft    inner: ±1000ft
  E6 = VS         ±100fpm
```

### 8.4 Page 3 — Autopilote (fond amber #FFB300 teinté)

```
LCD layout — états ON/OFF/ARM avec couleur :
  [AP  ][FD  ][HDG ][NAV ][ALT ][VS  ]
  [FLC ][VNV ][APR ][BC  ][    ][    ]

Couleurs état :
  ON  → fond vert  #00FF65, texte blanc
  ARM → fond amber #FFB300, texte blanc
  OFF → fond gris  #1A1A1A, texte #666666

Encodeurs :
  E1 = HDG Bug    ±1°
  E2 = Altitude   outer: ±100ft   inner: ±1000ft
  E3 = VS         ±100fpm
  E4 = FLC Speed  ±1kt
  E5 = Course
  E6 = Barometer  ±1 hPa
```

### 8.5 Page 4 — COM / NAV (fond cyan #00CCFF teinté)

```
LCD layout :
  [COM1 Act MHz][COM1 Act kHz][⇄][COM2 Act MHz][COM2 Act kHz][⇄]
  [COM1 Sby MHz][COM1 Sby kHz][  ][COM2 Sby MHz][COM2 Sby kHz][  ]
  [NAV1 Active ][NAV1 Stby   ][⇄][NAV2 Active  ][NAV2 Stby   ][⇄]

Encodeurs :
  E1 = COM1 outer (MHz)
  E2 = COM1 inner (kHz)
  E3 = NAV1 outer (MHz)
  E4 = COM2 outer (MHz)
  E5 = COM2 inner (kHz)
  E6 = NAV2 outer (MHz)
```

-----

## 9. DualRingEncoder — Spécification

```csharp
public class DualRingEncoder {
    private RingMode _mode = RingMode.Outer;

    public enum RingMode { Outer, Inner }

    // Clic = toggle mode + redessine label LCD adjacent
    void OnClick() {
        _mode = _mode == RingMode.Outer ? RingMode.Inner : RingMode.Outer;
        RefreshLabel();
    }

    void RefreshLabel() {
        using var bmp = new BitmapBuilder(imageSize);
        var isInner = _mode == RingMode.Inner;
        bmp.FillRectangle(0, 0, bmp.Width, bmp.Height,
            isInner ? G1000Theme.AccentCyan : G1000Theme.PFD.Color);
        bmp.DrawText(
            isInner ? $"{_name}\nkHz" : $"{_name}\nMHz",
            color: BitmapColor.White,
            fontSize: 12
        );
        _plugin.SetButtonImage(_labelButtonId, bmp.ToImage());
    }

    void OnRotate(int delta) {
        var hEvent = (_mode, delta > 0) switch {
            (RingMode.Outer, true)  => _outerIncEvent,
            (RingMode.Outer, false) => _outerDecEvent,
            (RingMode.Inner, true)  => _innerIncEvent,
            (RingMode.Inner, false) => _innerDecEvent,
        };
        _simLayer.ExecuteCalculatorCode($"(>H:{hEvent})");
    }
}
```

-----

## 10. G1000StateManager — Abonnements WASimCommander

```csharp
// Lifecycle (SimConnect)
OnAircraftLoaded   → charger profil avion, réinitialiser state
OnSimStarted       → connecter WASimCommander
OnSimStopped       → déconnecter, afficher écran attente
OnSimPaused        → optionnel : indicateur visuel pause

// État avionics (WASimCommander)
"(A:ELECTRICAL AVIONICS BUS VOLTAGE, Volts)" → AvionicsMasterOn (> 0V)
// → si false : SetAllButtonsBrightness(0) + tous LCD noirs

// Autopilote
"(A:AUTOPILOT MASTER, bool)"                 → AutopilotMaster
"(A:AUTOPILOT HEADING LOCK, bool)"           → ApHdgMode
"(A:AUTOPILOT ALTITUDE LOCK, bool)"          → ApAltMode
"(A:AUTOPILOT VERTICAL HOLD, bool)"          → ApVsMode
"(A:AUTOPILOT FLIGHT LEVEL CHANGE, bool)"    → ApFlcMode
"(A:AUTOPILOT APPROACH HOLD, bool)"          → ApApproachMode
"(A:AUTOPILOT NAV1 LOCK, bool)"              → ApNavMode

// Radios
"(A:COM ACTIVE FREQUENCY:1, MHz)"            → Com1ActiveMhz
"(A:COM STANDBY FREQUENCY:1, MHz)"           → Com1StandbyMhz
"(A:COM ACTIVE FREQUENCY:2, MHz)"            → Com2ActiveMhz
"(A:COM STANDBY FREQUENCY:2, MHz)"           → Com2StandbyMhz
"(A:NAV ACTIVE FREQUENCY:1, MHz)"            → Nav1ActiveMhz
"(A:NAV STANDBY FREQUENCY:1, MHz)"           → Nav1StandbyMhz
"(A:NAV ACTIVE FREQUENCY:2, MHz)"            → Nav2ActiveMhz
"(A:NAV STANDBY FREQUENCY:2, MHz)"           → Nav2StandbyMhz

// Softkey labels — à confirmer par reverse-engineering (Spike S1+S3)
"(L:AS1000_PFD_SOFTKEY1_LABEL, string)"      → PfdSoftkeyLabels[0]
// ... x12 PFD, x12 MFD
```

-----

## 11. Power ON / OFF simulation

```csharp
// Dans G1000StateManager — on AvionicsMasterOn change
void OnAvionicsMasterChanged(bool isOn) {
    if (isOn) {
        _adapter.SetAllButtonsBrightness(100);
        _folder.RefreshCurrentPage();           // redessiner tous les LCD
        OnPageChanged(_currentPage);            // restaurer les LED couleur
    } else {
        _adapter.SetAllButtonsBrightness(0);    // boutons physiques éteints
        _folder.SetAllButtonsBlack();           // LCD noirs
    }
}
```

-----

## 12. Bibliothèque MSFS — actions.json

```json
{
  "version": "1.0",
  "groups": [
    {
      "id": "flight_controls",
      "label": "MSFS — Flight Controls",
      "actions": [
        {
          "id": "gear_toggle",
          "label": "Gear Toggle",
          "type": "command",
          "event": "(>K:GEAR_TOGGLE)",
          "stateVar": "(A:GEAR HANDLE POSITION, bool)",
          "icons": { "0": "gear_up", "1": "gear_down" }
        },
        {
          "id": "flaps_inc",
          "label": "Flaps +",
          "type": "command",
          "event": "(>K:FLAPS_INCR)"
        },
        {
          "id": "flaps_dec",
          "label": "Flaps −",
          "type": "command",
          "event": "(>K:FLAPS_DECR)"
        },
        {
          "id": "spoilers_toggle",
          "label": "Spoilers",
          "type": "command",
          "event": "(>K:SPOILERS_TOGGLE)",
          "stateVar": "(A:SPOILERS HANDLE POSITION, percent)",
          "icons": { "0": "spoilers_ret", "100": "spoilers_ext" }
        },
        {
          "id": "parking_brake",
          "label": "Parking Brake",
          "type": "command",
          "event": "(>K:PARKING_BRAKES)",
          "stateVar": "(A:BRAKE PARKING INDICATOR, bool)",
          "icons": { "0": "brake_off", "1": "brake_on" }
        }
      ]
    },
    {
      "id": "lights",
      "label": "MSFS — Lights",
      "actions": [
        { "id": "nav_lights",     "label": "Nav Lights",     "event": "(>K:NAV_LIGHTS_TOGGLE)",      "stateVar": "(A:LIGHT NAV, bool)",      "icons": { "0": "light_off", "1": "light_on" } },
        { "id": "beacon_lights",  "label": "Beacon",         "event": "(>K:TOGGLE_BEACON_LIGHTS)",   "stateVar": "(A:LIGHT BEACON, bool)",   "icons": { "0": "light_off", "1": "light_on" } },
        { "id": "strobe_lights",  "label": "Strobe",         "event": "(>K:STROBES_TOGGLE)",         "stateVar": "(A:LIGHT STROBE, bool)",   "icons": { "0": "light_off", "1": "light_on" } },
        { "id": "landing_lights", "label": "Landing Lights", "event": "(>K:LANDING_LIGHTS_TOGGLE)",  "stateVar": "(A:LIGHT LANDING, bool)",  "icons": { "0": "light_off", "1": "light_on" } },
        { "id": "taxi_lights",    "label": "Taxi Lights",    "event": "(>K:TOGGLE_TAXI_LIGHTS)",     "stateVar": "(A:LIGHT TAXI, bool)",     "icons": { "0": "light_off", "1": "light_on" } }
      ]
    },
    {
      "id": "engine",
      "label": "MSFS — Engine",
      "adjustments": [
        { "id": "throttle1", "label": "Throttle 1", "eventInc": "(>K:THROTTLE1_INCR_SMALL)", "eventDec": "(>K:THROTTLE1_DECR_SMALL)", "stateVar": "(A:GENERAL ENG THROTTLE LEVER POSITION:1, percent)" },
        { "id": "mixture1",  "label": "Mixture 1",  "eventInc": "(>K:MIXTURE1_RICH)",        "eventDec": "(>K:MIXTURE1_LEAN)",        "stateVar": "(A:RECIP ENG MIXTURE RATIO:1, percent)" }
      ]
    },
    {
      "id": "navigation",
      "label": "MSFS — Navigation",
      "adjustments": [
        { "id": "transponder", "label": "Transponder", "eventInc": "(>K:XPNDR_INC_TENS)", "eventDec": "(>K:XPNDR_DEC_TENS)" }
      ]
    }
  ]
}
```

-----

## 13. FrameStreamer — Render-loop données temps réel

```csharp
public class FrameStreamer {
    private const int TargetFps = 20;

    public async Task StartAsync(string targetId, Func<SKBitmap> renderFrame,
        CancellationToken ct) {
        var interval = TimeSpan.FromMilliseconds(1000.0 / TargetFps);
        while (!ct.IsCancellationRequested) {
            var sw = Stopwatch.StartNew();
            var bitmap = renderFrame();
            _device.SetImage(targetId, ConvertToLoupedeck(bitmap));
            var remaining = interval - sw.Elapsed;
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining, ct);
        }
    }
}
```

**Cas d’usage Phase 4 :**

|Représentation|Target         |Données                      |
|--------------|---------------|-----------------------------|
|Engine Monitor|CT Wheel Screen|RPM, fuel flow, oil temp, CHT|
|Fuel Quantity |CT Wheel Screen|Left/right tank, total       |
|VSI Graphique |Bouton LCD     |Vertical speed visuel        |
|Mini Attitude |Bouton LCD     |Pitch/bank simplifié         |

-----

## 14. Phases d’implémentation

### Phase 0 — Spikes (4 jours)

- [ ] **S1+S3** — WASimCommander bidi : AP toggle + callback — mesurer latence
- [ ] **S2** — Dynamic Folder + BitmapBuilder + LED couleur par bouton physique
- [ ] **S2b** — CT Wheel Screen : API exacte + dimensions + latence
- [ ] **S5** — Render-loop 20fps : mesure débit LCD + wheel screen
- [ ] Documenter toutes les valeurs limites mesurées avant de continuer

### Phase 1 — Core plugin (3 semaines)

- [ ] Structure projet C# complète (tous les fichiers stub)
- [ ] SimLayer : WASimCommander connect/disconnect + SimConnect lifecycle
- [ ] G1000StateManager abonnements principaux (AP, radios, avionics master)
- [ ] IDeviceAdapter + LiveAdapter + CTAdapter (wheel screen + roue)
- [ ] G1000Folder avec 4 pages et navigation
- [ ] Page 3 Autopilote complète (états booléens, couleurs ON/ARM/OFF)
- [ ] DualRingEncoder inner/outer
- [ ] Power ON/OFF (brightness + LCD noirs)
- [ ] Système couleurs pages : LED boutons + fond LCD + wheel screen CT

### Phase 2 — G1000 complet (4 semaines)

- [ ] Page 1 PFD Softkeys dynamiques (après validation L:Vars labels Spike S1+S3)
- [ ] Page 2 MFD Softkeys dynamiques
- [ ] Page 4 COM/NAV avec fréquences temps réel
- [ ] Tous les encodeurs mappés
- [ ] Boutons fixes : ENT, CLR, MENU, FPL, DIRECT-TO
- [ ] Profils multi-avions JSON (Cessna 172, DA40, Bonanza)
- [ ] Tests croisés Loupedeck Live ET CT

### Phase 3 — Bibliothèque MSFS (2 semaines)

- [ ] MsfsActionLoader : chargement actions.json → PluginDynamicCommands
- [ ] Tous les groupes : Flight Controls, Lights, Engine, Navigation
- [ ] Icônes état ON/OFF pour chaque action
- [ ] Documentation CONTRIBUTING.md pour ajout d’actions par la communauté

### Phase 4 — Représentations CT + FrameStreamer (2 semaines)

- [ ] FrameStreamer SkiaSharp opérationnel
- [ ] Engine Monitor sur CT Wheel Screen (roue gauche)
- [ ] Fuel Quantity view
- [ ] Configuration JSON : quelle représentation sur quel target

### Phase 5 — Distribution (1 semaine)

- [ ] Packaging .lplug4 avec installeur
- [ ] Guide installation WASM (1 dossier Community à copier)
- [ ] README GitHub + CONTRIBUTING.md
- [ ] Soumission Marketplace Loupedeck

-----

## 15. Dépendances NuGet

```xml
<PackageReference Include="Loupedeck.Plugin.SDK"          Version="4.*" />
<PackageReference Include="WASimCommander.Client"          Version="*"  />
<PackageReference Include="SkiaSharp"                      Version="2.*" />
<PackageReference Include="Microsoft.Extensions.Logging"   Version="8.*" />
<PackageReference Include="Newtonsoft.Json"                Version="13.*" />
<!-- SimConnect : DLL locale du MSFS SDK, pas NuGet -->
```

-----

## 16. H:Events G1000 NXi — Référence principale

```
# Softkeys
AS1000_PFD_SOFTKEYS_1 .. AS1000_PFD_SOFTKEYS_12
AS1000_MFD_SOFTKEYS_1 .. AS1000_MFD_SOFTKEYS_12

# FMS Knob
AS1000_PFD_FMS_Upper_INC / DEC / PUSH
AS1000_MFD_FMS_Upper_INC / DEC / PUSH
AS1000_PFD_FMS_Lower_INC / DEC
AS1000_MFD_FMS_Lower_INC / DEC

# COM Radio
AS1000_PFD_COM_Radio_1_Whole_INC / DEC   (outer MHz)
AS1000_PFD_COM_Radio_1_Fract_INC / DEC   (inner kHz)
AS1000_PFD_COM_Radio_1_PUSH              (swap)
AS1000_PFD_COM_Radio_2_Whole_INC / DEC
AS1000_PFD_COM_Radio_2_Fract_INC / DEC
AS1000_PFD_COM_Radio_2_PUSH

# NAV Radio
AS1000_PFD_NAV_Radio_1_Whole_INC / DEC
AS1000_PFD_NAV_Radio_1_Fract_INC / DEC
AS1000_PFD_NAV_Radio_1_PUSH

# Navigation fixes
AS1000_PFD_DIRECTTO
AS1000_PFD_MENU_Push
AS1000_PFD_FPL_Push
AS1000_PFD_PROC_Push
AS1000_PFD_CLR
AS1000_PFD_ENT_Push

# BARO / RANGE
AS1000_PFD_BARO_INC / DEC / PUSH
AS1000_MFD_RANGE_INC / DEC
```

-----

## 17. Questions ouvertes — Résoudre par Spike S1+S3

1. **Labels softkeys en L:Var ?** Existe-t-il `AS1000_PFD_SOFTKEY1_LABEL` ou équivalent dans le NXi WorkingTitle ? Si non → reconstruire par logique de menu.
1. **Latence WASimCommander mesurée** → définir la fréquence de polling adaptative si > 100ms.
1. **API SetButtonLedColor** → RGB individuel disponible sur Live et CT ou seulement ON/OFF global ?
1. **Dimensions exactes wheel screen CT** → nécessaire pour S2b.
1. **Format fréquences L:Var** → entier (118350) ou flottant (118.35) ?
1. **AircraftLoaded event** → quel SimConnect system event exactement pour détecter le changement d’avion ?

-----

## 18. Prompt de démarrage pour Claude Code

```
Lis intégralement PLAN_G1000_Loupedeck_Plugin.md.

Commence par :
1. Créer la structure de projet C# complète décrite en section 4
   avec tous les fichiers stub (classes vides avec leurs membres publics
   définis mais non implémentés, et leurs commentaires TODO).

2. Implémenter entièrement le Spike S1+S3 (section 7) dans
   G1000LoupedeckPlugin.Spikes/Spike_S1S3_WaSimBidi.cs —
   programme console autonome qui teste la communication
   bidirectionnelle WASimCommander avec mesure de latence.

3. Implémenter le Spike S2 (section 7) dans
   G1000LoupedeckPlugin.Spikes/Spike_S2_DynamicFolder.cs —
   plugin Loupedeck minimal avec un Dynamic Folder,
   un bouton LCD avec texte dynamique coloré, et test
   SetButtonLedColor sur B5/B6/B7/B8.

Ne pas implémenter le plugin final avant que les spikes
soient validés et leurs résultats documentés.
```

-----

*G1000-Loupedeck-Plugin — Plan v2.0 — Mai 2026*

-----

## 19. Addendum v3 — Organisation par univers

Statut : decision produit apres essais CT et analyse ergonomique.

Le plugin ne doit pas melanger les commandes Garmin, les commandes aviation generale et les commandes du simulateur dans les memes pages. L'utilisateur doit pouvoir choisir son usage :

1. **Univers Garmin G1000 NXi**  
   Pages et actions qui miment le bezel Garmin : softkeys PFD/MFD, FMS, `D->`, `FPL`, `MENU`, `PROC`, `CLR`, `ENT`, COM/NAV Garmin, autopilote G1000. Ces pages utilisent en priorite les events `H:AS1000_*` et, a terme, les libelles dynamiques issus du `SoftKeyMenuSystem`.

2. **Univers Aviation Generale**  
   Pages pour avions plus anciens ou sans Garmin. Ces pages utilisent les commandes et donnees generiques MSFS via WASim/SimConnect : NAV, COM, lights, flaps, gear, trim, power, magnetos, starter, fuel pump, parking brake, autopilote generique si present.

3. **Univers Simulateur**  
   Pages pour l'interaction avec le jeu : vues, pause, ATC, VFR map, options non avioniques, aides et outils MSFS. Ces actions ne doivent pas polluer les pages avioniques.

4. **Univers Jets / Liners**  
   Extension future. Ne creer des pages jets/liners que lorsque les commandes specifiques sont verifiees. Ne pas introduire de pages placeholder dans les profils utilisateurs si elles ne font rien.

Regle de conception : **une page = un univers ou un cas d'utilisation clair dans cet univers**.

Navigation CT recommandee :

- boutons physiques 1-4 : `PFD`, `MFD`, `AP`, `COM/NAV` ;
- bouton physique 5 : `G1000 FIXED` ;
- bouton physique 6 : entree vers pages `GA` ;
- bouton physique 7 : entree vers pages `SIM` ;
- bouton physique 8 : reserve page suivante/retour selon les contraintes du device.

La page par defaut d'un profil Garmin ne doit pas etre un menu de raccourcis. Elle doit etre une page de travail (`PFD`, `MFD` ou `AP`). Un menu peut exister pour la decouverte, mais ne doit pas devenir l'interface principale en vol.
