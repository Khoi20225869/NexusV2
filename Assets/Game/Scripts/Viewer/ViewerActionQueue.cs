using System.Collections.Generic;
using SoulForge.Economy;
using UnityEngine;

namespace SoulForge.Viewer
{
    public sealed class ViewerActionQueue : MonoBehaviour
    {
        [SerializeField] private ViewerEconomyConfig config;

        private readonly Queue<ViewerCommand> commands = new();

        public int Count => commands.Count;

        public bool TryEnqueue(in ViewerCommand command)
        {
            int maxCommands = config != null ? config.MaxQueuedCommands : 8;
            if (commands.Count >= maxCommands)
            {
                return false;
            }

            commands.Enqueue(command);
            return true;
        }

        public bool TryDequeue(out ViewerCommand command)
        {
            if (commands.Count == 0)
            {
                command = default;
                return false;
            }

            command = commands.Dequeue();
            return true;
        }
    }
}
