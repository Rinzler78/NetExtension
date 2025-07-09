# Package Management Commands

## restore
Restore NuGet packages
```bash
restore() {
    echo "📦 Restoring NuGet packages..."
    dotnet restore src/Rinzler78.NetExtension.sln
}
```

## pack
Create NuGet package
```bash
pack() {
    echo "📦 Creating NuGet package..."
    dotnet pack src/Rinzler78.NetExtension.Standard/Rinzler78.NetExtension.Standard.csproj --configuration Release --output ./artifacts
}
```