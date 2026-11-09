# dsjob-net

An example command-line utility for IBM DataStage, written in C# and using the [RKM.DataStage.DSAPI](https://www.nuget.org/packages/RKM.DataStage.DSAPI/) NuGet package.

This demonstrates the **low-level** DataStage DSAPI wrapper, not the higher-level API planned for future work.

## Build

Open the `RKM.DataStage.Examples.slnx` solution in Visual Studio, restore NuGet packages and build for `net10.0-windows` / **x86**.

The project consumes NuGet package `RKM.DataStage.DSAPI` version `0.1.0-preview.1`. There is no project reference to the library source.

To execute against DataStage, the Windows machine needs the licensed IBM 32-bit `vmdsapi.dll` and its dependent native runtime components. This repository does not include them.

## Example commands

Use the executable `dsjob-net.exe` from your build output directory:

```text
dsjob-net -authfile "C:\Secure\dsjob-net.auth.json" -lprojects
dsjob-net -authfile "C:\Secure\dsjob-net.auth.json" -ljobs ExampleProject
dsjob-net -authfile "C:\Secure\dsjob-net.auth.json" -jobinfo ExampleProject ExampleJob
dsjob-net -authfile "C:\Secure\dsjob-net.auth.json" -lparams ExampleProject ExampleJob
```

Other available commands include `-run`, `-stop`, `-lstages`, `-llinks`, `-stageinfo`, `-linkinfo`, `-paraminfo`, `-log`, `-logsum`, `-logdetail` and `-lognewest`. Run without arguments to see primary command choices. Commands have different arguments.

### Authentication file

Create a private JSON file such as `C:\Secure\dsjob-net.auth.json`, using [authfile.example.json](authfile.example.json) as a template:

```json
{
  "domain": "NONE",
  "server": "datastage.example.com",
  "username": "YOUR_DATASTAGE_USERNAME",
  "password": "REPLACE_WITH_YOUR_PASSWORD"
}
```

`username` and `password` are required, non-empty strings. `domain` and `server` are optional; `"NONE"` for domain selects default/null behaviour. Property names are case-insensitive; unknown fields, invalid JSON and missing credentials are rejected. Place `-authfile` **before** the primary command.

`-authfile` cannot be combined with inline `-domain`, `-server`, `-user` or `-password` arguments. The original switches remain supported, but inline passwords can appear in command history or process listings.

**Security:** The authentication JSON contains a plaintext password. Do not put real credentials in `authfile.example.json`. Keep populated credentials outside the repository, protect them with Windows file permissions and never commit them. Git ignore patterns provide only an additional safety net.

## Scope and licence

This is an illustrative utility designed to exercise DSAPI. Command handlers return status codes and display relevant native error messages where available. Execution requires Windows x86 because of the IBM native runtime.

MIT, see the [repository licence](../../LICENSE). Historical DataStage sample acknowledgement is retained in `EntryPoint.cs`.
