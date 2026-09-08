namespace SlashBar.Modules.Native;

public static class ExecutionStateNative {

    public const uint Continuous = 0x80000000;
    public const uint SystemRequired = 0x00000001;
    public const uint DisplayRequired = 0x00000002;


    public static uint Set(uint flags) =>
        NativeMethods.SetThreadExecutionState(flags);
}
