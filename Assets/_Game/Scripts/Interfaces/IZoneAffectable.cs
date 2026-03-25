public interface IZoneAffectable
{
    void ApplyZone(ZoneProfileSO profile, ZoneArea2D source);
    void RemoveZone(ZoneArea2D source);
}