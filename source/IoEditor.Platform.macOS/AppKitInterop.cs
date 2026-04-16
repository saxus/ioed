using System.Runtime.InteropServices;

namespace IoEditor.Platform;

internal static class AppKitInterop
{
    private const int NsFileHandlingPanelOkButton = 1;
    private const int NsAlertFirstButtonReturn = 1000;

    [DllImport("/usr/lib/libobjc.dylib", CharSet = CharSet.Ansi)]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.dylib", CharSet = CharSet.Ansi)]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend0(IntPtr receiver, IntPtr op);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend1(IntPtr receiver, IntPtr op, IntPtr a1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern nint objc_msgSend_nint0(IntPtr receiver, IntPtr op);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void0(IntPtr receiver, IntPtr op);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void1(IntPtr receiver, IntPtr op, IntPtr a1);

    private static void InvokeInAutoreleasePool(Action action)
    {
        var poolClass = objc_getClass("NSAutoreleasePool");
        var pool = objc_msgSend0(
            objc_msgSend0(poolClass, sel_registerName("alloc")),
            sel_registerName("init"));
        try
        {
            action();
        }
        finally
        {
            if (pool != IntPtr.Zero)
            {
                objc_msgSend_void0(pool, sel_registerName("release"));
            }
        }
    }

    private static T InvokeInAutoreleasePool<T>(Func<T> fn)
    {
        var poolClass = objc_getClass("NSAutoreleasePool");
        var pool = objc_msgSend0(
            objc_msgSend0(poolClass, sel_registerName("alloc")),
            sel_registerName("init"));
        try
        {
            return fn();
        }
        finally
        {
            if (pool != IntPtr.Zero)
            {
                objc_msgSend_void0(pool, sel_registerName("release"));
            }
        }
    }

    internal static IntPtr NSStringFromUtf8(string s)
    {
        var utf8 = Marshal.StringToCoTaskMemUTF8(s);
        try
        {
            var nsStringClass = objc_getClass("NSString");
            return objc_msgSend1(nsStringClass, sel_registerName("stringWithUTF8String:"), utf8);
        }
        finally
        {
            Marshal.FreeCoTaskMem(utf8);
        }
    }

    private static string? NSStringToUtf8(IntPtr nsString)
    {
        if (nsString == IntPtr.Zero)
        {
            return null;
        }

        var utf8 = objc_msgSend0(nsString, sel_registerName("UTF8String"));
        return utf8 == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(utf8);
    }

    internal static void RunError(string message, string title)
    {
        InvokeInAutoreleasePool(() =>
        {
            var alertClass = objc_getClass("NSAlert");
            var alert = objc_msgSend0(
                objc_msgSend0(alertClass, sel_registerName("alloc")),
                sel_registerName("init"));
            try
            {
                objc_msgSend_void1(alert, sel_registerName("setMessageText:"), NSStringFromUtf8(title));
                objc_msgSend_void1(alert, sel_registerName("setInformativeText:"), NSStringFromUtf8(message));
                objc_msgSend_void1(alert, sel_registerName("setAlertStyle:"), (IntPtr)2); // critical
                objc_msgSend_void1(alert, sel_registerName("addButtonWithTitle:"), NSStringFromUtf8("OK"));
                _ = objc_msgSend_nint0(alert, sel_registerName("runModal"));
            }
            finally
            {
                objc_msgSend_void0(alert, sel_registerName("release"));
            }
        });
    }

    internal static void RunInformational(string message, string title)
    {
        InvokeInAutoreleasePool(() =>
        {
            var alertClass = objc_getClass("NSAlert");
            var alert = objc_msgSend0(
                objc_msgSend0(alertClass, sel_registerName("alloc")),
                sel_registerName("init"));
            try
            {
                var headline = string.IsNullOrEmpty(title) ? "IoEditor" : title;
                objc_msgSend_void1(alert, sel_registerName("setMessageText:"), NSStringFromUtf8(headline));
                objc_msgSend_void1(alert, sel_registerName("setInformativeText:"), NSStringFromUtf8(message));
                objc_msgSend_void1(alert, sel_registerName("setAlertStyle:"), (IntPtr)1);
                objc_msgSend_void1(alert, sel_registerName("addButtonWithTitle:"), NSStringFromUtf8("OK"));
                _ = objc_msgSend_nint0(alert, sel_registerName("runModal"));
            }
            finally
            {
                objc_msgSend_void0(alert, sel_registerName("release"));
            }
        });
    }

    internal static bool RunConfirm(string message, string title)
    {
        return InvokeInAutoreleasePool(() =>
        {
            var alertClass = objc_getClass("NSAlert");
            var alert = objc_msgSend0(
                objc_msgSend0(alertClass, sel_registerName("alloc")),
                sel_registerName("init"));
            try
            {
                objc_msgSend_void1(alert, sel_registerName("setMessageText:"), NSStringFromUtf8(title));
                objc_msgSend_void1(alert, sel_registerName("setInformativeText:"), NSStringFromUtf8(message));
                objc_msgSend_void1(alert, sel_registerName("setAlertStyle:"), (IntPtr)1);
                objc_msgSend_void1(alert, sel_registerName("addButtonWithTitle:"), NSStringFromUtf8("Yes"));
                objc_msgSend_void1(alert, sel_registerName("addButtonWithTitle:"), NSStringFromUtf8("No"));
                var r = (int)objc_msgSend_nint0(alert, sel_registerName("runModal"));
                return r == NsAlertFirstButtonReturn;
            }
            finally
            {
                objc_msgSend_void0(alert, sel_registerName("release"));
            }
        });
    }

    internal static string? RunOpenIoPanel(string title)
    {
        return InvokeInAutoreleasePool(() =>
        {
            var panelClass = objc_getClass("NSOpenPanel");
            var panel = objc_msgSend0(panelClass, sel_registerName("openPanel"));
            objc_msgSend_void1(panel, sel_registerName("setTitle:"), NSStringFromUtf8(title));
            objc_msgSend_void1(panel, sel_registerName("setCanChooseFiles:"), (IntPtr)1);
            objc_msgSend_void1(panel, sel_registerName("setCanChooseDirectories:"), IntPtr.Zero);
            objc_msgSend_void1(panel, sel_registerName("setAllowsMultipleSelection:"), IntPtr.Zero);
            var types = NSMutableArrayWithSingleString("io");
            objc_msgSend_void1(panel, sel_registerName("setAllowedFileTypes:"), types);

            if ((int)objc_msgSend_nint0(panel, sel_registerName("runModal")) != NsFileHandlingPanelOkButton)
            {
                return null;
            }

            var urls = objc_msgSend0(panel, sel_registerName("URLs"));
            if (urls == IntPtr.Zero)
            {
                return null;
            }

            var count = (int)objc_msgSend_nint0(urls, sel_registerName("count"));
            if (count <= 0)
            {
                return null;
            }

            var url = objc_msgSend1(urls, sel_registerName("objectAtIndex:"), IntPtr.Zero);
            return UrlToPath(url);
        });
    }

    internal static string? RunSaveIoPanel(string title)
    {
        return InvokeInAutoreleasePool(() =>
        {
            var panelClass = objc_getClass("NSSavePanel");
            var panel = objc_msgSend0(panelClass, sel_registerName("savePanel"));
            objc_msgSend_void1(panel, sel_registerName("setTitle:"), NSStringFromUtf8(title));
            objc_msgSend_void1(panel, sel_registerName("setCanCreateDirectories:"), (IntPtr)1);
            var types = NSMutableArrayWithSingleString("io");
            objc_msgSend_void1(panel, sel_registerName("setAllowedFileTypes:"), types);

            if ((int)objc_msgSend_nint0(panel, sel_registerName("runModal")) != NsFileHandlingPanelOkButton)
            {
                return null;
            }

            var url = objc_msgSend0(panel, sel_registerName("URL"));
            return UrlToPath(url);
        });
    }

    internal static string? RunOpenFolderPanel(string title)
    {
        return InvokeInAutoreleasePool(() =>
        {
            var panelClass = objc_getClass("NSOpenPanel");
            var panel = objc_msgSend0(panelClass, sel_registerName("openPanel"));
            objc_msgSend_void1(panel, sel_registerName("setTitle:"), NSStringFromUtf8(title));
            objc_msgSend_void1(panel, sel_registerName("setCanChooseFiles:"), IntPtr.Zero);
            objc_msgSend_void1(panel, sel_registerName("setCanChooseDirectories:"), (IntPtr)1);
            objc_msgSend_void1(panel, sel_registerName("setAllowsMultipleSelection:"), IntPtr.Zero);
            if ((int)objc_msgSend_nint0(panel, sel_registerName("runModal")) != NsFileHandlingPanelOkButton)
            {
                return null;
            }

            var urls = objc_msgSend0(panel, sel_registerName("URLs"));
            if (urls == IntPtr.Zero)
            {
                return null;
            }

            var count = (int)objc_msgSend_nint0(urls, sel_registerName("count"));
            if (count <= 0)
            {
                return null;
            }

            var url = objc_msgSend1(urls, sel_registerName("objectAtIndex:"), IntPtr.Zero);
            return UrlToPath(url);
        });
    }

    private static IntPtr NSMutableArrayWithSingleString(string ext)
    {
        var arr = objc_msgSend0(objc_getClass("NSMutableArray"), sel_registerName("array"));
        objc_msgSend_void1(arr, sel_registerName("addObject:"), NSStringFromUtf8(ext));
        return arr;
    }

    private static string? UrlToPath(IntPtr url)
    {
        if (url == IntPtr.Zero)
        {
            return null;
        }

        var pathNs = objc_msgSend0(url, sel_registerName("path"));
        return NSStringToUtf8(pathNs);
    }
}
