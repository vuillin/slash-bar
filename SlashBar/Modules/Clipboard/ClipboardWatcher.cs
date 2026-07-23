using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace SlashBar.Modules.Clipboard;

public sealed class ClipboardWatcher {

    private const int WM_CLIPBOARDUPDATE = 0x031D;
    private static readonly IntPtr HwndMessage = new(-3);

    private readonly ClipboardHistoryStore _store;
    private HwndSource? _source;
    private string _lastSeen = "";
    private bool _ignoreNext;


    public ClipboardWatcher(ClipboardHistoryStore store) {
        _store = store;
    }


    public void Start() {
        
        if (_source != null)
            return;

        var parameters = new HwndSourceParameters("SlashBarClipboardWatcher") {
            Width = 0,
            Height = 0,
            WindowStyle = 0,
            ParentWindow = HwndMessage
        };

        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);

        AddClipboardFormatListener(_source.Handle);
    }


    public void Stop() {

        if (_source == null)
            return;

        RemoveClipboardFormatListener(_source.Handle);
        _source.RemoveHook(WndProc);
        _source.Dispose();
        _source = null;
    }


    public void IgnoreNext() => _ignoreNext = true;


    private IntPtr WndProc(
        IntPtr hwnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled) {

            if (msg == WM_CLIPBOARDUPDATE) {
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    Capture);
            }

        return IntPtr.Zero;
    }


    private void Capture() {
        try {

            if (!System.Windows.Clipboard.ContainsText())
                return;

            var text = System.Windows.Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(text))
                return;

            // même contenu que la dernière fois → ignore.
            if (text == _lastSeen)
                return;
            _lastSeen = text;

            if (_ignoreNext) {
                _ignoreNext = false;
                return;
            }
            
            _store.Add(text);
        }
        catch {
            // clipboard parfois encore verrouillé malgré le BeginInvoke.
        }
    }


    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

}