using System;
using System.Runtime.InteropServices;

class Program
{
    // Dynamic library loading
    [DllImport("/usr/lib/libSystem.dylib")]
    static extern IntPtr dlopen(string path, int mode);

    // Objective-C runtime functions
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
    static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, CGRect arg1, ulong arg2, IntPtr arg3, bool arg4);

    [DllImport("/usr/lib/libobjc.dylib")]
    static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, CGRect arg1);

    // Structure for window frame
    [StructLayout(LayoutKind.Sequential)]
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

        // Step 0: Load the AppKit framework
        IntPtr appKit = dlopen("/System/Library/Frameworks/AppKit.framework/AppKit", 0x2); // RTLD_NOW = 0x2
        if (appKit == IntPtr.Zero)
        {
            Console.WriteLine("ERROR: Failed to load AppKit framework!");
            return;
        }

        // Step 1: Get the NSApplication class
        IntPtr nsApplicationClass = objc_getClass("NSApplication");
        IntPtr sharedApplicationSel = sel_registerName("sharedApplication");
        IntPtr app = objc_msgSend(nsApplicationClass, sharedApplicationSel);

        // Step 2: Set activation policy to regular app
        IntPtr setActivationPolicySel = sel_registerName("setActivationPolicy:");
        objc_msgSend(app, setActivationPolicySel, IntPtr.Zero); // NSApplicationActivationPolicyRegular = 0

        // Step 3: Activate the application
        IntPtr activateSel = sel_registerName("activateIgnoringOtherApps:");
        objc_msgSend(app, activateSel, new IntPtr(1));

        // Step 3: Create a window
        IntPtr nsWindowClass = objc_getClass("NSWindow");
        IntPtr allocSel = sel_registerName("alloc");
        IntPtr window = objc_msgSend(nsWindowClass, allocSel);

        // Step 4: Initialize the window with a frame
        CGRect frame = new CGRect(100, 100, 1920, 1080);
        IntPtr initSel = sel_registerName("initWithContentRect:styleMask:backing:defer:");

        // Window style: titled + closable + miniaturizable + resizable
        ulong styleMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3);
        window = objc_msgSend(window, initSel,
            frame,
            styleMask,
            new IntPtr(2), // NSBackingStoreBuffered
            false);

        // Step 5: Set window title
        IntPtr setTitleSel = sel_registerName("setTitle:");
        IntPtr title = CreateNSString("My First GUI From Scratch!");
        objc_msgSend(window, setTitleSel, title);

        // Step 6: Create a text field to display in the window
        IntPtr nsTextFieldClass = objc_getClass("NSTextField");
        IntPtr textField = objc_msgSend(nsTextFieldClass, allocSel);

        CGRect textFrame = new CGRect(50, 250, 700, 100);
        IntPtr initWithFrameSel = sel_registerName("initWithFrame:");
        textField = objc_msgSend(textField, initWithFrameSel, textFrame);

        // Set text field properties
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

        // Step 7: Add text field to window
        IntPtr contentViewSel = sel_registerName("contentView");
        IntPtr contentView = objc_msgSend(window, contentViewSel);

        IntPtr addSubviewSel = sel_registerName("addSubview:");
        objc_msgSend(contentView, addSubviewSel, textField);

        // Step 8: Make window visible
        IntPtr makeKeyAndOrderFrontSel = sel_registerName("makeKeyAndOrderFront:");
        objc_msgSend(window, makeKeyAndOrderFrontSel, IntPtr.Zero);

        // Step 9: Run the application event loop
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
        IntPtr purpleColorSel = sel_registerName("purpleColor");
        return objc_msgSend(nsColorClass, purpleColorSel);
    }
}
