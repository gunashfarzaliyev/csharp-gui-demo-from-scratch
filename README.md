# Building a macOS GUI From Scratch in C#

## Table of Contents
1. [Introduction](#introduction)
2. [Prerequisites](#prerequisites)
3. [Understanding the Architecture](#understanding-the-architecture)
4. [Step-by-Step Code Explanation](#step-by-step-code-explanation)
5. [Running Your Application](#running-your-application)
6. [Troubleshooting](#troubleshooting)
7. [Going Deeper](#going-deeper)

---

## Introduction

This guide walks you through creating a native macOS window **completely from scratch** using C#. We'll bypass high-level frameworks like MAUI or Avalonia and interact directly with macOS's native AppKit framework using the Objective-C runtime.

By the end of this guide, you'll understand:
- How GUI applications communicate with the operating system
- What happens "under the hood" when you create a window
- How C# can call Objective-C code using P/Invoke
- The fundamental building blocks of GUI programming

---

## Prerequisites

### What You Need
- **macOS Ventura 13.7.6** (or any recent macOS version)
- **.NET SDK installed** (you should already have this from earlier steps)
- **Basic understanding of C#** (variables, functions, classes)
- **Terminal application** (built into macOS)
- **Text editor** (Visual Studio Code recommended, but any will work)

### Verify Your Setup
Open Terminal and run:
```bash
dotnet --version
```
You should see a version number like `8.0.x` or similar.

---

## Understanding the Architecture

### The Big Picture

When you create a GUI application on macOS, here's what happens:

```
Your C# Code
     ↓
P/Invoke (DllImport)
     ↓
Objective-C Runtime (libobjc.dylib)
     ↓
AppKit Framework (Cocoa)
     ↓
macOS Window Manager
     ↓
Your Screen
```

### Key Concepts

#### 1. **Objective-C Runtime**
macOS is built on Objective-C, a language that extends C with object-oriented features. The Objective-C runtime is a library that handles:
- Creating and destroying objects
- Calling methods on objects (called "sending messages")
- Managing memory

Think of it as the translator between C# and macOS's native language.

#### 2. **AppKit Framework**
AppKit is Apple's framework for building macOS applications. It provides:
- `NSApplication`: The main application object
- `NSWindow`: Window objects
- `NSTextField`, `NSButton`, etc.: UI controls
- Event handling and drawing

#### 3. **P/Invoke (Platform Invoke)**
This is C#'s mechanism for calling native (non-.NET) code. The `[DllImport]` attribute tells C# where to find functions and how to call them.

#### 4. **Selectors**
In Objective-C, you don't call methods directly by name. Instead, you use "selectors" which are runtime representations of method names. For example:
- Method name: `setTitle:`
- Selector: A runtime token representing that method

---

## Step-by-Step Code Explanation

Let's break down the entire program section by section.

### Part 1: Importing Native Functions

```csharp
using System;
using System.Runtime.InteropServices;
```

**What this does:**
- `System`: Provides basic types like `IntPtr`, `String`, etc.
- `System.Runtime.InteropServices`: Provides P/Invoke functionality (`DllImport`, `Marshal`)

---

```csharp
[DllImport("/usr/lib/libobjc.dylib")]
static extern IntPtr objc_getClass(string name);
```

**What this does:**
- **`[DllImport]`**: Tells C# "this function exists in native code"
- **`"/usr/lib/libobjc.dylib"`**: Location of the Objective-C runtime library
- **`objc_getClass`**: Function that returns a class object by name
- **`string name`**: The name of the class you want (e.g., "NSWindow")
- **Returns `IntPtr`**: A pointer to the class object

**Why we need it:**
In Objective-C, everything is an object, including classes themselves. Before you can create a window, you need to get the `NSWindow` class object.

---

```csharp
[DllImport("/usr/lib/libobjc.dylib")]
static extern IntPtr sel_registerName(string name);
```

**What this does:**
- Converts a method name (like "setTitle:") into a selector
- The selector is what you use to actually call the method

**Example:**
```csharp
IntPtr setTitleSelector = sel_registerName("setTitle:");
// Now you can use this selector to call the setTitle: method on any object that supports it
```

---

```csharp
[DllImport("/usr/lib/libobjc.dylib")]
static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);
```

**What this does:**
This is the heart of Objective-C. It sends a message (calls a method) on an object.

**Parameters:**
- `receiver`: The object you're calling the method on
- `selector`: Which method you want to call

**Returns:**
Whatever the method returns (we get it as an `IntPtr`)

**Multiple Versions:**
We define several versions with different parameters because methods take different arguments:
```csharp
objc_msgSend(IntPtr receiver, IntPtr selector)  // No arguments
objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1)  // One argument
objc_msgSend(IntPtr receiver, IntPtr selector, double arg1, double arg2, ...)  // Multiple arguments
```

---

### Part 2: Defining Data Structures

```csharp
struct CGRect
{
    public double x, y, width, height;
    public CGRect(double x, double y, double width, double height)
    {
        this.x = x; this.y = y; this.width = width; this.height = height;
    }
}
```

**What this does:**
Defines a rectangle structure used by AppKit to specify positions and sizes.

**Fields:**
- `x, y`: Position (top-left corner on macOS, with origin at bottom-left of screen)
- `width, height`: Size of the rectangle

**Usage example:**
```csharp
CGRect windowFrame = new CGRect(100, 100, 800, 600);
// Creates a window at position (100, 100) with size 800x600 pixels
```

---

### Part 3: The Main Method - Creating the Application

```csharp
static void Main()
{
    Console.WriteLine("Creating a native macOS window from scratch...");
```

Simple debug output so we know the program is running.

---

```csharp
    // Step 1: Get the NSApplication class
    IntPtr nsApplicationClass = objc_getClass("NSApplication");
    IntPtr sharedApplicationSel = sel_registerName("sharedApplication");
    IntPtr app = objc_msgSend(nsApplicationClass, sharedApplicationSel);
```

**What's happening:**

1. **Get the NSApplication class**
   ```csharp
   IntPtr nsApplicationClass = objc_getClass("NSApplication");
   ```
   This is like saying: "Hey macOS, I need to work with the NSApplication class."

2. **Get the selector for sharedApplication**
   ```csharp
   IntPtr sharedApplicationSel = sel_registerName("sharedApplication");
   ```
   `sharedApplication` is a class method that returns the singleton application instance.
   In Objective-C, this would be: `[NSApplication sharedApplication]`

3. **Call sharedApplication to get the app instance**
   ```csharp
   IntPtr app = objc_msgSend(nsApplicationClass, sharedApplicationSel);
   ```
   This actually calls the method and gives us our application object.

**Why NSApplication?**
Every macOS GUI app needs an `NSApplication` object. It:
- Manages the event loop (handles clicks, keyboard input)
- Coordinates with the system
- Manages windows and menus

---

```csharp
    // Step 2: Set activation policy to regular app
    IntPtr setActivationPolicySel = sel_registerName("setActivationPolicy:");
    objc_msgSend(app, setActivationPolicySel, IntPtr.Zero);
```

**What this does:**
Sets how the application interacts with the system.

**Activation policies:**
- `0` (NSApplicationActivationPolicyRegular): Normal app with dock icon and menu bar
- `1` (NSApplicationActivationPolicyAccessory): Background app, no dock icon
- `2` (NSApplicationActivationPolicyProhibited): Never activates

We use `0` because we want a normal windowed application.

---

```csharp
    // Step 3: Activate the application
    IntPtr activateSel = sel_registerName("activateIgnoringOtherApps:");
    objc_msgSend(app, activateSel, new IntPtr(1));
```

**What this does:**
Brings our application to the front, even if other apps are active.

**Parameter `new IntPtr(1)`:**
- `1` (YES in Objective-C): Activate even if other apps are active
- `0` (NO): Only activate if no other app is active

This ensures our window appears on top when the program runs.

---

### Part 4: Creating the Window

```csharp
    // Step 3: Create a window
    IntPtr nsWindowClass = objc_getClass("NSWindow");
    IntPtr allocSel = sel_registerName("alloc");
    IntPtr window = objc_msgSend(nsWindowClass, allocSel);
```

**What's happening:**

1. **Get the NSWindow class**
   ```csharp
   IntPtr nsWindowClass = objc_getClass("NSWindow");
   ```
   
2. **Allocate memory for a window object**
   ```csharp
   IntPtr allocSel = sel_registerName("alloc");
   IntPtr window = objc_msgSend(nsWindowClass, allocSel);
   ```
   This is like saying "reserve memory for a new window, but don't set it up yet."
   
   In Objective-C: `NSWindow *window = [NSWindow alloc];`

---

```csharp
    // Step 4: Initialize the window with a frame
    CGRect frame = new CGRect(100, 100, 800, 600);
    IntPtr initSel = sel_registerName("initWithContentRect:styleMask:backing:defer:");
    
    // Window style: titled + closable + miniaturizable + resizable
    ulong styleMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3);
    window = objc_msgSend(window, initSel, 
        Marshal.AllocHGlobal(Marshal.SizeOf(frame)), 
        styleMask, 
        new IntPtr(2), // NSBackingStoreBuffered
        false);
```

**What this does:**
Actually initializes the window with specific properties.

**Breaking down the parameters:**

1. **frame**: Position and size
   ```csharp
   CGRect frame = new CGRect(100, 100, 800, 600);
   ```
   Window at (100, 100), size 800x600

2. **styleMask**: Window features
   ```csharp
   ulong styleMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3);
   ```
   This is a bit mask. Each bit represents a feature:
   - Bit 0 (1 << 0 = 1): Titled window (has a title bar)
   - Bit 1 (1 << 1 = 2): Closable (has close button)
   - Bit 2 (1 << 2 = 4): Miniaturizable (has minimize button)
   - Bit 3 (1 << 3 = 8): Resizable (user can resize)
   
   Combined: 1 | 2 | 4 | 8 = 15 (all features enabled)

3. **backing**: How the window stores its content
   ```csharp
   new IntPtr(2)  // NSBackingStoreBuffered = 2
   ```
   Buffered means content is stored in an off-screen buffer for smooth rendering.

4. **defer**: Whether to defer window creation
   ```csharp
   false
   ```
   Create the window immediately.

---

### Part 5: Setting Window Properties

```csharp
    // Step 5: Set window title
    IntPtr setTitleSel = sel_registerName("setTitle:");
    IntPtr title = CreateNSString("My First GUI From Scratch!");
    objc_msgSend(window, setTitleSel, title);
```

**What this does:**
Sets the text that appears in the window's title bar.

**Steps:**
1. Get the `setTitle:` selector
2. Convert our C# string to an NSString (Objective-C string)
3. Call `setTitle:` on the window with our NSString

In Objective-C: `[window setTitle:@"My First GUI From Scratch!"];`

---

### Part 6: Creating UI Elements (TextField)

```csharp
    // Step 6: Create a text field to display in the window
    IntPtr nsTextFieldClass = objc_getClass("NSTextField");
    IntPtr textField = objc_msgSend(nsTextFieldClass, allocSel);
    
    CGRect textFrame = new CGRect(50, 250, 700, 100);
    IntPtr initWithFrameSel = sel_registerName("initWithFrame:");
    textField = objc_msgSend(textField, initWithFrameSel, 
        Marshal.AllocHGlobal(Marshal.SizeOf(textFrame)));
```

**What this does:**
Creates a text field (label) to display text inside the window.

**Process:**
1. Get the NSTextField class
2. Allocate memory for a text field
3. Initialize it with a frame (position: 50, 250; size: 700x100)

---

```csharp
    // Set text field properties
    IntPtr setStringValueSel = sel_registerName("setStringValue:");
    IntPtr message = CreateNSString("Hello from a window created completely from scratch using C# and Objective-C runtime!");
    objc_msgSend(textField, setStringValueSel, message);
```

**What this does:**
Sets the actual text that will be displayed.

---

```csharp
    IntPtr setBorderlessSel = sel_registerName("setBordered:");
    objc_msgSend(textField, setBorderlessSel, IntPtr.Zero);
    
    IntPtr setEditableSel = sel_registerName("setEditable:");
    objc_msgSend(textField, setEditableSel, IntPtr.Zero);
    
    IntPtr setBackgroundColorSel = sel_registerName("setBackgroundColor:");
    IntPtr clearColor = GetClearColor();
    objc_msgSend(textField, setBackgroundColorSel, clearColor);
```

**What these do:**
Configure the text field's appearance:
- `setBordered:`: Remove the border (IntPtr.Zero = NO/false)
- `setEditable:`: Make it non-editable (just for display)
- `setBackgroundColor:`: Make background transparent

This makes it look like a label rather than an input field.

---

### Part 7: Adding the TextField to the Window

```csharp
    // Step 7: Add text field to window
    IntPtr contentViewSel = sel_registerName("contentView");
    IntPtr contentView = objc_msgSend(window, contentViewSel);
    
    IntPtr addSubviewSel = sel_registerName("addSubview:");
    objc_msgSend(contentView, addSubviewSel, textField);
```

**What this does:**
Adds our text field to the window so it's visible.

**Understanding the hierarchy:**
```
NSWindow
  └─ Content View (NSView)
       └─ Our TextField (NSTextField)
```

Every window has a "content view" - this is the container for all UI elements.

**Steps:**
1. Get the window's content view: `[window contentView]`
2. Add our text field to it: `[contentView addSubview:textField]`

---

### Part 8: Making the Window Visible

```csharp
    // Step 8: Make window visible
    IntPtr makeKeyAndOrderFrontSel = sel_registerName("makeKeyAndOrderFront:");
    objc_msgSend(window, makeKeyAndOrderFrontSel, IntPtr.Zero);
```

**What this does:**
Actually displays the window on screen.

**"makeKeyAndOrderFront":**
- **Key**: Makes this window receive keyboard input
- **Order Front**: Brings it to the front of all other windows

---

### Part 9: Running the Application

```csharp
    // Step 9: Run the application event loop
    IntPtr runSel = sel_registerName("run");
    objc_msgSend(app, runSel);
}
```

**What this does:**
Starts the application's event loop.

**The Event Loop:**
This is an infinite loop that:
1. Waits for events (mouse clicks, keyboard input, window resize, etc.)
2. Processes those events
3. Updates the UI
4. Repeats

Without this, your program would create the window and immediately exit.

The loop runs until the user quits the application (closes the window or presses Cmd+Q).

---

### Part 10: Helper Functions

```csharp
static IntPtr CreateNSString(string str)
{
    IntPtr nsStringClass = objc_getClass("NSString");
    IntPtr stringWithUTF8Sel = sel_registerName("stringWithUTF8String:");
    IntPtr utf8Ptr = Marshal.StringToHGlobalAnsi(str);
    return objc_msgSend(nsStringClass, stringWithUTF8Sel, utf8Ptr);
}
```

**What this does:**
Converts a C# string to an Objective-C NSString.

**Steps:**
1. Get the NSString class
2. Get the `stringWithUTF8String:` class method
3. Convert C# string to unmanaged UTF-8 bytes
4. Call the method to create an NSString

**Why we need this:**
Objective-C doesn't understand C# strings directly. We need to convert them.

---

```csharp
static IntPtr GetClearColor()
{
    IntPtr nsColorClass = objc_getClass("NSColor");
    IntPtr clearColorSel = sel_registerName("clearColor");
    return objc_msgSend(nsColorClass, clearColorSel);
}
```

**What this does:**
Gets a transparent color object.

In Objective-C: `[NSColor clearColor]`

This is used to make the text field's background transparent.

---

## Running Your Application

### Method 1: Using a .NET Project (Recommended)

#### Step 1: Create a Project
```bash
# Navigate to where you want to create the project
cd ~/Desktop

# Create new console project
dotnet new console -n MacOSGuiFromScratch

# Navigate into the project
cd MacOSGuiFromScratch
```

#### Step 2: Add Your Code
Open `Program.cs` in your text editor and replace all its content with the GUI code.

#### Step 3: Run
```bash
dotnet run
```

### Method 2: Standalone File

#### Step 1: Create the File
```bash
# Create a directory
mkdir ~/Desktop/MyGui
cd ~/Desktop/MyGui

# Create Program.cs
touch Program.cs
```

#### Step 2: Add Your Code
Open `Program.cs` and paste the entire GUI code.

#### Step 3: Create a Project File
Create a file named `MyGui.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
```

#### Step 4: Run
```bash
dotnet run
```

---

## What Should Happen

When you run the program successfully:

1. **Console Output**
   ```
   Creating a native macOS window from scratch...
   Look for the window - it should appear on your screen!
   ```

2. **A Window Appears**
   - Title: "My First GUI From Scratch!"
   - Size: 800x600 pixels
   - Position: Near top-left of screen
   - Has standard macOS window controls (close, minimize, maximize)

3. **Window Content**
   - Text displayed: "Hello from a window created completely from scratch using C# and Objective-C runtime!"
   - White background
   - Standard macOS window appearance

4. **The Application Runs**
   - Window stays open
   - You can move it around
   - You can resize it
   - Close button works
   - Responds to Cmd+Q to quit(It is not working right now)

---

## Troubleshooting

### Problem: Window Doesn't Appear

**Possible causes:**
1. Window is created but behind other windows
2. Program crashed before window was shown

**Solutions:**
- Check if a dock icon appeared - click it
- Use Cmd+Tab to switch to the application
- Look in Activity Monitor for your process
- Check Terminal for error messages

---

### Problem: Program Crashes Immediately

**Common causes:**
1. Incorrect selector names (typos)
2. Wrong number of arguments to objc_msgSend
3. Memory allocation issues

**Debug steps:**
```csharp
// Add try-catch to see errors
try
{
    IntPtr nsApplicationClass = objc_getClass("NSApplication");
    if (nsApplicationClass == IntPtr.Zero)
    {
        Console.WriteLine("Failed to get NSApplication class!");
        return;
    }
    Console.WriteLine("Got NSApplication class successfully");
    // ... rest of code
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
```

---

### Problem: Window Appears But No Text

**Cause:**
Text field wasn't added properly or is positioned outside visible area.

**Check:**
- Verify the CGRect coordinates for the text field
- Ensure `addSubview:` was called
- Make sure text color isn't same as background

---

### Problem: Can't Close Window

**Cause:**
Event loop isn't processing close events.

**Solution:**
The window should close normally. If not:
- Press Cmd+Q (quits the entire application)
- Press Ctrl+C in Terminal (forcefully stops the program)

---


### Exploring AppKit

Check Apple's documentation to see what other classes and methods are available:
- NSButton
- NSTextField
- NSImageView
- NSTableView
- NSMenu
- And hundreds more!

Each follows the same pattern:
1. Get the class
2. Allocate an instance
3. Initialize it
4. Set properties
5. Add to window

### Understanding Memory Management

Objective-C uses reference counting. In production code, you should:
- Call `retain` on objects you want to keep
- Call `release` when you're done with them
- Use `autorelease` for temporary objects

We skipped this for simplicity, but it's important for real applications.

---

## Key Takeaways

### What You Learned

1. **P/Invoke**: How to call native code from C#
2. **Objective-C Runtime**: How macOS applications work at a low level
3. **AppKit**: The framework powering all macOS applications
4. **Event Loops**: How GUI applications stay responsive
5. **Memory Management**: Converting between managed and unmanaged memory

### Why This Matters

Understanding how GUI frameworks work "under the hood" helps you:
- Debug issues more effectively
- Make better architectural decisions
- Appreciate the abstractions provided by high-level frameworks
- Understand performance implications
- Port code between platforms

### Next Steps

Now that you understand the basics, you can:
1. Explore other AppKit classes
2. Add more complex UI elements
3. Handle user input (buttons, text fields)
4. Draw custom graphics
5. Create menus and toolbars
6. Learn about delegates and protocols

Or, with this knowledge, appreciate what high-level frameworks like MAUI and Avalonia do for you automatically!

---

## Complete Code Reference

Here's the entire program for easy reference:

```csharp
using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("/usr/lib/libobjc.dylib")]
    static extern IntPtr objc_getClass(string name);
    
    [DllImport("/usr/lib/libobjc.dylib")]
    static extern IntPtr sel_registerName(string name);
    
    [DllImport("/usr/lib/libobjc.dylib")]
    static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);
    
    [DllImport("/usr/lib/libobjc.dylib")]
    static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);
    
    [DllImport("/usr/lib/libobjc.dylib")]
    static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, double arg1, double arg2, double arg3, double arg4);
    
    [DllImport("/usr/lib/libobjc.dylib")]
    static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, ulong arg2, IntPtr arg3, bool arg4);

    struct CGRect
    {
        public double x, y, width, height;
        public CGRect(double x, double y, double width, double height)
        {
            this.x = x; this.y = y; this.width = width; this.height = height;
        }
    }

    static void Main()
    {
        Console.WriteLine("Creating a native macOS window from scratch...");
        Console.WriteLine("Look for the window - it should appear on your screen!");
        
        IntPtr nsApplicationClass = objc_getClass("NSApplication");
        IntPtr sharedApplicationSel = sel_registerName("sharedApplication");
        IntPtr app = objc_msgSend(nsApplicationClass, sharedApplicationSel);
        
        IntPtr setActivationPolicySel = sel_registerName("setActivationPolicy:");
        objc_msgSend(app, setActivationPolicySel, IntPtr.Zero);
        
        IntPtr activateSel = sel_registerName("activateIgnoringOtherApps:");
        objc_msgSend(app, activateSel, new IntPtr(1));
        
        IntPtr nsWindowClass = objc_getClass("NSWindow");
        IntPtr allocSel = sel_registerName("alloc");
        IntPtr window = objc_msgSend(nsWindowClass, allocSel);
        
        CGRect frame = new CGRect(100, 100, 800, 600);
        IntPtr initSel = sel_registerName("initWithContentRect:styleMask:backing:defer:");
        
        ulong styleMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3);
        window = objc_msgSend(window, initSel, 
            Marshal.AllocHGlobal(Marshal.SizeOf(frame)), 
            styleMask, 
            new IntPtr(2),
            false);
        
        IntPtr setTitleSel = sel_registerName("setTitle:");
        IntPtr title = CreateNSString("My First GUI From Scratch!");
        objc_msgSend(window, setTitleSel, title);
        
        IntPtr nsTextFieldClass = objc_getClass("NSTextField");
        IntPtr textField = objc_msgSend(nsTextFieldClass, allocSel);
        
        CGRect textFrame = new CGRect(50, 250, 700, 100);
        IntPtr initWithFrameSel = sel_registerName("initWithFrame:");
        textField = objc_msgSend(textField, initWithFrameSel, 
            Marshal.AllocHGlobal(Marshal.SizeOf(textFrame)));
        
        IntPtr setStringValueSel = sel_registerName("setStringValue:");
        IntPtr message = CreateNSString("Hello from a window created completely from scratch using C# and Objective-C runtime!");
        objc_msgSend(textField, setStringValueSel, message);
        
        IntPtr setBorderlessSel = sel_registerName("setBordered:");
        objc_msgSend(textField, setBorderlessSel, IntPtr.Zero);
        
        IntPtr setEditableSel = sel_registerName("setEditable:");
        objc_msgSend(textField, setEditableSel, IntPtr.Zero);
        
        IntPtr setBackgroundColorSel = sel_registerName("setBackgroundColor:");
        IntPtr clearColor = GetClearColor();
        objc_msgSend(textField, setBackgroundColorSel, clearColor);
        
        IntPtr contentViewSel = sel_registerName("contentView");
        IntPtr contentView = objc_msgSend(window, contentViewSel);
        
        IntPtr addSubviewSel = sel_registerName("addSubview:");
        objc_msgSend(contentView, addSubviewSel, textField);
        
        IntPtr makeKeyAndOrderFrontSel = sel_registerName("makeKeyAndOrderFront:");
        objc_msgSend(window, makeKeyAndOrderFrontSel, IntPtr.Zero);
        
        IntPtr runSel = sel_registerName("run");
        objc_msgSend(app, runSel);
    }
    
    static IntPtr CreateNSString(string str)
    {
        IntPtr nsStringClass = objc_getClass("NSString");
        IntPtr stringWithUTF8Sel = sel_registerName("stringWithUTF8String:");
        IntPtr utf8Ptr = Marshal.StringToHGlobalAnsi(str);
        return objc_msgSend(nsStringClass, stringWithUTF8Sel, utf8Ptr);
    }
    
    static IntPtr GetClearColor()
    {
        IntPtr nsColorClass = objc_getClass("NSColor");
        IntPtr clearColorSel = sel_registerName("clearColor");
        return objc_msgSend(nsColorClass, clearColorSel);
    }
}
```

---

## Conclusion

Congratulations! You've just created a native macOS GUI application from scratch, bypassing all high-level frameworks and working directly with the operating system.

This knowledge gives you a deep understanding of how GUI applications really work, which will make you a better programmer regardless of which framework you ultimately choose to use.

Remember: most developers use high-level frameworks (MAUI, Avalonia, etc.) for production work because they handle all this complexity for you. But understanding what happens "under the hood" is invaluable.

Happy coding!