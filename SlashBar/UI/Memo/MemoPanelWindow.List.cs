using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SlashBar.Modules;
using SlashBar.Modules.Memo;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class MemoPanelWindow {

    private static readonly SolidColorBrush AddIconBrush = Freeze(0x34, 0xC7, 0x59);
    private static readonly SolidColorBrush EditIconBrush = Freeze(0x00, 0x7A, 0xFF);
    private static readonly SolidColorBrush DeleteIconBrush = Freeze(0xFF, 0x3B, 0x30);

    private bool _listSubscribed;
    private string? _editingId;


    private static SolidColorBrush Freeze(byte r, byte g, byte b) {
        var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }


    private void SubscribeList() {
        if (_listSubscribed)
            return;

        MemoBook.Store.Changed += OnMemosChanged;
        _listSubscribed = true;
    }


    private void RefreshList() {
        var all = MemoBook.Store.GetAll();

        var query = MemoSearchBox.Text.Trim().ToLowerInvariant();

        if (query.Length == 0) {
            MemoList.ItemsSource = all;
            return;
        }

        MemoList.ItemsSource = all
            .Where(m =>
                m.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                || m.Value.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }


    private void OnMemosChanged() =>
        Dispatcher.Invoke(RefreshList);


    private void MemoNameBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
        var caret = MemoNameBox.CaretIndex;
        var lower = MemoNameBox.Text.ToLowerInvariant();
        if (MemoNameBox.Text == lower)
            return;

        MemoNameBox.Text = lower;
        MemoNameBox.CaretIndex = Math.Min(caret, lower.Length);
    }


    private void AddMemo_Click(object sender, RoutedEventArgs e) {
        var wasEditing = _editingId != null;
        var name = MemoNameBox.Text.Trim();
        var value = MemoValueBox.Text.Trim();

        if (name.Length == 0 || value.Length == 0) {
            ShowToast("!", "Nom et valeur requis", DeleteIconBrush, useMdl2: false);
            return; // on ne vide PAS le formulaire
        }

        var saved = wasEditing
            ? MemoBook.Store.Update(_editingId!, name, value)
            : MemoBook.Store.Add(name, value);

        if (!saved) {
            // cas typique : en édition, le nouveau nom appartient déjà à un autre memo
            ShowToast("!", "Nom déjà pris", DeleteIconBrush, useMdl2: false);
            return;
        }

        if (wasEditing)
            ShowToast("\uE73E", "Modifié", EditIconBrush, useMdl2: true);
        else
            ShowToast("\uE73E", "Ajouté", AddIconBrush, useMdl2: true);

        ClearEditor();
        MemoNameBox.Focus();
    }


    private void EditMemo_Click(object sender, RoutedEventArgs e) {
        e.Handled = true;

        if (sender is not FrameworkElement { Tag: MemoEntry entry })
            return;

        _editingId = entry.Id;
        MemoNameBox.Text = entry.Name;
        MemoValueBox.Text = entry.Value;
        SetEditorMode(editing: true);
        MemoValueBox.Focus();
        MemoValueBox.CaretIndex = MemoValueBox.Text.Length;
    }


    private void MemoItem_Click(object sender, MouseButtonEventArgs e) {
        if (sender is FrameworkElement { Tag: MemoEntry entry }) {
            ClipboardHelper.SetText(entry.Value);
            ShowToast("✓", "Copié", AddIconBrush, useMdl2: false);
        }
    }


    private void MemoSearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
        RefreshList();
    }


    private void DeleteMemo_Click(object sender, RoutedEventArgs e) {
        e.Handled = true;

        if (sender is not FrameworkElement { Tag: MemoEntry entry })
            return;

        if (_editingId == entry.Id)
            ClearEditor();

        MemoBook.Store.Remove(entry.Id);
        ShowToast("\uE711", "Supprimé", DeleteIconBrush, useMdl2: true);
    }


    private void ClearEditor() {
        _editingId = null;
        MemoNameBox.Text = "";
        MemoValueBox.Text = "";
        SetEditorMode(editing: false);
    }


    private void SetEditorMode(bool editing) {
        if (editing) {
            AddMemoIcon.Text = "\uE73E";
            AddMemoIcon.Foreground = EditIconBrush;
            AddMemoButton.ToolTip = "Enregistrer";
            AddMemoButton.Tag = "edit";
        }
        else {
            AddMemoIcon.Text = "\uE710";
            AddMemoIcon.Foreground = AddIconBrush;
            AddMemoButton.ToolTip = "Ajouter";
            AddMemoButton.Tag = "add";
        }
    }


    private void ShowToast(string icon, string message, SolidColorBrush iconBrush, bool useMdl2) {
        ToastIcon.Text = icon;
        ToastIcon.FontFamily = useMdl2
            ? new System.Windows.Media.FontFamily("Segoe MDL2 Assets")
            : new System.Windows.Media.FontFamily("Segoe UI Variable Text, Segoe UI");
        ToastIcon.FontSize = useMdl2 ? 14 : 13;
        ToastIcon.Foreground = iconBrush;
        ToastText.Text = message;
        CopiedToastAnimator.Show(CopiedToast, CopiedToastSlide);
    }
}
