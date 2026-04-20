# ISOBMFF / QTFF Ufex Extension

This extension provides support for the ISO Base Media File Format (ISOBMFF) and QuickTime File Format (QTFF) in Ufex.

## Architecture

The extension uses a shared base class (`BaseIsobmffFileType`) that contains common parsing and display logic for both formats. Format-specific validation is handled by subclasses:

- **`QtffFileType`** — For QuickTime `.mov` / `.qt` files. QTFF files may lack an `ftyp` box and can contain Apple-specific atoms (`wide`, `clip`, `matt`, `gmhd`, etc.).
- **`IsoBmffFileType`** — For ISOBMFF derivatives (`.mp4`, `.m4a`, `.m4v`, `.3gp`, etc.). Validates `ftyp` presence, box ordering, and flags QTFF-specific atoms.
- **`HeifFileType`** — For HEIF/HEIC/AVIF image files (`.heic`, `.heif`, `.avif`). Uses item-based infrastructure (`meta`, `iloc`, `iinf`, `iprp`) rather than tracks. Extracts EXIF metadata from Exif items.

## Project Structure

```
ISOBMFF/
├── Data/
│   ├── Box.cs              — Base box class with factory and recursive parsing
│   ├── BoxTypes.cs          — FourCC descriptions, handler types, brand lookup tables
│   ├── FtypBox.cs           — File Type box (brand/compatibility)
│   ├── MvhdBox.cs           — Movie Header box (timing, rate, matrix)
│   ├── TkhdBox.cs           — Track Header box (per-track metadata)
│   ├── MdhdBox.cs           — Media Header box (media timing, language)
│   ├── HdlrBox.cs           — Handler Reference box (media/data handler)
│   ├── Heif/                — HEIF-specific box classes
│   │   ├── PitmBox.cs       — Primary Item box
│   │   ├── IlocBox.cs       — Item Location box
│   │   ├── InfeBox.cs       — Item Information Entry box
│   │   ├── IspeBox.cs       — Image Spatial Extents property
│   │   ├── PixiBox.cs       — Pixel Information property
│   │   ├── ColrBox.cs       — Colour Information property (ICC/nclx)
│   │   ├── IrotBox.cs       — Image Rotation property
│   │   ├── ImirBox.cs       — Image Mirror property
│   │   ├── ClapBox.cs       — Clean Aperture property
│   │   ├── AuxcBox.cs       — Auxiliary Type property
│   │   ├── IpmaBox.cs       — Item Property Association box
│   │   ├── PaspBox.cs       — Pixel Aspect Ratio property
│   │   ├── ClliBox.cs       — Content Light Level Info (HDR)
│   │   └── MdcvBox.cs       — Mastering Display Colour Volume (HDR)
│   └── ThreeGpp/            — 3GPP-specific box classes
├── Structure/
│   ├── BoxNode.cs           — Base TreeNode with factory and tabular display
│   ├── FtypBoxNode.cs       — File Type node (brand display)
│   ├── MvhdBoxNode.cs       — Movie Header node (timestamps, duration)
│   ├── TkhdBoxNode.cs       — Track Header node (dimensions, flags)
│   ├── MdhdBoxNode.cs       — Media Header node (language decoding)
│   ├── HdlrBoxNode.cs       — Handler Reference node
│   ├── Heif/                — HEIF-specific node classes
│   └── ThreeGpp/            — 3GPP-specific node classes
├── BaseIsobmffFileType.cs   — Shared parsing, QuickInfo, FileMap, Structure
├── QtffFileType.cs          — QTFF-specific validation
├── IsoBmffFileType.cs       — ISOBMFF-specific validation
├── HeifFileType.cs          — HEIF/HEIC/AVIF validation + EXIF integration
├── BoxStreamReader.cs       — Top-level box stream parser
└── README.md
```


## File Types

```mermaid
classDiagram
  direction TB

  class QTFF {
    Apple QuickTime, 1991
    atom-based structure
    4-byte FourCC type codes
    no ftyp box required
  }

  class MOV {
    .mov .qt
    Direct QTFF derivative
    Apple-specific atoms ok
    ProRes, H.264, H.265
  }

  class ISOBMFF {
    ISO 14496-12, 2001
    Standardised from QTFF
    ftyp box mandatory
    brand compatibility model
  }

  class MP4 {
    .mp4 .m4v
    MPEG-4 Part 14
    H.264, H.265, AV1
    brand: mp41 mp42 isom
  }

  class M4Audio {
    .m4a .m4b .m4r
    Audio-only MP4 variants
    AAC, ALAC inside
    brand: M4A M4B M4R
  }

  class fMP4 {
    Fragmented MP4
    moof+mdat structure
    HLS, DASH streaming
    brand: iso5 iso6 cmf2
  }

  class CMAF {
    Common Media App Format
    Restricted fMP4 profile
    Single-track segments
    brand: cmf2 cmfc
  }

  class ThreeGP {
    .3gp .3g2
    3GPP mobile standard
    Constrained MP4 profile
    brand: 3gp6 3g2a
  }

  class HEIF {
    .heic .heif .avif
    High Eff. Image Format
    Still images in ISOBMFF
    brand: heic mif1 avif
  }

  class F4V {
    .f4v
    Adobe Flash MP4
    H.264 in MP4 container
    brand: f4v f4p
  }

  QTFF <|-- MOV : direct derivative
  QTFF <|-- ISOBMFF : standardised from
  ISOBMFF <|-- MP4 : MPEG-4 Part 14
  ISOBMFF <|-- M4Audio : audio profile
  ISOBMFF <|-- ThreeGP : mobile profile
  ISOBMFF <|-- HEIF : image profile
  ISOBMFF <|-- F4V : Adobe extension
  MP4 <|-- fMP4 : fragmented extension
  fMP4 <|-- CMAF : restricted profile
```