using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ZoneArea2D : MonoBehaviour
{
    [SerializeField] private ZoneProfileSO profile;

    public ZoneProfileSO Profile => profile;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var affectables = other.GetComponentsInParent<MonoBehaviour>().OfType<IZoneAffectable>();
        foreach (var affectable in affectables)
        {
            affectable.ApplyZone(profile, this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var affectables = other.GetComponentsInParent<MonoBehaviour>().OfType<IZoneAffectable>();
        foreach (var affectable in affectables)
        {
            affectable.RemoveZone(this);
        }
    }
}