# Build Commands

## build
Fast build for development iterations
```bash
build() {
    echo "🔨 Building NetExtension library..."
    dotnet build src/Rinzler78.NetExtension.Standard/Rinzler78.NetExtension.Standard.csproj --no-restore --verbosity minimal
}
```

## build-release
Build in Release mode for production
```bash
build-release() {
    echo "🚀 Building NetExtension library in Release mode..."
    dotnet build src/Rinzler78.NetExtension.Standard/Rinzler78.NetExtension.Standard.csproj --configuration Release --no-restore --verbosity minimal
}
```

## build-full
Build entire solution (with potential warnings)
```bash
build-full() {
    echo "🏗️ Building full NetExtension solution..."
    dotnet build src/Rinzler78.NetExtension.sln --verbosity minimal
}
```

## clean
Clean build artifacts
```bash
clean() {
    echo "🧹 Cleaning build artifacts..."
    dotnet clean src/Rinzler78.NetExtension.sln
}
```