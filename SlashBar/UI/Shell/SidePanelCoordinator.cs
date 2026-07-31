namespace SlashBar.UI.Shell;

/// <summary>
/// Ensures only one side panel is open at a time.
/// </summary>
public static class SidePanelCoordinator {

    private static readonly Dictionary<Type, Action> Closers = new();

    public static void Register(Type panelType, Action closeIfOpen) =>
        Closers[panelType] = closeIfOpen;

    public static void CloseOthersExcept(Type keepType) {
        foreach (var (type, close) in Closers) {
            if (type != keepType)
                close();
        }
    }
}
