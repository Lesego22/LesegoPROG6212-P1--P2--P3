namespace RaceDay.Web.Models;

public static class Roles
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";

    
    public static readonly Guid OrganiserId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ParticipantId = new("22222222-2222-2222-2222-222222222222");
}
