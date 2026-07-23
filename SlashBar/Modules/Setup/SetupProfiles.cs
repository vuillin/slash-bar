namespace SlashBar.Modules.Setup;

/// <summary>
/// Profils déclarés en dur — ajouter une entrée ici = nouveau setup
/// </summary>
public static class SetupProfiles {

    public static IReadOnlyList<SetupProfile> All { get; } = [
        new("dev", "Environnement de dév. Mediverif", [
            new( // Cursor
                @"C:\Users\Thomas\AppData\Local\Programs\cursor\Cursor.exe",
                Arguments: "--classic",
                Layout: WindowLayout.Maximize,
                WindowProcessName: "Cursor"),

            new( // Mail Thunderbird
                @"C:\Program Files\Mozilla Thunderbird\thunderbird.exe",
                Layout: WindowLayout.Minimized,
                WindowProcessName: "thunderbird")
        ]),
    ];

    public static SetupProfile? Find(string name) =>
        All.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}