using UnityEngine;

public class LineEntity : MonoBehaviour
{
    public enum EntityType
    {
        Player,
        NPC
    }

    public EntityType entityType = EntityType.NPC;
    public int lineIndex = -1;

    public void Initialize(int index, EntityType type)
    {
        lineIndex = index;
        entityType = type;
    }

    public bool IsPlayer => entityType == EntityType.Player;
}