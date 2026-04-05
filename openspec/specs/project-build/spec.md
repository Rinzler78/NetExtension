# Capability: Project Build Configuration

## Purpose
Defines the MSBuild configuration for the library project (`Rinzler78.NetExtension.Standard`)
and test project, including compiler settings, package metadata, and tool references.

## Requirements

### Requirement: No Unsafe Blocks
The library project SHALL NOT enable `AllowUnsafeBlocks`. No unsafe C# code exists
in the library and the flag SHALL be absent from the `.csproj`.

#### Scenario: Csproj does not allow unsafe
- **WHEN** the `.csproj` file is inspected
- **THEN** `<AllowUnsafeBlocks>` is not present or is explicitly `false`

### Requirement: TestResults Not Tracked by Git
The `TestResults/` directory produced by `dotnet test` SHALL be listed in `.gitignore`
and SHALL NOT be committed to the repository.

#### Scenario: gitignore covers TestResults
- **WHEN** `.gitignore` is inspected
- **THEN** a pattern matching `**/TestResults/` is present

#### Scenario: No TestResults in repo
- **WHEN** the repository tree is inspected
- **THEN** no `.trx` or `coverage.cobertura.xml` files are present in tracked history

### Requirement: Package Path Uses Empty String
The `README.md` None item SHALL use `PackagePath=""` (empty string) to place the readme
at the package root, ensuring cross-platform NuGet packing compatibility.

#### Scenario: Pack succeeds on Linux
- **WHEN** `dotnet pack` is run on Linux
- **THEN** the README.md is included at the root of the NuGet package
