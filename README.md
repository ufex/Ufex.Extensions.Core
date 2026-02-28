# Ufex.Extensions.Core

A collection of C# plugin projects for [Ufex](https://github.com/ufex) (Universal File Explorer). Each project implements a `FileType` plugin that provides binary-level parsing, structure visualization, and metadata extraction for a specific file format or family of related formats.

## Plugins

| Project | Format(s) | Description |
|---------|-----------|-------------|
| **BMP** | BMP, ICO, CUR | Windows Bitmap and Icon/Cursor files. Parses file headers, info headers (V3/V4/V5), color tables, and pixel data. |
| **GIF** | GIF87a, GIF89a | GIF images and animations. Extracts frame data, global/local color tables, extension blocks (application, comment, NETSCAPE looping). |
| **GZIP** | GZIP | GZIP compressed archives. Parses member headers and compressed data blocks. |
| **INI** | INI | INI configuration files. Parses sections, properties, comments, and detects encoding/BOM. |
| **JPEG** | JPEG, JFIF | JPEG/JFIF images. Parses marker segments (SOI, SOF, SOS, APP0, COM, etc.), JFIF metadata, chroma subsampling, and quantization tables. |
| **PDF** | PDF | Portable Document Format. Parses headers, indirect objects, cross-reference tables/streams, trailers, and document metadata (title, author, etc.). |
| **PNG** | PNG | PNG images. Parses chunks (IHDR, IDAT, IEND, tEXt, pHYs, gAMA, tIME, etc.), extracts pixel format, gamma, and embedded text metadata. |
| **RIFF** | AVI, WAV | RIFF container format. Parses the chunk hierarchy including LIST and RIFF form types. Handles AVI video and WAV audio files. |
| **ZIP** | ZIP | ZIP archives. Parses local file headers, central directory records, end of central directory, and reports compression methods. |

## Project Structure

```
ext/Ufex.Extensions.Core/
├── Ufex.Extensions.Core.sln        # Solution file
├── config/
│   └── 001_file_types_core.xml     # File type registration config
└── src/
    ├── Directory.Build.props        # Shared build properties (target framework, output paths)
    └── <FORMAT>/                    # Plugin project directory
```

Each plugin project follows a consistent layout:

```
<FORMAT>/
├── <Format>FileType.cs              # FileType implementation (entry point)
├── <Format>StreamReader.cs           # Binary stream parser
├── <Format>.csproj                   # Project file
├── Data/                             # Data model classes (parsed structures)
├── Structure/                        # Tree node classes (UI structure view)
└── README.md                         # Format-specific references and notes
```

## Architecture

Every plugin extends the `Ufex.API.FileType` base class and implements `ProcessFile()`, which drives three output pipelines:

- **Quick Info** — Key-value summary table (dimensions, version, metadata, etc.)
- **Visuals** — File map showing the byte-level layout as colored spans, plus format-specific visuals (e.g., raster image preview)
- **Structure** — Hierarchical tree view of parsed structures and their field values

## Building

All plugin projects target **.NET 10** and share common build configuration via `src/Directory.Build.props`. Built DLLs are automatically copied to the `build/<Configuration>/<TFM>/plugins/` staging directory.

```bash
dotnet build Ufex.Extensions.Core.sln
```

## Configuration

File type registrations are defined in `config/001_file_types_core.xml`. This XML maps each `FileType` class to the file format IDs it handles, allowing the Ufex host to discover and load plugins at runtime.
