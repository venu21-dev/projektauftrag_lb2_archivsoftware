\# 📁 Archivsoftware - Dokumentenverwaltung für PDF \& DOCX



> Desktop-Anwendung zur automatischen Ablage, Verwaltung und Suche von Dokumenten in einer SQL-Server-Datenbank



!\[Status](https://img.shields.io/badge/Status-In%20Development-yellow)

!\[.NET](https://img.shields.io/badge/.NET-6.0%2B-blue)

!\[License](https://img.shields.io/badge/License-Educational-green)



---



\## 📋 Projektübersicht



\*\*Projektname:\*\* Archivsoftware für PDF- und Word-Dokumente  

\*\*Team:\*\* Jegatheeswaran Mathumithan \& Manivannan Venurshan  

\*\*Kurs:\*\* PROG1 - LB2  

\*\*Technologie:\*\* C# WPF, Entity Framework Core, SQL Server



\### Zielsetzung



Entwicklung einer Desktop-Anwendung, die eine automatische Ablage, Verwaltung und Volltextsuche von PDF- und DOCX-Dokumenten in einer SQL-Server-Datenbank ermöglicht. 

Der Fokus liegt auf GUI-Usability, Datenbankanbindung via Entity Framework Core, einer sauberen Schichtenarchitektur und hoher Codequalität.



---



\## ✨ Hauptfunktionen



\### 🗂️ Ordnerverwaltung

\- Hierarchische Ordnerstruktur (anlegen, umbenennen, verschieben, löschen)

\- Validierung: Eindeutige Namen pro Ebene

\- Intuitive Baumansicht mit auf-/zuklappbaren Unterordnern



\### 📄 Dateiimport

\- \*\*Automatisch:\*\* Überwachter Ordner (FileSystemWatcher)

\- \*\*Manuell:\*\* Import per GUI (FileDialog)

\- Unterstützte Formate: PDF, DOCX

\- Speicherung als BLOB + extrahierter PlainText in Datenbank



\### 🔍 Volltextsuche

\- Suche über extrahierten PlainText (SQL LIKE)

\- Anzeige der Treffer mit:

&nbsp; - Dokumenttitel

&nbsp; - Ordnerpfad

&nbsp; - Text-Snippet mit Highlight des Suchbegriffs

\- Echtzeit-Filterung der Suchergebnisse



\### 🖥️ Benutzeroberfläche

\- \*\*Links:\*\* Ordnerbaum (TreeView) zur Navigation

\- \*\*Mitte:\*\* Suchergebnisse / Dokumentenliste (ListView)

\- \*\*Rechts:\*\* Detailansicht des ausgewählten Dokuments

\- Dunkles, modernes Design für angenehmes Arbeiten



---



\## 🏗️ Architektur



Das Projekt folgt einer \*\*3-Schichten-Architektur\*\*:



```

ArchivsoftwareApp/

│

├── Presentation/              # GUI Layer (WPF)

│   ├── MainWindow.xaml       # Hauptfenster

│   ├── ViewModels/           # MVVM ViewModels

│   └── Controls/             # Custom Controls

│

├── Business/                  # Business Logic Layer

│   ├── Services/             # Business Services

│   │   ├── DocumentService.cs

│   │   ├── FolderService.cs

│   │   └── SearchService.cs

│   └── Models/               # Business Models

│

└── DataAccess/                # Data Access Layer

&nbsp;   ├── Entities/             # Database Entities

&nbsp;   │   ├── Folder.cs

&nbsp;   │   └── Document.cs

&nbsp;   ├── Repositories/         # Repository Pattern

&nbsp;   │   ├── IFolderRepository.cs

&nbsp;   │   ├── FolderRepository.cs

&nbsp;   │   ├── IDocumentRepository.cs

&nbsp;   │   └── DocumentRepository.cs

&nbsp;   └── AppDbContext.cs       # EF Core DbContext

```



---



\## 🛠️ Technologie-Stack



\### Frameworks \& Libraries

\- \*\*.NET 8\*\* - Application Framework

\- \*\*WPF\*\* - Windows Presentation Foundation für GUI

\- \*\*Entity Framework Core 7.0+\*\* - ORM für Datenbankzugriff

\- \*\*SQL Server Express LocalDB\*\* - Datenbank



\### NuGet-Pakete

```xml

<PackageReference Include="Microsoft.EntityFrameworkCore" Version="7.0+" />

<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.0+" />

<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="7.0+" />

<PackageReference Include="iTextSharp" Version="5.5+" />

<PackageReference Include="DocumentFormat.OpenXml" Version="2.20+" />

```



\### Entwicklungstools

\- \*\*Visual Studio 2022\*\* - IDE

\- \*\*Git / GitHub\*\* - Versionsverwaltung

\- \*\*SQL Server Management Studio (SSMS)\*\* - Datenbankverwaltung (optional)



---



\## 🚀 Installation \& Setup



\### Voraussetzungen

\- Windows 10/11

\- Visual Studio 2022 Community (oder höher)

\- .NET 6/7/8 SDK

\- SQL Server Express LocalDB (kommt mit Visual Studio)



---



\## 📖 Features



\### Ordner erstellen

1\. Klicke auf \*\*"+ Neuer Ordner"\*\*

2\. Gib einen eindeutigen Namen ein

3\. Ordner erscheint in der Ordnerstruktur links



\### Dokument importieren



\#### Manuell:

1\. Klicke auf \*\*"Importieren"\*\*

2\. Wähle eine PDF- oder DOCX-Datei

3\. Dokument wird in den aktuell ausgewählten Ordner importiert



\#### Automatisch:

1\. Konfiguriere den Überwachungsordner (in Settings)

2\. Kopiere PDF/DOCX in den Überwachungsordner

3\. Dokument wird automatisch importiert



\### Dokument suchen

1\. Gib ein Stichwort in das \*\*Suchfeld\*\* oben ein

2\. Drücke Enter oder klicke auf Suchen

3\. Ergebnisse werden in der Mitte angezeigt mit Snippet

4\. Klicke auf ein Ergebnis → Dokument öffnet sich rechts



\### Dokument anzeigen

1\. Klicke auf ein Dokument in der Liste oder im Baum

2\. Dokumentinhalt wird rechts angezeigt

3\. Suchbegriffe werden gelb markiert (bei aktiver Suche)



---



\## 🧪 Testing



Das Projekt umfasst umfassende Tests zur Qualitätssicherung:



\### Black-Box Tests (18 Tests)

\- \*\*Ordnerverwaltung\*\* (BB-01 bis BB-06): Erstellen, Umbenennen, Verschieben, Löschen

\- \*\*Dateiimport\*\* (BB-07 bis BB-11): Manuell/Automatisch, Validierung

\- \*\*Suche\*\* (BB-12 bis BB-14): Volltextsuche, Treffer-Anzeige

\- \*\*GUI\*\* (BB-15 bis BB-18): Benutzerinteraktionen, Layout



\### White-Box Tests (21 Tests)

\- \*\*Datenbank\*\* (WB-01 bis WB-05): CRUD-Operationen, Validierung

\- \*\*Text-Extraktion\*\* (WB-06 bis WB-10): PDF/DOCX Parsing

\- \*\*Suche\*\* (WB-11 bis WB-14): Algorithmen, Edge Cases

\- \*\*File Watcher\*\* (WB-15 bis WB-17): Event-Handling

\- \*\*GUI-Code\*\* (WB-18 bis WB-21): Binding, Exception-Handling



\### Tests ausführen

```bash

\# Alle Tests ausführen

dotnet test



\# Spezifische Test-Suite

dotnet test --filter "Category=BlackBox"

dotnet test --filter "Category=WhiteBox"

```



---



\## 🗂️ Projektstruktur (Detailed)



```

ArchivsoftwareApp/

│

├── Presentation/

│   ├── App.xaml

│   ├── App.xaml.cs

│   ├── MainWindow.xaml                 # Hauptfenster UI

│   ├── MainWindow.xaml.cs              # Hauptfenster Code-Behind

│   ├── ViewModels/

│   │   ├── MainViewModel.cs            # Haupt-ViewModel

│   │   ├── FolderViewModel.cs          # Ordner-ViewModel

│   │   └── DocumentViewModel.cs        # Dokument-ViewModel

│   └── Converters/

│       └── BoolToVisibilityConverter.cs

│

├── Business/

│   ├── Services/

│   │   ├── FolderService.cs            # Ordner-Logik

│   │   ├── DocumentService.cs          # Dokument-Logik

│   │   ├── SearchService.cs            # Such-Logik

│   │   ├── FileWatcherService.cs       # Überwachung

│   │   └── TextExtractionService.cs    # PDF/DOCX Extraktion

│   └── Models/

│       ├── SearchResult.cs             # Suchergebnis-Modell

│       └── DocumentInfo.cs             # Dokument-Info

│

└── DataAccess/

&nbsp;   ├── Entities/

&nbsp;   │   ├── Folder.cs                   # Ordner-Entity

&nbsp;   │   └── Document.cs                 # Dokument-Entity

&nbsp;   ├── Repositories/

&nbsp;   │   ├── IFolderRepository.cs        # Ordner-Interface

&nbsp;   │   ├── FolderRepository.cs         # Ordner-Repository

&nbsp;   │   ├── IDocumentRepository.cs      # Dokument-Interface

&nbsp;   │   └── DocumentRepository.cs       # Dokument-Repository

&nbsp;   ├── AppDbContext.cs                 # EF Core Context

&nbsp;   └── Migrations/                     # EF Migrations

```



---



\## 🎯 Lernziele (LB2)



Dieses Projekt erfüllt folgende Lernziele:



\### Basic (14 Lernziele)

\- ✅ \*\*A4:\*\* Komplette Maske mit Validierung und Fehlerbehandlung

\- ✅ \*\*B1:\*\* Klassen als Tabellen erstellen

\- ✅ \*\*B2:\*\* Fremdschlüssel-Beziehungen

\- ✅ \*\*B3:\*\* SQL Abfragen erstellen

\- ✅ \*\*B4:\*\* Datenmanipulationen durchführen

\- ✅ \*\*C1:\*\* Git Repository im Team

\- ✅ \*\*C2:\*\* Aussagekräftige Commits

\- ✅ \*\*C4:\*\* Sinnvolles README.md

\- ✅ \*\*D1:\*\* Vor-/Nachbedingungen (Testing)

\- ✅ \*\*D2:\*\* Ablaufplanung

\- ✅ \*\*D3:\*\* Teilprobleme beschrieben

\- ✅ \*\*D4:\*\* Schätzungen vorhanden

\- ✅ \*\*E1:\*\* NuGet-Pakete von Drittherstellern

\- ✅ \*\*E4:\*\* Clean Code Regeln eingehalten



\### Expert (3 Lernziele)

\- ✅ \*\*D6:\*\* Architektur als Klassendiagramm

\- ✅ \*\*D7:\*\* Realistisches Mockup erstellt

\- ✅ \*\*D8:\*\* Zustände der Masken geplant



\### Funktionalitäten (5 von 5)

1\. ✅ Dokument automatisch einlesen und ablegen in Datenbank

2\. ✅ Ordnerstruktur kann verwaltet werden im GUI

3\. ✅ Anzeige eines Dokuments im GUI

4\. ✅ Suche nach Begriffen ergibt korrekte Liste mit Dokumenten

5\. ✅ Programm funktioniert ohne Abstürze (Testing gemacht)



---



\## 📄 Lizenz



Dieses Projekt wurde im Rahmen des Kurses PROG1 an der \[Ihre Schule/Hochschule] erstellt und dient ausschließlich zu Bildungszwecken.



---



\## 🔗 Links \& Ressourcen



\- \[Entity Framework Core Dokumentation](https://learn.microsoft.com/ef/core/)

\- \[WPF Tutorial](https://learn.microsoft.com/dotnet/desktop/wpf/)

\- \[iTextSharp Documentation](https://github.com/itext/itextsharp)

\- \[OpenXml SDK](https://github.com/OfficeDev/Open-XML-SDK)



---



\## 🙏 Acknowledgements



Vielen Dank an unseren Dozenten Reto für die Unterstützung und das Feedback während des Projekts.

