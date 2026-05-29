// NumberFormatter.cs
// Version: 2026-05-29 v1.0
// Purpose: Compact number formatting for UI display only.
// Does not affect runtime values, save/load, or economy calculations.

public static class NumberFormatter
{
    private const long TRILLION = 1000000000000L;
    private const long BILLION  = 1000000000L;
    private const long MILLION  = 1000000L;
    private const long THOUSAND = 1000L;

    /// <summary>
    /// Format a number compactly: 1500 → "1.5K", 17000000 → "17M"
    /// </summary>
    public static string Format(long value)
    {
        bool negative = value < 0;
        long abs = negative ? -value : value;
        string result;

        if (abs >= TRILLION)
            result = FormatWithSuffix(abs, TRILLION, "T");
        else if (abs >= BILLION)
            result = FormatWithSuffix(abs, BILLION, "B");
        else if (abs >= MILLION)
            result = FormatWithSuffix(abs, MILLION, "M");
        else if (abs >= THOUSAND)
            result = FormatWithSuffix(abs, THOUSAND, "K");
        else
            result = abs.ToString();

        return negative ? "-" + result : result;
    }

    public static string Format(int value)
    {
        return Format((long)value);
    }

    public static string Format(float value)
    {
        return Format((long)value);
    }

    public static string Format(double value)
    {
        return Format((long)value);
    }

    /// <summary>
    /// Format with a leading +/- sign: 1500 → "+1.5K", -300 → "-300"
    /// </summary>
    public static string FormatSigned(long value)
    {
        string formatted = Format(value);
        if (value > 0) return "+" + formatted;
        return formatted;
    }

    public static string FormatSigned(int value)
    {
        return FormatSigned((long)value);
    }

    // ================= INTERNAL =================

    private static string FormatWithSuffix(long abs, long divisor, string suffix)
    {
        long whole = abs / divisor;
        long remainder = abs % divisor;

        if (remainder == 0)
            return whole + suffix;

        // Two decimal digits max
        long hundredths = remainder * 100 / divisor;

        if (hundredths == 0)
            return whole + suffix;

        // Trim trailing zero: 1.50 → 1.5
        if (hundredths % 10 == 0)
            return whole + "." + (hundredths / 10) + suffix;

        return whole + "." + hundredths + suffix;
    }
}
