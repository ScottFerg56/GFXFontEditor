# GFX Font Editor
An editor for bitmap fonts for use with the Adafruit GFX Libraries.

[This program runs on Microsoft Windows and the .NET 7 runtime,
which will be required to be installed on first run, if not already present on the system.]

## Supported font file formats

### GFXfont C header (.h) file (Load & Save)
These header files are used by the Adafruit GFX Libraries using the Arduino framework.
They define the font in C program code as a list of character glyph bitmap definitions and are compiled into the program.
Unlike some other font formats, the glyphs are required to be a sequential list free of gaps and duplicates in the character codes they represent.
So, after loading other formats, the list may need to be filled with blank glyphs, or the list compressed and codes reassigned.
The __*Flatten Glyph List*__ command in the __*Glyph List*__ context menu does just that.
The possible range of C language syntax in a header file representing the construction of a valid
GFXfont is far too complex to parse here, so some errors may be detected in 'non-standard' files.
The parsing done is sufficient to handle the representative samples that Adafruit has published at the time.
The set of fonts provided with the Adafruit GFX libraries can also be seen on [GitHub](https://github.com/adafruit/Adafruit-GFX-Library/tree/master/Fonts).

### Glyph Bitmap Distribution Format (BDF) files (Load & Save)
The __BDF format__ was defined decades ago, but is still in use, especially by the Adafruit CircuitPython GFX Libraries.
There are also many examples available on the internet.
Check out Rob Hagemans' hoard-of-bitfonts on [GitHub](http://robhagemans.github.io/monobit/) where you can browse and download
BDF versions many useful fonts!
These can be also saved as font header files for use with Arduino.
When loading fonts in other formats, you may need to __*Edit Font Properties*__ before saving as BDF as some font properties not
available in other formats are required for use in CircuitPython.
__*GFX Font Editor*__ saves only a few properties useful with CircuitPython and NOT the full set that may have been loaded
which may be useful or required by other processes.

### GFX Font Editor XML format (.gfxfntx) (Load & Save)
This format is exlusively for the __*GFX Font Editor*__ to preserve the full state of an editing session
as an 'agnostic' format when working with multiple other formats.

### GFX Font Editor Binary format (.gfxfntb) (Load & Save)
This format is essentially a memory image of what would be compiled into a program when using the header file format.
These files can be dynamically loaded from any suitable file resource into memory on demand, and then freed when no longer needed.
A __*BinaryFontDemo*__ program can be found on [GitHub](https://github.com/ScottFerg56/BinaryFontDemo/tree/main).
The program uses an [Adafruit ESP32 Feather V2](https://www.adafruit.com/product/5400)
and an [Adafruit ILI9341 TFT FeatherWing](http://www.adafruit.com/products/3315),
reading the binary font file from ESP32 SPIFFS (Serial Peripheral Interface Flash File System).
But the basic concept applies to Arduino devices in general and other file systems.

## GFX Font Editor User Interface

![User Interface](Images/GFXFontEditor.png)

### Tool Bar
The File Menu contains the usual file operations.
You can also drag & drop supported font files onto the UI for loading.
Dropping multiple files can be useful, especially for browsing many fonts using the __*Font View*__.
A __*Next File*__ button will appear on the right end of the __*Tool Bar*__ for advancing through the files.
Holding the CTRL key while dropping or while click __*Next File*__ will automatically sequence through the files
at two-second intervals. Pressing __*Next File*__ again (without CTRL) will pause the sequence.

The File Menu also contains the __*Edit Font Properties*__ command to prepare fonts for saving as BDF.
Blank properties will be supplied default values. Ascent and Descent are of particular value to the CircuitPython library.
The toolbar also displays the font's line height (yAdvance) property, the number of glyphs, the pixels per dot used to scale the
__*Font View*__, and the __*Sample Text*__ to be rendered in the __*Font View*__.

### Glyph Edit Pane
The blue __*Glyph Box*__ shows the height of the font, as displayed in the __*Tool Bar*__, and the width of the selected glyph.
Drag the bottom (baseline) of the __*Glyph Box*__ to change the font line height (effecting ALL glyphs in the font).
When dragging with SHIFT, ALL the glyphs are offset at the same time to maintain their relationships with the baseline.
Drag the right side of the box to adjust the selected glyph's amount of advance (the xAdvance) shown in the __*Glyph List*__.
When dragging with SHIFT, the same advance is set for ALL glyphs as a shorcut for creating or modifying a FIXED spacing font.
The white *Bitmap Box* shows the extent of pixels set for the glyph.
Use the left mouse button to paint and erase pixels.
Use the right mouse to click and drag all the pixels around as a whole.
Dark gray pixel blocks show the fuller extent of all glyphs outside the __*Glyph Box*__.

### Glyph List
Select a glyph to edit from the __*Glyph List*__.
The glyph's Code value can be changed with the control just above the list, using the updown arrows or by entering a new value.
The glyph may jump around in the list which is kept in sorted order.
The __*Hex*__ check box allows for entry in either hexadecimal or decimal.
Other values shown for each glyph reflect the state of the glyph as edited in the __*Glyph Edit Pane*__
and are written, as required by the format, to the saved output files.
Multiple glyphs may be selected for operations in the (right click) Context Menu.
There you will find commands for list management and cut and paste, as well as operations
to rotate and flip the image of the glyph.
Glyphs can be moved around or copied in the list using the copy, cut and paste operations.
The Context Menu also contains the __*Flatten Glyph List*__ command
which will ensure a sequential list of glyphs without Code gaps or duplicate Codes.
This flattening is required for fonts to be saved in the header (.h) file format.
After flattening, you may see some glyph entries displayed in dim colors or red
to note inserted blanks or glyphs with code values changed.
To keep the in-memory size of a font to a reasonable value, large gaps are not allowed
and will result in the font Codes being compressed at the end and highlighted in red.
Duplicate codes are distributed to gaps or moved to the end.
It may be of interest to manually reorder or note the new codes for these highlighted glyphs.

### Font View
The currently selected glyph is highlighted in the view with a red bounding box
and a glyph can be selected with a click in the view.
Use the __*Pixels Per Dot*__ control in the __*Tool Bar*__ to scale the view.
The __*Sample Text*__ control in the __*Tool Bar*__ establishes the set of glyphs to view.
When left blank, all glyphs will be displayed.
Characters can be entered with the keyboard, or specific code values can be entered
by number as an escape code in the style of \u0000 with FOUR hex digits. Two slashes '\\\\' are required
to represent a single slash '\\'.
Characters in the __*Sample Text*__ not present in the font __*Glyph List*__ have placeholders in the view of a vertical yellow bar.
