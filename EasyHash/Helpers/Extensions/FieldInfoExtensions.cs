namespace EasyHash.Helpers.Extensions
{
    using System.Reflection;

    internal static class FieldInfoExtensions
    {
        public static bool IsBackingField(this FieldInfo field) => field.Name.EndsWith(">k__BackingField");
    }
}
