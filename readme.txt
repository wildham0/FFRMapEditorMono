FFR Map Editor v1.02

Credits ........... [S01]
Version Changes ... [S02]
Controls .......... [S03]
Settings .......... [S04]

*** Credits *** [S01]
Main Developer
  wildham

Icons Designer
  DarkmoonEx

Based on the work of
  tetron (Overworld Map Format)
  madmartin (Enabling Logic)

Source code at
  https://github.com/wildham0/FFRMapEditorMono

*** Version Change *** [S02]
1.02: - Added Undo/Redo options for paiting actions on the map.
      - Added automatic backup ("backupowmap.json" in root folder)
1.01: - Added safeguards to New Map and Load Map functions.
      - New Map now clear Map Objects.
1.00: - Initial Release

*** Controls *** [S03]
Main Map:
 Left-click: Use selected tool
 Right-click: Select current tile
 Mousewheel: Zoom-in/zoom-out

New File:
 Create a new map of sea tiles only.

Load File:
 Load a FFR Map (json) or a FFHackster map (ffm).

Save File/Save File As:
 Save the current map as a FFR Map (json) or a FFHackster map (ffm).
 Note: FFHackster map files won't include domains, docks and objects info.

Pencil Tool:
 Right-click: Show the tile picker.
  X: The tile isn't found on the map.
  Arrow Down: The tile is found on the map.
 On Map: Place a single tile of the current selected tile.

Smart Brush Tool:
 Right-click: Show the brush picker.
 Left-click: Increase brush size.
 On Map: Place an area of the current selected tile, adjusting the borders to match the surrounding tiles.
 LCTRL+Mousewheel: Increase/decrease brush size.

Templates Tool:
 Select a predrawn template.
 On Map: Place the template.

Domains Overlay:
 Select an encounter domain.
 On Map: Set selected zone to the selected domain.

Docks Overlay:
 Select a dock location (if the ship is found at that location, the ship will spawn at that dock).
  X: The dock isn't placed.
  Arrow Down: The dock is placed.
 On Map: Set selected dock at location.

Map Objects Overlay:
 Select a map object location. The Ship object isn't used (see Docks Overlay).
  X: The object isn't placed.
  Arrow Down: The object is placed.
 On Map: Set selected object at location.

Undo:
 Cancel the previous painting action on the map.
 Keyboard shortcut: LCTRL+Z

Redo:
 If undo was used, go forward to current actions.
 Keyboard shortcut: LSHIFT+LCTRL+Z

Exit:
 Exit Program

Info:
 Show credits, version, urls.
 
 *** Settings *** [S04]
 These settings can be modified in the settings.json file in the root folder. If the file isn't there, running and quitting the map editor will create it.

 Resolution X/Y: Default window size.
 Undo Depth: How many actions back that can be undone. Default is 4, higher count means higher memory usage.
 Backup Delay: How often the backup map is saved. Default is 5 minutes.

