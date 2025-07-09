# Claude Commands

This directory contains organized command definitions for the Rinzler78.NetExtension project.

## Command Categories

### Build Commands (`build.md`)
- `build` - Fast build for development iterations
- `build-release` - Build in Release mode for production
- `build-full` - Build entire solution (with potential warnings)
- `clean` - Clean build artifacts

### Package Management (`package.md`)
- `restore` - Restore NuGet packages
- `pack` - Create NuGet package

### Development Tools (`development.md`)
- `dev-cycle` - Complete development cycle with status feedback
- `analyze` - Run code analysis with full output

## Usage

Each command file contains both documentation and the actual command definitions. You can source these files or copy the command definitions to your shell configuration.

## Quick Reference

```bash
# Most common development commands
build           # Fast build
build-release   # Release build
dev-cycle       # Build with status
analyze         # Full analysis
clean           # Clean artifacts
restore         # Restore packages
pack            # Create package
```