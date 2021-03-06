[![codecov](https://codecov.io/gh/Energinet-DataHub/geh-core/branch/main/graph/badge.svg?token=CXGH54CZ85)](https://codecov.io/gh/Energinet-DataHub/geh-core)

# Introduction

This repository is dedicated to code that will be shared between two or more domains. The shared code will be published as reusable components in the form of NuGet packages on [nuget.org](https://www.nuget.org/).

## Links

- [development.md](./documents/development.md)

## Folder Structure

Artifacts should be organized in the following folder structure:

``` txt
<root>
│   .editorconfig
│   .gitignore
│   .licenserc.json
|   codecov.yml
│   LICENSE
│   README.md
│
├───.github
│       └───actions
│       └───workflows
│
├───documents
│      development.md
│
└───source
```

### `root`

Contains:

- `.editorconfig` file for configuration of Formatting, Code Style and Analyzers (including StyleCop).
- `.gitignore` file that defines which files should be ignored (not checked in) by Git.
- `.licenserc.json` *TODO: Add a description.*
- `codecov.yml`file contains the CodeCov configuration outlining the flags/projects where code coverage is tracked.
- `LICENSE` *TODO: Add a description.*
- `README.md` file that gives an introduction to this repository.

### `.github`

Contains GitHub workflow and action (`*.yml`) files for establishing build pipelines.

### `documents`

Contains notes and documentation stored in `*.md` files.

### `source`

Contains libraries in subfolders.

For details on library organization and development see [development.md](./documents/development.md).
