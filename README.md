# WinZLR
*WinZLR* is a front-end GUI for *ConsoleZLR*, a Z-machine debugger. As the name hints, it is only for Windows (use WinForms).

Features include:
* Setting breakpoints in source code or assembly
* Stepping through code, one instruction at a time
* Changing global and local variables on-the-fly
* Inspect content on stack
* See and navigate through the call stack
* Resetting/moving program counter inside routine
* Switch between source line and its assembly
* Syntax highlighting of source code
* Read debugger files both in `xml.dbg` format and in `dbg` format
* Fix incomplete paths for source files to fully resolved paths
* Can send low-level debug commands to *ConsoleZLR* (see below) 

*WinZLR* is dependent on two other components:
* [*ConsoleZLR*](https://github.com/taradinoc/zlr), by Tara McGrew
*  [*UnZ*](https://github.com/heasm66/unz), by Henrik Åsman

[Pre-compiled files (Win-x64)](https://drive.google.com/drive/folders/1ibXBW9MttHoTtwe8xvfulxLaUGRk4H3_?usp=drive_link) of all three components.
 ## Instructions
 ### Inform6
 *Inform6 creates a `xml.dbg`-file (gametext.dbg) with the `-k` switch. The paths in the file should be fully resolved so just place the story-file and the `dbg`-file in the same directory then open the story-file in *WinZLR*.
 ### ZILF
 *ZILF* creates an `[game].dbg` with the `-d` switch. *ZILF* doesn't supply full paths, so you'll either place all files in the same directory or add search paths in the setting dialog to let *WinZLR* resolve the paths before you open the story-file in *WinZLR*.
# ConsoleZLR
ZLR is a .NET implementation of the [Z-machine](http://www.ifwiki.org/index.php/Z-machine). Originally developed as a proof of concept for using JIT to speed up complex interactive fiction games, ZLR continues to be developed for use as a debugger and integration test engine in [ZILF](https://foss.heptapod.net/zilf/zilf).
## Instructions
### Setting up a debug session with two console windows
1. Start two consoles windows (the second as administrator)

2. Console 1: `ConsoleZLR -debug -listen 55555 zork1.z3 zork1.dbg`

3. Console 2: `Telnet 127.0.0.1 55555`
### Commands and how to debug
`ConsoleZLR.exe -debug advent.z3 advent.dbg`

It'll break on the first instruction. Type `h` or `?` for the command list.

* `reset` - Restart the game
* `step` - Execute one instruction, entering function calls
* `over` - Like step, but instead of entering a function call, run until the function returns or a breakpoint is hit
* `stepline, overline` - Like step and over but for source lines instead of instructions
* `up` - Run until the current function returns or a breakpoint is hit
* `run` - Run until the game ends or a breakpoint is hit
* `break, clear` - Set or unset a breakpoint. The argument can be a routine name (with optional offset, like PARSER+3), numeric address, or a source line reference like full path to source file:1234
* `breakpoints` - List active breakpoints
* `tracecalls` - Toggle printing a message for every function call
* `backtrace` - Print the call stack
* `locals` - List local variables and stack values for the current call frame
* `globals` - List global variables
* `print` - Evaluate an expression, using ZIL-like syntax. You can make changes by calling FSET, MOVE, SETG, etc.
* `showobj` - Print the details of an object. The object is specified using the same ZIL-like syntax as print; in particular, you'll need a comma if the object is in a global variable (showobj ,WINNER).
* `tree` - Print the tree of all objects, or if an argument is given, the object tree with the given object as its root. The argument is in the same ZIL-like syntax as above.
* `quit` - Exit
* `jump` - Change the PC to a new address.

*ZLR* only works with binary debug info files (not XML).