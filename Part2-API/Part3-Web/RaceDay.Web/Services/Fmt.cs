namespace RaceDay.Web.Services;


public static class Fmt
{
    
    public static string Ordinal(int number)
    {
        var lastTwo = number % 100;
        if (lastTwo is >= 11 and <= 13) return number + "th";

        return (number % 10) switch
        {
            1 => number + "st",
            2 => number + "nd",
            3 => number + "rd",
            _ => number + "th"
        };
    }

    public static string Time(TimeSpan time) => time.ToString(@"hh\:mm\:ss");
}
