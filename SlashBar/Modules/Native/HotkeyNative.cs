namespace SlashBar.Modules.Native;

public static class HotkeyNative {

    public const uint ModAlt = 0x0001;
    public const uint ModControl = 0x0002;
    public const uint ModShift = 0x0004;
    public const uint ModNoRepeat = 0x4000;

    public const int WmHotkey = 0x0312;


    public static bool Register(IntPtr hwnd, int id, uint modifiers, uint vk) =>
        NativeMethods.RegisterHotKey(hwnd, id, modifiers, vk);

    public static bool Unregister(IntPtr hwnd, int id) =>
        NativeMethods.UnregisterHotKey(hwnd, id);
}
