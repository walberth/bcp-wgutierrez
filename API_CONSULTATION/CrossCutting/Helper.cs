using System.Reflection;
using System.Runtime.Serialization;

namespace API_CONSULTATION.CrossCutting
{
    public static class Helper
    {
        public static string? GetEnumMemberValue<T>(this T value) where T : Enum =>
            typeof(T)
                .GetTypeInfo()
                .DeclaredMembers
                .SingleOrDefault(x => x.Name == value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value;

        public static T TryParseEnum<T>(this string value) where T : struct, Enum
        {
            var type = typeof(T);
            foreach (var field in type.GetFields())
            {
                if (field.GetCustomAttributes(typeof(EnumMemberAttribute), false).FirstOrDefault() is EnumMemberAttribute attribute)
                {
                    if (attribute.Value == value)
                    {
                        return (T)field.GetValue(null);
                    }
                }
                else if (field.Name == value)
                {
                    return (T)field.GetValue(null);
                }
            }
            return default;
        }
    }
}
