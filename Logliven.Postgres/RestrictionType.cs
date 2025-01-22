namespace Logliven.Postgres;

[Flags]
public enum RestrictionType {
    AllowImages = 1,
    AllowAttachments = 2,
    AllowLinks = 4,
}