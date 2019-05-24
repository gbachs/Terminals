namespace Terminals.Connections
{
    internal class PluginDefinition
    {
        public PluginDefinition(string fullPath, string description)
        {
            this.FullPath = fullPath;
            this.Description = description;
        }

        internal string Description { get; }

        internal string FullPath { get; }

        public override string ToString()
        {
            return this.Description;
        }
    }
}