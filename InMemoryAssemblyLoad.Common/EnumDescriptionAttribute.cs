using System;

namespace InMemoryAssemblyLoad.Common
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnumDescriptionAttribute : Attribute
    {
        private readonly string _description;
        public string Description => _description;

        public EnumDescriptionAttribute(string description)
        {
            _description = description;
        }
    }
}