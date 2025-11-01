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

    struct CGRectangle
    {
        public double x, y, width, height;
        public CGRectangle(double x, double y, double width, double height)
        {
            this.x = x; this.y = y; this.width = width; this.height = height;
        }
    }
    static void Main()
    {
        IntPtr nsApplicationClass = objc_getClass("NSApplication");
        IntPtr sharedApplicationSel = sel_registerName("sharedApplication");
        IntPtr app = objc_msgSend(nsApplicationClass, sharedApplicationSel);
        IntPtr activateSel = sel_registerName("activateIgnoringOtherApps:");
        objc_msgSend(app, activateSel, new IntPtr(1));
        IntPtr nsWindowClass = objc_getClass("NSWindow");
        IntPtr allocSel = sel_registerName("alloc");
        IntPtr window = objc_msgSend(nsWindowClass, allocSel);
        CGRectangle frame = new CGRectangle(100, 100, 800, 600);
        IntPtr initSel = sel_registerName("initWithContentRect:styleMask:backing:defer:");

        ulong styleMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3);
        window = objc_msgSend(window, initSel, Marshal.AllocHGlobal(Marshal.SizeOf(frame)),
            styleMask, new IntPtr(2), false);
        IntPtr setTitleSel = sel_registerName("setTitle:");
        IntPtr title = CreateNSString("My First GUI from scratch");
        objc_msgSend(window, setTitleSel, title);
        IntPtr nsTextFieldClass = objc_getClass("NSTextField");
        IntPtr textField = objc_msgSend(nsTextFieldClass, allocSel);
        CGRectangle textFrame = new CGRectangle(50, 250, 700, 100);
        IntPtr initWithFrameSel = sel_registerName("initWithFrame:");
        textField = objc_msgSend(textField, initWithFrameSel, Marshal.AllocHGlobal(Marshal.SizeOf(textFrame)));
        IntPtr setStringValueSel = sel_registerName("setStringValue:");
        IntPtr message = CreateNSString("Hello from a window created completely from scratch using C# and Objective-C runtime!!!");
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
        IntPtr addSubviewSel = sel_registerName("addSubview");
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
