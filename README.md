# ChangeElementLevel (Revit)

Add-in para o Autodesk Revit que altera o nível de referência dos elementos selecionados mantendo a posição 3D (offset recalculado).

## Requisitos

- **Revit 2025** (64 bits), com API instalada em `Program Files` (o projeto referencia `RevitAPI.dll` / `RevitAPIUI.dll` dessa versão).
- **.NET 8 SDK** (Windows), para compilar o add-in e o build Nuke.

## Instalação para utilizadores (sem mexer em ficheiros à mão)

1. Obtém o ficheiro **`ChangeElementLevel-Revit2025-Setup.exe`** (gerado pelo build; ver abaixo).
2. Executa o instalador e conclui o assistente.
3. Os ficheiros são copiados para `%AppData%\Autodesk\Revit\Addins\2025\` (DLL, manifesto `.addin` e pasta `i18n`).
4. Abre o **Revit 2025**; o add-in deve aparecer no separador da ribbon.

O desinstalador fica em `%LocalAppData%\Programs\ChangeElementLevel` (não na pasta de add-ins), para não misturar com outros manifestos.

> O ano **2025** na pasta de add-ins corresponde ao valor usado na build. Se no futuro publicares para outra versão do Revit, gera outro instalador com `--revit-year` (ver secção de build).

## Build e instalador com Nuke

O repositório inclui um projeto Nuke em `build/` que:

1. Faz **restore** e **build** do `ChangeElementLevel.csproj` em **Release** com plataforma **x64**.
2. Copia a saída para `artifacts/staging`.
3. Compila o script **Inno Setup** (`installer/ChangeElementLevel.iss`) com o pacote NuGet **Tools.InnoSetup** (não é necessário instalar o Inno Setup à parte no PATH).

### Comando rápido (PowerShell, na raiz do repositório)

```powershell
.\build.ps1
```

Ou diretamente:

```powershell
dotnet run --project build\_build.csproj -c Release
```

O executável do instalador fica em:

`artifacts\installer\ChangeElementLevel-Revit2025-Setup.exe`

(nome inclui o ano do Revit configurado.)

### Parâmetros úteis

| Parâmetro | Descrição | Exemplo |
|-----------|-----------|---------|
| `--installer-version` | Versão mostrada no instalador | `--installer-version 1.2.0` |
| `--revit-year` | Subpasta `Addins\{ano}` de destino | `--revit-year 2026` |
| `--target` | Alvo Nuke (por omissão o `Main` invoca **Pack**) | `--target Clean` |

Exemplo:

```powershell
.\build.ps1 --installer-version 1.1.0 --revit-year 2026
```

### Estrutura relevante

- `ChangeElementLevel/` — código do add-in e `ChangeElementLevel.addin`.
- `build/Build.cs` — targets Nuke (`Restore`, `Compile`, `StageAddin`, `Installer`, `Pack`).
- `installer/ChangeElementLevel.iss` — definição do instalador Windows (Inno Setup).
- `.nuke/parameters.json` — parâmetros opcionais do Nuke (podes deixar vazio `{}`).

## Desenvolvimento

Abre `ChangeElementLevel.slnx` no Visual Studio ou usa `dotnet build ChangeElementLevel\ChangeElementLevel.csproj -c Debug` (com Revit 2025 instalado para resolver as referências).

## Licença

Define no repositório conforme a tua política (este README não impõe licença).
