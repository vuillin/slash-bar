namespace SlashBar.Modules.Native;

/// <summary>WinEvent hook helpers (pin border tracking).</summary>
public static class WinEventNative {

    public const uint EventSystemMovesizeStart = 0x000A;
    public const uint EventSystemMovesizeEnd = 0x000B;
    public const uint EventSystemMinimizeStart = 0x0016;
    public const uint EventSystemMinimizeEnd = 0x0017;
    public const uint EventObjectDestroy = 0x8001;
    public const uint EventObjectLocationChange = 0x800B;

    public const int ObjidWindow = 0;
    public const uint OutOfContext = 0x0000;
    public const uint SkipOwnProcess = 0x0002;


    public static IntPtr Hook(
        uint eventId,
        NativeMethods.WinEventProc callback,
        uint processId) =>
        NativeMethods.SetWinEventHook(
            eventId,
            eventId,
            IntPtr.Zero,
            callback,
            processId,
            0,
            OutOfContext | SkipOwnProcess);

    public static bool Unhook(IntPtr hook) =>
        hook != IntPtr.Zero && NativeMethods.UnhookWinEvent(hook);
}
