[Setup]
AppId={{8D3A9B6D-3A5B-4D6A-9F54-3D2B8E9B7A11}
AppName=GuardianShield
AppVersion=3.0.0
AppPublisher=GuardianShield

DefaultDirName={autopf}\GuardianShield
DefaultGroupName=GuardianShield

OutputDir=output
OutputBaseFilename=GuardianShield-Setup

Compression=lzma
SolidCompression=yes
WizardStyle=modern

ArchitecturesInstallIn64BitMode=x64compatible

PrivilegesRequired=admin

SetupIconFile=..\shield.ico
UninstallDisplayIcon={app}\GuardianShield.exe

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{autoprograms}\GuardianShield"; Filename: "{app}\GuardianShield.exe"
Name: "{autodesktop}\GuardianShield"; Filename: "{app}\GuardianShield.exe"

[Run]
Filename: "{app}\GuardianShield.exe"; Description: "Launch GuardianShield"; Flags: nowait postinstall skipifsilent
