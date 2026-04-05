## ADDED Requirements

### Requirement: No Unsafe Blocks
The library project SHALL NOT enable `AllowUnsafeBlocks`. No unsafe C# code exists
in the library and the flag SHALL be absent from the `.csproj`.

#### Scenario: Csproj does not allow unsafe
- **WHEN** `Rinzler78.NetExtension.Standard.csproj` is inspected
- **THEN** `<AllowUnsafeBlocks>` is not present in any `PropertyGroup`

## ADDED Requirements

### Requirement: TestResults Not Tracked by Git
The `TestResults/` directory produced by `dotnet test` SHALL be listed in `.gitignore`
and SHALL NOT be committed to the repository.

#### Scenario: gitignore covers TestResults
- **WHEN** `.gitignore` is inspected
- **THEN** a pattern matching `**/TestResults/` is present

#### Scenario: No TestResults in tracked files
- **WHEN** `git ls-files src/` is run
- **THEN** no `.trx` or `coverage.cobertura.xml` files appear in the output

## MODIFIED Requirements

### Requirement: Package Path Uses Forward Slash
The `README.md` None item SHALL use `PackagePath=""` (empty string) to place the
readme at the package root, ensuring cross-platform NuGet packing compatibility.

#### Scenario: Pack succeeds on Linux with README at root
- **WHEN** `dotnet pack --configuration Release` is run on ubuntu-latest
- **THEN** the `.nupkg` contains `README.md` at its root level
