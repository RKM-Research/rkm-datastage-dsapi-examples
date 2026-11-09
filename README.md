# RKM DataStage DSAPI examples

Public C#/.NET example applications demonstrating IBM DataStage integration with libraries published by **RKM Research Ltd**.

The examples use **published NuGet packages**, not project references to the library source code.

## Available examples

| Example | Description | Dependency |
| --- | --- | --- |
| [dsjob-net](low-level/console/dsjob-net/) | Windows x86 command-line tool for DataStage projects, jobs, metadata, parameters and logs | [RKM.DataStage.DSAPI](https://www.nuget.org/packages/RKM.DataStage.DSAPI/) |

The repository is organised so that future examples using a higher-level RKM DataStage .NET library, as well as advanced integration scenarios, can be added separately when ready.

## Getting started

1. Clone this repository or download the source ZIP.
2. Open `RKM.DataStage.Examples.slnx` in Visual Studio with .NET 10 support.
3. Restore NuGet packages and build `dsjob-net` for **Windows x86**.
4. Run it on a Windows system with compatible IBM DataStage 32-bit native DSAPI components installed.

See the [dsjob-net README](low-level/console/dsjob-net/README.md) for commands and authentication-file instructions.

## Requirements

- Windows x86 (32-bit) process for the native IBM DSAPI runtime
- .NET 10 SDK and compatible Visual Studio
- Compatible IBM DataStage `vmdsapi.dll` and native dependencies, installed and accessible to the client
- Appropriate DataStage credentials and network access

A 64-bit .NET process cannot load the 32-bit native DSAPI library. **No IBM binaries, IBM licences or DataStage credentials are distributed with these examples.**

## Related projects

- [RKM.DataStage.DSAPI source](https://github.com/RKM-Research/rkm-datastage-dsapi)
- [RKM.DataStage.DSAPI NuGet package](https://www.nuget.org/packages/RKM.DataStage.DSAPI/)
- [RKM Research](https://rkmresearch.com)

## Licence and acknowledgement

Licensed under the [MIT License](LICENSE).

The `dsjob-net` application was rewritten and extended in C# from the historical DataStage `dsjob` command-line example published in the DataStage Server Job Developer's Guide. Its source header retains that acknowledgement.

IBM and DataStage are trademarks of their respective owners. This independent project is not affiliated with or endorsed by IBM.
