# Loupedeck MSFS G1000 - Product Redesign Brief

Statut : cadrage de refonte visuelle, ergonomique et technique.

Ce document consolide les rapports des roles suivants :

- expert ergonomie aviation / Garmin G1000 NXi ;
- webdesigner et developpeur senior pour softkeys dynamiques ;
- ingenieur QA / resilience ;
- specialiste outils de profils Loupedeck ;
- specialiste open source, publication et communication.

## 1. Diagnostic

Le plugin est fonctionnel pour plusieurs commandes MSFS/G1000, mais l'experience actuelle ressemble trop a un deck de raccourcis. Elle est loin du modele mental Garmin G1000 NXi.

Problemes principaux :

- les pages sont trop nombreuses et mal hierarchisees ;
- les titres et labels sont souvent dupliques entre image et libelle Loupedeck ;
- le rendu est trop "macro button", pas assez "bezel avionique" ;
- les softkeys PFD/MFD sont statiques alors que le G1000 NXi change leurs libelles selon le menu ;
- l'etat de connexion MSFS/WASim n'est pas assez visible ;
- l'outil de generation de profils fonctionne mais doit devenir un vrai moteur declaratif et validable.

Direction retenue : traiter le Loupedeck CT comme une facade de commandes avionique compacte, pas comme une miniature des ecrans du simulateur. Les PFD/MFD sont supposes visibles au-dessus du Loupedeck.

Principe d'architecture produit ajoute : ne pas melanger les univers. Le plugin doit fournir plusieurs familles de pages et d'actions :

- univers `Garmin G1000 NXi` : evenements directs Garmin, logique de bezel, softkeys PFD/MFD, FMS, COM/NAV Garmin ;
- univers `Aviation generale` : commandes et donnees MSFS generiques via WASim/SimConnect, pour avions classiques ou sans Garmin ;
- univers `Jets / liners` : extension future, seulement avec pages dediees lorsque des commandes specifiques fiables existent ;
- univers `Simulateur` : vues, pause, ATC, VFR map, options et fonctions non avioniques.

Une page doit correspondre a un seul univers ou a un seul cas d'utilisation dans cet univers. Les profils exemples peuvent proposer plusieurs univers, mais la navigation doit rendre le changement de contexte explicite.

## 2. Look and Feel Cible

Le rendu doit suivre une logique "bezel Garmin sombre".

Palette :

- fond global : noir `#000000` ;
- bouton inactif : `#050505` a `#101010` ;
- bordure inerte : `#2A2A2A` ;
- texte principal : blanc `#FFFFFF` ;
- texte secondaire : gris `#A0A0A0` ;
- texte desactive : gris `#555555` ;
- actif/capture : vert G1000 `#00FF65` ;
- arme/attention : amber `#FFB300` ;
- focus/selection radio : cyan `#00CCFF` ;
- navigation GPS : magenta `#FF00FF` ;
- accent PFD : bleu `#0055FF` ;
- accent MFD : vert `#00CC44` ;
- accent AP : amber `#FFB300` ;
- accent COM/NAV : cyan `#00CCFF`.

Regles visuelles :

- pas de grands fonds de couleur par page ;
- fond noir par defaut, accents sobres ;
- l'etat avionique prime toujours sur la couleur de page ;
- majuscules courtes : `HDG`, `ALT`, `VS`, `BARO`, `FMS`, `COM1`, `NAV1`, `D->`, `FPL`, `PROC`, `CLR`, `ENT` ;
- deux lignes maximum sur les boutons LCD ;
- aucun texte descriptif long ;
- pas de carres decoratifs dans les titres ;
- si un libelle Loupedeck externe reste visible, l'image du bouton doit eviter la redondance.

Etats standards :

- `Unavailable` : gris sombre, pas d'action utile ;
- `Inactive` : fond noir, texte gris clair ;
- `Active` : accent vert ;
- `Armed` : accent amber ;
- `Selected/Editing` : accent cyan ;
- `PowerOff` : LCD noirs, aucune couleur decorative ;
- `Alert` : amber seulement si evenement critique confirme.

## 3. Organisation Produit

La structure cible de navigation doit etre courte, stable et organisee par univers.

### Univers Garmin G1000 NXi

But : mimer le bezel et les commandes du Garmin. Ces pages utilisent les events `H:AS1000_*`, les softkeys dynamiques et les donnees Garmin/avionique disponibles.

1. `PFD`
2. `MFD`
3. `AP`
4. `COM/NAV`
5. `G1000 FIXED`

`PFD` et `MFD` sont les pages naturelles en vol. `AP` et `COM/NAV` sont des pages de travail frequentes. `G1000 FIXED` regroupe les touches physiques Garmin qui ne doivent pas etre dispersees partout : `D->`, `FPL`, `MENU`, `PROC`, `CLR`, `ENT`, swaps radio, range si necessaire.

### Univers Aviation Generale

But : fournir des controles generiques utilisables avec des avions plus anciens ou sans Garmin, via les actions MSFS/WASim.

Pages cible :

1. `GA NAV`
2. `GA COM`
3. `GA CONTROLS`
4. `GA POWER`

`GA NAV` contient heading, course, baro, altitude selected, navigation generique. `GA COM` contient radios COM/NAV generiques. `GA CONTROLS` contient trim, flaps, gear, parking brake, lights. `GA POWER` contient batterie, avionics, magnetos, starter, fuel pump.

### Univers Jets / Liners

But : preparer une extension sans polluer les pages G1000 ni aviation generale.

Pages cible futures :

1. `JET AP`
2. `JET RADIOS`
3. `JET SYSTEMS`

Ces pages ne doivent etre creees dans un profil exemple que lorsque les actions correspondantes sont verifiees. En attendant, elles restent dans la roadmap, pas dans le profil actif.

### Univers Simulateur

But : interagir avec le jeu et ses outils sans melanger les commandes avioniques.

Pages cible :

1. `SIM`
2. `VIEWS`
3. `ATC/MAP`

La premiere version peut fusionner ces usages dans une seule page `SIM` si le nombre d'actions est faible.

Navigation CT recommandee :

- boutons physiques 1-4 : pages racines Garmin `PFD`, `MFD`, `AP`, `COM/NAV` ;
- bouton physique 5 : `G1000 FIXED` ;
- bouton physique 6 : entree vers univers `GA` ;
- bouton physique 7 : entree vers univers `SIM` ;
- bouton physique 8 : page suivante ou retour selon contrainte de device ;
- roue / page cycle : navigation secondaire seulement ;
- ne pas utiliser les boutons physiques pour actions critiques avion sans confirmation.

Le menu d'accueil peut exister pour l'edition ou la decouverte, mais il ne doit pas etre la page principale en vol. La page par defaut doit etre `PFD`, `MFD` ou `AP` selon preference utilisateur. Les profils exemples doivent rendre le passage Garmin -> GA -> Sim explicite, jamais implicite.

## 4. Pages Cibles

Les pages suivantes sont les pages cibles de l'univers Garmin. Les pages Aviation Generale et Simulateur sont separees pour eviter les melanges.

### PFD

Boutons LCD :

- softkeys PFD 1 a 12, dynamiques.

Encodeurs :

- E1 `BARO`
- E2 `HDG`
- E3 `CRS`
- E4 `NAV1`
- E5 `COM1`
- E6 `RANGE` ou `INSET RNG`

Fallback softkeys si les libelles dynamiques ne sont pas disponibles :

- `INSET`, `PFD`, `OBS`, `CDI`, `XPDR`, `IDENT`, `TMR/REF`, `NRST`, `ALERTS`, slots vides selon contexte.

### MFD

Boutons LCD :

- softkeys MFD 1 a 12, dynamiques.

Encodeurs :

- E1 `FMS OUTER`
- E2 `FMS INNER`
- E3 `RANGE`
- E4 `ALT` ou `BARO`
- E5 `COM2`
- E6 `NAV2`

Fallback softkeys :

- `ENGINE`, `MAP OPT`, `DCLTR`, `TRAFFIC`, `TERRAIN`, `WEATHER`, `FPL`, `PROC`, `NRST`, `AUX`, `CHKLIST`, `BACK`.

### AP

Boutons LCD :

- `AP`, `FD`, `HDG`, `NAV`, `APR`, `ALT`, `VS`, `FLC`, `VNV`, `BC`, `YD` si avion concerne.

Encodeurs :

- E1 `HDG`
- E2 `ALT`
- E3 `VS`
- E4 `FLC`
- E5 `CRS`
- E6 `BARO`

Les boutons AP doivent afficher l'etat reel venu du simulateur, pas un etat optimiste local.

### COM/NAV

Boutons LCD :

- `COM1 A`, `COM1 S`, `COM1 <->`
- `COM2 A`, `COM2 S`, `COM2 <->`
- `NAV1 A`, `NAV1 S`, `NAV1 <->`
- `NAV2 A`, `NAV2 S`, `NAV2 <->`

Encodeurs :

- E1 `COM1`, clic MHz/kHz ;
- E2 `COM2`, clic MHz/kHz ;
- E3 `NAV1`, clic MHz/kHz ;
- E4 `NAV2`, clic MHz/kHz ;
- E5 `XPDR` si disponible ;
- E6 libre ou volume/squelch si disponible.

## 5. Softkeys Dynamiques PFD/MFD

Les events `H:AS1000_*_SOFTKEYS_n` suffisent pour appuyer sur une softkey, mais pas pour connaitre son libelle courant.

Architecture cible :

- conserver les `H:` events pour les appuis ;
- ajouter une source d'etat pour les libelles ;
- utiliser en priorite des `L:` vars lues par WASimCommander si elles existent ;
- si elles n'existent pas, creer un petit bridge avionique MSFS qui lit le `SoftKeyMenuSystem` du G1000 NXi et publie les libelles en `L:` vars ;
- fallback temporaire : modele interne statique ou semi-statique.

Modele a creer :

- `G1000SoftkeyState`
  - `Display` : `PFD` ou `MFD`
  - `Index` : 1..12
  - `Label`
  - `Enabled`
  - `Indicator`
  - `IsKnown`
  - `LastUpdatedUtc`

Comportement :

- libelle vide : bouton noir complet, pas d'action ;
- bouton disabled : texte gris, pas d'action ;
- bouton actif/indicator : fine barre verte ou amber ;
- apres appui : envoyer le `H:` event, puis attendre la mise a jour WASim/bridge pour redessiner.

Changements techniques :

- ajouter le support string dans `WaSimReflectionClient` ;
- etendre `G1000State` avec les softkeys ;
- ajouter `ISoftkeyStateProvider` ;
- ajouter `G1000SoftkeyCommand` parametre `pfd.1..pfd.12` et `mfd.1..mfd.12` ;
- mettre a jour le generateur de profil pour utiliser ces actions parametrees.

## 6. Resilience Et QA

L'etat actuel est insuffisant pour un usage robuste. Il faut une vraie machine d'etat de connexion.

Etats requis :

- `Unavailable`
- `Disconnected`
- `Connecting`
- `Connected`
- `Degraded`
- `Reconnecting`
- `Faulted`

Exigences :

- afficher un bouton/status `G1000 STATUS` ;
- distinguer DLL WASim absente, MSFS non lance, serveur WASim non connecte, callbacks absents ;
- backoff exponentiel borne : 1s, 2s, 5s, 10s, 30s max ;
- resubscribe automatique apres reconnexion ;
- etat stale si plus aucun callback depuis un seuil defini ;
- aucune action ne doit crasher le plugin ;
- logs structures avec operation, tentative, duree, resultat, exception.

Tests manuels minimaux :

1. Loupedeck sans MSFS.
2. MSFS lance mais pas de vol charge.
3. Vol charge avec WASim OK.
4. Fermeture MSFS pendant que le plugin tourne.
5. Relance MSFS et verification resubscription.
6. DLL WASim absente.
7. WASM WASim absent ou non connecte.
8. Spam de commandes pendant deconnexion.
9. Restart Logi Plugin Service.
10. Test CT et Live separement.

## 7. Outil De Profils

Le dossier `Profile builer loupedeck` doit devenir un outil structure, pas seulement un script.

Nom propose : `lpdeck`.

Objectifs :

- roundtrip `.lp5` sans perte ;
- inspection ;
- diff ;
- validation stricte ;
- generation depuis YAML/JSON ;
- copier/coller page ou workspace entre profils ;
- remap correct des GUIDs ;
- preservation des IDs en remplacement ;
- pack `.lp5` deterministe.

Commandes cible :

```powershell
lpdeck inspect profile.lp5 --json
lpdeck extract profile.lp5 workdir
lpdeck pack workdir out.lp5
lpdeck validate workdir --strict
lpdeck diff a.lp5 b.lp5
lpdeck page copy --src live.lp5 --page "Sound control" --dst ct.lp5 --strategy replace --out ct_synced.lp5
lpdeck page generate profile.lp5 --from g1000.yaml --workspace 0 --out msfs_g1000.lp5
lpdeck workspace copy --src a.lp5 --workspace "Daily Use" --dst b.lp5 --strategy clone
```

Priorite immediate :

- stabiliser un core Python `lpdeck_core` ;
- ajouter tests de roundtrip CT, Live, MSFS ;
- valider/remapper les actions de navigation `ChangeTouchPage` et `ChangeEncoderPage` ;
- remplacer progressivement le generateur ad hoc G1000 par un YAML declaratif.

## 8. Publication Open Source

Licence recommandee : conserver `Apache-2.0`.

Raisons :

- adaptee a un plugin open source distribue en binaire ;
- protection brevet meilleure que MIT ;
- coherent avec le package existant.

Avant publication :

- auditer les assets, surtout `assets/G1000H-NXi-Hero-blog.jpg` ;
- ne pas publier d'image Garmin/Microsoft/Working Title non autorisee ;
- ajouter `NOTICE` si redistribution de DLLs ou assets tiers ;
- ajouter disclaimers marques ;
- clarifier redistribution ou non des fichiers WASimCommander ;
- nettoyer les exports personnels et outils experimentaux du perimetre public.

Disclaimer a utiliser :

```text
This project is an independent, community-made plugin. It is not affiliated with, endorsed by, sponsored by, or approved by Microsoft, Asobo Studio, Garmin, Working Title, Logitech, or Loupedeck. Microsoft Flight Simulator, Garmin, G1000, G1000 NXi, Logitech, Logi, and Loupedeck are trademarks of their respective owners.
```

Roadmap release :

- `v0.1.0-alpha.1` : testeurs techniques ;
- `v0.2.0-alpha` : actions utilisateur stables ;
- `v0.5.0-beta` : installation documentee + profils exemples ;
- `v1.0.0` : plugin stable, packaging, compatibilite documentee.

## 9. Backlog Priorise

### P0 - Stopper la derive visuelle

- supprimer les pages inutiles et les labels redondants ;
- revenir a une structure par univers : Garmin, Aviation Generale, Simulateur, puis Jets/Liners plus tard ;
- garantir qu'une page corresponde a un seul univers ou cas d'utilisation ;
- refaire le renderer en style bezel sombre ;
- masquer ou neutraliser proprement les titres Loupedeck quand ils doublonnent.

### P1 - Softkeys dynamiques

- spike `L:` vars softkeys ;
- support string WASim ;
- `G1000SoftkeyState` ;
- `G1000SoftkeyCommand` 24 touches ;
- pages PFD/MFD dynamiques ;
- fallback statique propre.

### P2 - Resilience

- `SimConnectionState` ;
- `ConnectionSupervisor` ;
- status action visible ;
- auto-reconnexion ;
- tests de panne.

### P3 - Profils declaratifs

- `lpdeck_core` ;
- YAML G1000 ;
- roundtrip tests ;
- remap GUIDs ;
- generation CT/Live propre.

### P4 - Publication

- README public ;
- disclaimers ;
- release alpha ;
- templates GitHub ;
- audit assets/licences.

## 10. Decision De Chef De Projet

La prochaine phase ne doit pas consister a ajouter encore des commandes. Elle doit refondre la base UX et technique autour de trois axes :

1. charte visuelle G1000 sombre ;
2. softkeys PFD/MFD dynamiques ;
3. resilience de connexion visible.

Les commandes supplementaires doivent etre ajoutees dans le bon univers. Les pages Garmin doivent rester Garmin. Les pages aviation generale doivent rester basees sur les actions/donnees MSFS generiques. Les pages simulateur doivent rester dediees au jeu et a ses outils. Les pages jets/liners viendront plus tard, uniquement avec des actions specifiques verifiees.
