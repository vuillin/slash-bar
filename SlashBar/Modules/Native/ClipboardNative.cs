namespace SlashBar.Modules.Native;

public static class ClipboardNative {

    public const int WmClipboardUpdate = 0x031D;
    public static readonly IntPtr HwndMessage = new(-3);


    public static bool AddFormatListener(IntPtr hwnd) =>
        NativeMethods.AddClipboardFormatListener(hwnd);

    public static bool RemoveFormatListener(IntPtr hwnd) =>
        NativeMethods.RemoveClipboardFormatListener(hwnd);
}
