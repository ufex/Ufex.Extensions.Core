# JPEG/JFIF File Type Plugin

Ufex file type plugin for parsing and displaying the internal structure of **JPEG File Interchange Format (JFIF)** files.

## Format Overview

JFIF (JPEG File Interchange Format) is a minimal file format that enables JPEG bitstreams to be exchanged between a wide variety of platforms and applications. A JFIF file consists of a sequence of marker segments:

| Marker | Name | Description |
|--------|------|-------------|
| `FFD8` | SOI | Start of Image (mandatory, first marker) |
| `FFE0` | APP0 (JFIF) | JFIF application data: version, density, thumbnail |
| `FFE0` | APP0 (JFXX) | JFIF extension: additional thumbnail formats |
| `FFE1`-`FFEF` | APP1-APP15 | Application-specific data (Exif, ICC, etc.) |
| `FFDB` | DQT | Define Quantization Table(s) |
| `FFC4` | DHT | Define Huffman Table(s) |
| `FFC0`-`FFC3` | SOF0-SOF3 | Start of Frame (image dimensions, components) |
| `FFDD` | DRI | Define Restart Interval |
| `FFDA` | SOS | Start of Scan (followed by entropy-coded data) |
| `FFFE` | COM | Comment |
| `FFD9` | EOI | End of Image (mandatory, last marker) |

## Supported Features

- Parses all standard JPEG marker segments
- Identifies JFIF and JFXX APP0 markers
- Extracts image dimensions, sample precision, and component information from SOF
- Displays quantization and Huffman table metadata
- Handles entropy-coded scan data (skips to next marker)
- Reports chroma subsampling ratio (4:4:4, 4:2:2, 4:2:0, etc.)
- Shows file structure as a visual file map

## References

- ECMA TR/98 - JPEG File Interchange Format (JFIF) Version 1.02
- ITU-T Recommendation T.81 | ISO/IEC 10918-1 (JPEG standard)
- JFIF Version 1.02 specification by Eric Hamilton, C-Cube Microsystems
