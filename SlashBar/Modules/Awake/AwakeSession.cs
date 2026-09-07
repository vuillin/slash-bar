using System.Runtime.InteropServices;

namespace SlashBar.Modules.Awake;

/// <summary>
/// Keeps the system (and display) awake via SetThreadExecutionState.
/// Does not change Windows power settings.
/// </summary>
public static class AwakeSession {

    private const uint EsContinuous = 0x80000000;
    private const uint EsSystemRequired = 0x00000001;
    private const uint EsDisplayRequired = 0x00000002;

    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);


    public static bool IsActive { get; private set; }


    public static void Enable() {
        if (IsActive)
            return;

        SetThreadExecutionState(EsContinuous | EsSystemRequired | EsDisplayRequired);
        IsActive = true;
    }


    public static void Disable() {
        if (!IsActive)
            return;

        SetThreadExecutionState(EsContinuous);
        IsActive = false;
    }


    public static bool Toggle() {
        if (IsActive) 
            Disable();
        else
            Enable();
        
        return IsActive;
    }
}