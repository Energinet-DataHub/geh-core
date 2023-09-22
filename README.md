# Introduction

This repository is dedicated to code that will be shared between two or more domains. The shared code will be published as reusable components in the form of NuGet packages on [nuget.org](https://www.nuget.org/).

## Table of contents

1. [Folders structure](#folder-structure)
1. [Development](#development)

## Folder Structure

Artifacts should be organized in the following folder structure:

``` txt
.
├── .github/
│   ├── actions/
│   ├── workflows/
│   └── CODEOWNERS
│
├── .vscode/
│
├───documents
│   ├── development.md
│   └── release-notes
│       └──release-notes.md
│
├───source/
│   ├── <NuGet package bundle>
│   ├── Directory.Build.props
│   └── stylecop.json
│
├── .editorconfig
├── .gitignore
├── .licenserc.json
├── codecov.yml
├── LICENSE
└── README.md
```

### Root

Contains:

- `.editorconfig` file for configuration of Formatting, Code Style and Analyzers (including StyleCop).
- `.gitignore` file that defines which files should be ignored (not checked in) by Git.
- `.licenserc.json` file that defines the expected license header of certain file types.
- `codecov.yml`file contains the CodeCov configuration outlining the flags/projects where code coverage is tracked.
- `LICENSE` License information for all code within this repository.
- `README.md` file that gives an introduction to this repository.

### Folder: .github

Actions and workflows spanning [NuGet package bundles](./docs/development.md#nuget-package-bundle).

We should name workflows in a way that allows us to easily identify which package bundle they belongs to.

We use the following workflow postfix format:

- `*--bundle-publish.yml` for workflows that build and tests a package bundle.

#### File: CODEOWNERS

Define the maintainers of this repository, one of which must approve any given pull request.

### Folder: .vscode/

Contains VS Code workspace configuration which is relevant for any developer working in the repository. E.g. if we want to ensure all obey a certain setting we can configure it in `settings.json`.

### Folder: documents

Contains notes and documentation stored in `*.md` files.

### Folder: source/

Each [NuGet package bundles](./docs/development.md#nuget-package-bundle) should have one root folder within the `source` folder. For organization within each package bundle, see [Organization of packages](./docs/development.md).

Any configuration files (e.g. `stylecop.json`) shared by _all_ VS projects must be located in the `source` folder.

#### File: Directory.Build.props

VS project properties that we want to set on _all_ VS projects must be defined in the `Directory.Build.props`.

Never put NuGet package dependencies (except the style cop analyzers dependency) in this file, keep them in each VS project.

The same goes for VS project properties that could cause breaking changes that we cannot easily detect, like `TargetFramework`.

## Development

For details on package bundle organization and development see [development.md](./docs/development.md).
