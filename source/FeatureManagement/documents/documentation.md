# Documentation

## Overview

- [Introduction](#introduction)
- [Guidelines](#guidelines)
  - [General principles](#general-principles)
  - [Document feature flags](#document-feature-flags)
- Samples
  - [Disabled flag](./samples.md#disabled-flag)
  - [Feature flag](./samples.md#feature-flag)
- Quickstarts
  - [Feature flag](./quickstarts-feature-flag.md)

## Introduction

This is a sample showing how we by use of simple techniques, and a few common guidelines, can support feature flags in an Azure Functions App.

It is meant to be a simple solution, as we aim for a minimum viable product.

We show two techniques:

- Use of *disabled flags* to disable functions.

- Use of the Microsoft Feature Management libraries for handling *feature flags*.

### Configuration at deployment time

Configuration depends on app settings, which must be configured in *infrastructure as code*.

This means we can enable/disable functionality at deployment time, but not at runtime.

It is however possible to have different configurations per environment, which means we can e.g. enable a feature in development/test but disable it in pre-production/production.

## Guidelines

### General principles

- DO keep the number of active feature flags low in an area at all times.
  - Aim for having short lived feature flags, and remove them as soon as they are obsolete.
- DO use feature flags to enable/disable functionality at a high level, like:
  - Enable/disable a function using a *disabled flag*.
  - Enable/disable a functionality at an application level by using a *feature flag*.
- DO NOT use feature flags to enable/disable functionality at a low level, like:
  - Enable/disable functionality deep within a component.

### Document feature flags

- DO document all active feature flags within an area, in a `development.md` file or other *easy to spot* place.
- DO add a section named *Active feature flags* close to the top of the choosen `*.md` file.
- DO add the following table to the *Active feature flags* section:

| Name | Purpose | Must be removed when |
| ---- | ------- | ------------------- |
| A name | A purpose | Explain the condition under which we can remove this feature flag again |

- DO document when a feature flag can be removed so we continuously have focus on keeping the number of active feature flags low.
