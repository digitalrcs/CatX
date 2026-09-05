#define MyAppName "CatX Keyboard Guard"
#ifndef MyAppVersion
  #define MyAppVersion "1.1.0"
#endif
#define MyAppPublisher "Dan Roberts DigitalRCS"
#define MyAppExeName "CatX.exe"

[Setup]
AppId={{B7DEB791-848A-44C6-9CC6-4CB34D80B176}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://github.com/digitalrcs/CatX
AppSupportURL=https://github.com/digitalrcs/CatX/issues
AppUpdatesURL=https://github.com/digitalrcs/CatX/releases
DefaultDirName={localappdata}\Programs\CatX
DefaultGroupName=CatX
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
InfoBeforeFile=INSTALL_INFO.txt
OutputDir=..\dist
OutputBaseFilename=CatX-Setup-{#MyAppVersion}
SetupIconFile=..\src\CatX\Assets\CatX.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.22000
CloseApplications=yes
RestartApplications=no
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=CatX Keyboard Guard Installer
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}
VersionInfoVersion={#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "..\publish\CatX\CatX.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\output\pdf\CatX-User-Guide-v1.0.3.pdf"; DestDir: "{app}\Documentation"; DestName: "CatX-User-Guide.pdf"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\docs\ANIMATED_CATS.md"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\docs\USER_GUIDE.md"; DestDir: "{app}\Documentation"; Flags: ignoreversion

[Icons]
Name: "{group}\CatX Keyboard Guard"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{group}\CatX User Guide"; Filename: "{app}\Documentation\CatX-User-Guide.pdf"
Name: "{autodesktop}\CatX Keyboard Guard"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch CatX"; Flags: nowait postinstall skipifsilent
