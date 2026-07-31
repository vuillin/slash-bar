using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SlashBar.Modules;

public sealed class CalcModule : IModule {

    private static readonly Regex Allowed =
        new(@"^[0-9+\-*/().\s]+$", RegexOptions.Compiled);

    public string Prefix => "calc";
    public string Name => "Calculator";
    public string Description => "Evaluate an expression";


    public ModuleResult Execute(string argument) {

        if (!TryEvaluate(argument, out var text, out var error))
            return ModuleResult.Error(error);

        ClipboardHelper.SetText(text);
        return ModuleResult.Copied(text);

    }


    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) {

        if (!TryEvaluate(argument, out var text, out _))
            return Array.Empty<ArgCompletion>();

        // Value = current expression (no ghost); Description = preview
        return [new ArgCompletion(argument.Trim(), $"= {text}")];

    }


    private static bool TryEvaluate(string argument, out string text, out string error) {

        text = "";
        error = "";
        var expr = Normalize(argument);

        if (expr.Length == 0) {
            error = "Expression required";
            return false;
        }

        if (!Allowed.IsMatch(expr)) {
            error = "Invalid expression";
            return false;
        }
        
        try {
            var value = new DataTable().Compute(expr, null);
            text = Format(value);
            return true;
        }
        catch (DivideByZeroException) {
            error = "Division by zero";
            return false;
        }
        catch {
            error = "Invalid expression";
            return false;
        }
    }


    private static string Normalize(string input) {

        var sb = new StringBuilder(input.Length);

        foreach (var ch in input.Trim()) {

            switch (ch) {

                case '×' or '⋅' or '·':
                    sb.Append('*');
                    break;
                case '÷' or '／':
                    sb.Append('/');
                    break;
                case '−' or '–' or '—':
                    sb.Append('-');
                    break;
                case ',':
                    sb.Append('.');
                    break;
                case '\u00A0' or '\u202F': // non-breaking spaces
                    sb.Append(' ');
                    break;
                default:
                    sb.Append(ch);
                    break;

            }

        }
        return sb.ToString().Trim();
    }


    private static string Format(object? value) {

        if (value is null or DBNull)
            return "0";

        var d = Convert.ToDecimal(value, CultureInfo.InvariantCulture);

        // exact integer → no decimals
        if (d == decimal.Truncate(d))
            return decimal.Truncate(d).ToString(CultureInfo.InvariantCulture);
            
        // otherwise trim trailing zeros (~10 useful decimals max)
        return d.ToString("0.##########", CultureInfo.InvariantCulture);

    }

}