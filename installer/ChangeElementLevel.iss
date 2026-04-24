; Ficheiros do add-in em %AppData%\Autodesk\Revit\Addins\{ano}\ (manifesto na raiz, como exige o Revit).
; O desinstalador fica em %LocalAppData%\Programs\ChangeElementLevel para nao misturar com outros .addin.
#ifndef StagingDir
  #define StagingDir "..\artifacts\staging"
#endif
#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#ifndef RevitYear
  #define RevitYear "2025"
#endif

#define MyAppName "ChangeElementLevel"

[Setup]
AppId={{8F3E2B1A-6C5D-4E4F-9A0B-1C2D3E4F5A6C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=ChangeElementLevel
DefaultDirName={localappdata}\Programs\{#MyAppName}
DisableDirPage=yes
DisableProgramGroupPage=yes
OutputBaseFilename=ChangeElementLevel-Revit{#RevitYear}-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"

[Files]
Source: "{#StagingDir}\ChangeElementLevel.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\{#RevitYear}"; Flags: ignoreversion
Source: "{#StagingDir}\ChangeElementLevel.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\{#RevitYear}"; Flags: ignoreversion
Source: "{#StagingDir}\ChangeElementLevel.deps.json"; DestDir: "{userappdata}\Autodesk\Revit\Addins\{#RevitYear}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "{#StagingDir}\ChangeElementLevel.runtimeconfig.json"; DestDir: "{userappdata}\Autodesk\Revit\Addins\{#RevitYear}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "{#StagingDir}\i18n\*"; DestDir: "{userappdata}\Autodesk\Revit\Addins\{#RevitYear}\i18n"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
