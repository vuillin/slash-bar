using System.Windows.Input;
using SlashBar.Modules.Native;

namespace SlashBar.Modules.Settings;

public static class HotkeyChordMapper {

    public static IReadOnlyList<string> ToChips(HotkeyChord chord) {
        var chips = new List<string>();
        if (chord.Control)
            chips.Add("Ctrl");
        if (chord.Shift)
            chips.Add("Shift");
        if (chord.Alt)
            chips.Add("Alt");
        if (!string.IsNullOrWhiteSpace(chord.Key))
            chips.Add(chord.Key);
        return chips;
    }


    public static string ToLabel(HotkeyChord chord) =>
        string.Join("+", ToChips(chord));


    public static bool Same(HotkeyChord? a, HotkeyChord? b) {
        if (a is null || b is null)
            return ReferenceEquals(a, b);

        return a.Control == b.Control
            && a.Shift == b.Shift
            && a.Alt == b.Alt
            && string.Equals(a.Key, b.Key, StringComparison.OrdinalIgnoreCase);
    }


    public static bool TryFromKey(Key key, ModifierKeys modifiers, out HotkeyChord chord) {
        chord = new HotkeyChord();

        if (IsModifierKey(key))
            return false;

        var control = modifiers.HasFlag(ModifierKeys.Control);
        var shift = modifiers.HasFlag(ModifierKeys.Shift);
        var alt = modifiers.HasFlag(ModifierKeys.Alt);

        if (!control && !shift && !alt)
            return false;

        if (!TryKeyName(key, out var name))
            return false;

        chord.Control = control;
        chord.Shift = shift;
        chord.Alt = alt;
        chord.Key = name;
        return true;
    }


    public static bool TryToNative(HotkeyChord chord, out uint modifiers, out uint vk) {
        modifiers = 0;
        vk = 0;

        if (chord is null || string.IsNullOrWhiteSpace(chord.Key))
            return false;

        if (!TryParseKey(chord.Key, out var key))
            return false;

        var code = KeyInterop.VirtualKeyFromKey(key);
        if (code <= 0)
            return false;

        if (chord.Control)
            modifiers |= HotkeyNative.ModControl;
        if (chord.Shift)
            modifiers |= HotkeyNative.ModShift;
        if (chord.Alt)
            modifiers |= HotkeyNative.ModAlt;

        if (modifiers == 0)
            return false;

        vk = (uint)code;
        return true;
    }


    public static bool IsModifierKey(Key key) =>
        key is Key.LeftCtrl or Key.RightCtrl
            or Key.LeftShift or Key.RightShift
            or Key.LeftAlt or Key.RightAlt
            or Key.LWin or Key.RWin
            or Key.System;


    private static bool TryKeyName(Key key, out string name) {
        name = "";

        if (key is >= Key.A and <= Key.Z) {
            name = key.ToString();
            return true;
        }

        if (key is >= Key.D0 and <= Key.D9) {
            name = ((char)('0' + (key - Key.D0))).ToString();
            return true;
        }

        if (key is >= Key.F1 and <= Key.F24) {
            name = key.ToString();
            return true;
        }

        if (key == Key.Space) {
            name = "Space";
            return true;
        }

        // Keep a few navigation / editing keys usable as chords.
        switch (key) {
            case Key.Tab:
            case Key.Enter:
            case Key.Back:
            case Key.Delete:
            case Key.Insert:
            case Key.Home:
            case Key.End:
            case Key.PageUp:
            case Key.PageDown:
            case Key.Left:
            case Key.Right:
            case Key.Up:
            case Key.Down:
            case Key.OemComma:
            case Key.OemPeriod:
            case Key.OemMinus:
            case Key.OemPlus:
                name = key.ToString();
                return true;
            default:
                return false;
        }
    }


    private static bool TryParseKey(string raw, out Key key) {
        key = Key.None;
        var name = raw.Trim();
        if (name.Length == 0)
            return false;

        if (name.Length == 1 && name[0] is >= '0' and <= '9') {
            key = Key.D0 + (name[0] - '0');
            return true;
        }

        return Enum.TryParse(name, ignoreCase: true, out key) && key != Key.None;
    }
}
