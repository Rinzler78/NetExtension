# Extensions nécessaires pour Test Explorer dans Cursor

## 🎯 Extensions à installer immédiatement

### Dans Cursor : Extensions (Ctrl+Shift+X)

#### 1. Extension C# principale
- **`ms-dotnettools.csharp`** (C# for Visual Studio Code)

#### 2. Extensions Test Explorer
- **`formulahendry.dotnet-test-explorer`** (.NET Core Test Explorer)
- **`hbenl.vscode-test-explorer`** (Test Explorer UI)

#### 3. Extensions support
- **`ms-dotnettools.vscode-dotnet-runtime`** (.NET Runtime)

## 🔧 Instructions d'installation

1. **Ouvrir Cursor**
2. **Ctrl+Shift+X** (ou cliquer sur l'icône Extensions)
3. **Rechercher et installer** chaque extension listée ci-dessus
4. **Redémarrer Cursor** après installation

## ✅ Vérification

Après installation et redémarrage :
- **Icône Testing** 🧪 doit apparaître dans la barre latérale
- **View → Testing** doit être disponible dans le menu
- **Code Lens** "Run Test | Debug Test" doit apparaître au-dessus des méthodes [Fact]

## 🚨 Si Test Explorer n'apparaît pas

Alternative : **Ctrl+Shift+P** → "Test: Focus on Test Explorer View"