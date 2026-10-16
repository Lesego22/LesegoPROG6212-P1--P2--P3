namespace RaceDay.Api.Models
{
    public static class RoleNames
    {
        public const string Organiser = "Organiser";
        public const string Participant = "Participant";
    }
    public static class RoleIds
    {
        public static readonly Guid Organiser = new("11111111-1111-1111-1111-111111111111");
        public static readonly Guid Participant = new("22222222-2222-2222-2222-222222222222");
    }

    public static class EventTypes
    {
        public const string Run = "Run";
        public const string Walk = "Walk";
        public const string Cycle = "Cycle";
        public static readonly string[] All = { Run, Walk, Cycle };

        public static bool TryNormalise(string? input, out string normalised)
        {
            var match = All.FirstOrDefault(t => string.Equals(t, input?.Trim(), StringComparison.OrdinalIgnoreCase));
            normalised = match ?? string.Empty;
            return match is not null;
        }
    }

    public static class EventStatuses
    {
        public const string Scheduled = "Scheduled";
        public const string Cancelled = "Cancelled";
        public const string Completed = "Completed";
        public static readonly string[] All = { Scheduled, Cancelled, Completed };

        public static bool TryNormalise(string? input, out string normalised)
        {
            var match = All.FirstOrDefault(t => string.Equals(t, input?.Trim(), StringComparison.OrdinalIgnoreCase));
            normalised = match ?? string.Empty;
            return match is not null;
        }
    }

    public static class EnrolmentStatuses
    {
        public const string Active = "Active";
        public const string Cancelled = "Cancelled";
    }
}

