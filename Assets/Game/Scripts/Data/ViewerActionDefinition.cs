using SoulForge.Viewer;
using UnityEngine;

namespace SoulForge.Data
{
    [CreateAssetMenu(menuName = "SoulForge/Viewer Action Definition", fileName = "ViewerActionDefinition")]
    public sealed class ViewerActionDefinition : ScriptableObject
    {
        [field: SerializeField] public string ActionId { get; private set; } = "spawn_weak_enemy";
        [field: SerializeField] public string DisplayName { get; private set; } = "Spawn Weak Enemy";
        [field: SerializeField] public string Description { get; private set; } = "Add pressure to the host with a light enemy.";
        [field: SerializeField] public ViewerActionCategory Category { get; private set; } = ViewerActionCategory.Hostile;
        [field: SerializeField] public int Price { get; private set; } = 25;
        [field: SerializeField] public float Cooldown { get; private set; } = 5f;
        [field: SerializeField] public int BudgetCost { get; private set; } = 1;
        [field: SerializeField] public string TargetId { get; private set; } = "";
        [field: SerializeField] public float HealAmount { get; private set; } = 1f;
        [field: SerializeField] public Color AccentColor { get; private set; } = Color.white;
    }
}
