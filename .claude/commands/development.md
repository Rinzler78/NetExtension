# Development Commands

## dev-cycle
Complete development cycle with status feedback
```bash
dev-cycle() {
    echo "🔄 Running development cycle..."
    dotnet build src/Rinzler78.NetExtension.Standard/Rinzler78.NetExtension.Standard.csproj --no-restore --verbosity minimal
    if [ $? -eq 0 ]; then
        echo "✅ Build successful!"
    else
        echo "❌ Build failed!"
    fi
}
```

## analyze
Run code analysis with full output
```bash
analyze() {
    echo "🔍 Running code analysis..."
    dotnet build src/Rinzler78.NetExtension.Standard/Rinzler78.NetExtension.Standard.csproj --configuration Debug --verbosity normal
}
```