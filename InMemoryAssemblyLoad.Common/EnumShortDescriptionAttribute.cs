using System;

namespace InMemoryAssemblyLoad.Common
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnumShortDescriptionAttribute : Attribute
    {
        private readonly string _description;
        public string Description => _description;

        public EnumShortDescriptionAttribute(string description)
        {
            _description = description;
        }
    }
}