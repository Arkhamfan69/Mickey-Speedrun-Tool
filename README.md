# Mickey Speedrun Tool

A Windows utility for managing Epic Mickey speedrun saves, launching the game via Steam, and installing UE4SS.

## Features

- Launch the game through Steam
- Auto-detect UE4SS install path and install UE4SS from bundled source files
- Manage save categories for `Any%`, `100%`, and `IL` runs
- Browse and load save files by category and area
- Configure the game save folder manually
- Simple dashboard with key actions and status indicators

## Requirements

- Windows
- .NET 8 runtime
- Steam installed for game launch support

## Getting Started

## Usage

1. Open the app.
2. Use the dashboard to:
   - Install UE4SS
   - Locate the game executable/save folder
   - Launch the game through Steam
3. Open the `Saves` tab.
4. Select a category like `Any%` or `100%`.
5. Choose an area from the dropdown.
6. Load the desired save folder.

## Folder Structure

- `Save Files/` contains bundled save categories
- `Save Files/Any%` and `Save Files/100%` are expected to contain subfolders for run areas
- `Save Files/IL's` contains flat IL save folders

## Notes

- The save browser is designed to show top-level categories and then area-specific save folders.
- If you want to distribute the app, package the published output and include the `Save Files` content if required.

## Release

This project is currently ready for a `v1.0` release.

## License

Add your preferred license here.
