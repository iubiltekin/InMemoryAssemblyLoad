using System;
using System.Linq;

namespace InMemoryAssemblyLoad.Common
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var description = value.ToString();
            var fieldInfo = value.GetType().GetField(description);

            if (fieldInfo != null)
            {
                if (fieldInfo.GetCustomAttributes(typeof(EnumDescriptionAttribute), true).FirstOrDefault() is EnumDescriptionAttribute currentEnumDescriptionAttribute)
                    description = currentEnumDescriptionAttribute.Description;
            }

            return description;
        }

        public static string GetShortDescription(this Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var description = value.ToString();
            var fieldInfo = value.GetType().GetField(description);

            if (fieldInfo != null)
            {
                if (fieldInfo.GetCustomAttributes(typeof(EnumShortDescriptionAttribute), true).FirstOrDefault() is EnumShortDescriptionAttribute currentEnumShortDescriptionAttribute)
                    description = currentEnumShortDescriptionAttribute.Description;
            }

            return description;
        }
    }
}
