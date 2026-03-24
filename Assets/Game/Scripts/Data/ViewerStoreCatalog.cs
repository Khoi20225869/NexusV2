using System.Collections.Generic;
using UnityEngine;

namespace SoulForge.Data
{
    [CreateAssetMenu(menuName = "SoulForge/Viewer Store Catalog", fileName = "ViewerStoreCatalog")]
    public sealed class ViewerStoreCatalog : ScriptableObject
    {
        [field: SerializeField] public List<ViewerActionDefinition> Actions { get; private set; } = new();
    }
}
