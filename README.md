# Dofus Data Center (DDC)

Extract and release data from Dofus.\
The main feature of DDC is that the data is extracted by a CI script whenever a new version of Dofus is released.

Tools:
- [cytrus-v6](https://github.com/Dofus-Batteries-Included/cytrus-v6): get latest game version and download game files without the ankama launcher
- [BepInEx](https://github.com/BepInEx/BepInEx): inject the data extractor plugin to the Dofus client

# Download the extracted data

- Directly from the [Releases](https://github.com/Dofus-Batteries-Included/DDC/releases) of this repository: the data will always be packaged in a `data.zip` archive. The archive contains a `metadata.json` file containing a `GameVersion` and `GameBuildId` fields that provide info about the version of the game that the data is extracted from
- Using the [Data Center API](https://api.dofusbatteriesincluded.fr/swagger/index.html?urls.primaryName=data-center): it exposes all the releases of this repository through REST APIs. The raw files are available through the endpoints in the 'Raw data' group.
See also [DBI.Api](https://github.com/Dofus-Batteries-Included/DBI.Api).

# How does it work?

Instead of reverse engineering the assets of the game, DDC uses the game client itself to retrieve the data and export it as JSON.
The extraction itself is implemented in the [DDC.Extractor](https://github.com/Dofus-Batteries-Included/DDC/tree/main/DDC.Extractor) project, it is a BepInEx plugin that is injected to the unity application at startup. It reads data mainly from the `DataCenterModule` class of the client.

The extraction itself is performed by the [Extract game data](https://github.com/Dofus-Batteries-Included/DDC/blob/main/.github/workflows/release.yml) workflow that runs on every release. Releases are automatically triggered by the [Poll game update](https://github.com/Dofus-Batteries-Included/DDC/blob/main/.github/workflows/poll_game_update.yml) workflow when new versions of the game are published.