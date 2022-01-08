using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary
{
    public static class ObjectProcess
    {
        //public static T1 CopyPropertiesFrom<T1, T2>(this T1 obj, T2 otherObject)
        //    where T1 : class
        //    where T2 : class
        //{
        //    PropertyInfo[] srcFields = otherObject.GetType().GetProperties(
        //        BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

        //    PropertyInfo[] destFields = obj.GetType().GetProperties(
        //        BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

        //    foreach (var property in srcFields)
        //    {
        //        var dest = destFields.FirstOrDefault(x => x.Name == property.Name);
        //        if (dest != null && dest.CanWrite)
        //            dest.SetValue(obj, property.GetValue(otherObject, null), null);
        //    }

        //    return obj;
        //}
        public static void CopyTo<T1, T2>(this T1 source, ref T2 destination)
        {
            //https://www.programmingnotes.org/7521/cs-how-to-copy-all-properties-fields-from-one-object-to-another-using-cs/

            if (source is null)
                return;

            var sourceMembers = GetMembers(source.GetType());
            var destinationMembers = GetMembers(destination.GetType());

            // Copy data from source to destination
            foreach (var sourceMember in sourceMembers)
            {
                if (!CanRead(sourceMember))
                {
                    continue;
                }
                var destinationMember = destinationMembers.FirstOrDefault(x => x.Name.ToLower() == sourceMember.Name.ToLower());
                if (destinationMember == null || !CanWrite(destinationMember))
                {
                    continue;
                }
                SetObjectValue(ref destination, destinationMember, GetMemberValue(source, sourceMember));
            }
        }

        private static void SetObjectValue<T>(ref T obj, System.Reflection.MemberInfo member, object value)
        {
            // Boxing method used for modifying structures
            var boxed = obj.GetType().IsValueType ? (object)obj : obj;
            SetMemberValue(ref boxed, member, value);
            obj = (T)boxed;
        }

        private static void SetMemberValue<T>(ref T obj, System.Reflection.MemberInfo member, object value)
        {
            if (IsProperty(member))
            {
                var prop = (System.Reflection.PropertyInfo)member;
                if (prop.SetMethod != null)
                {
                    prop.SetValue(obj, value);
                }
            }
            else if (IsField(member))
            {
                var field = (System.Reflection.FieldInfo)member;
                field.SetValue(obj, value);
            }
        }

        private static object GetMemberValue(object obj, System.Reflection.MemberInfo member)
        {
            object result = null;
            if (IsProperty(member))
            {
                var prop = (System.Reflection.PropertyInfo)member;
                result = prop.GetValue(obj, prop.GetIndexParameters().Count() == 1 ? new object[] { null } : null);
            }
            else if (IsField(member))
            {
                var field = (System.Reflection.FieldInfo)member;
                result = field.GetValue(obj);
            }
            return result;
        }

        private static bool CanWrite(System.Reflection.MemberInfo member)
        {
            return IsProperty(member) ? ((System.Reflection.PropertyInfo)member).CanWrite : IsField(member);
        }

        private static bool CanRead(System.Reflection.MemberInfo member)
        {
            return IsProperty(member) ? ((System.Reflection.PropertyInfo)member).CanRead : IsField(member);
        }

        private static bool IsProperty(System.Reflection.MemberInfo member)
        {
            return IsType(member.GetType(), typeof(System.Reflection.PropertyInfo));
        }

        private static bool IsField(System.Reflection.MemberInfo member)
        {
            return IsType(member.GetType(), typeof(System.Reflection.FieldInfo));
        }

        private static bool IsType(System.Type type, System.Type targetType)
        {
            return type.Equals(targetType) || type.IsSubclassOf(targetType);
        }

        private static List<System.Reflection.MemberInfo> GetMembers(System.Type type)
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic;
            var members = new List<System.Reflection.MemberInfo>();
            members.AddRange(type.GetProperties(flags));
            members.AddRange(type.GetFields(flags));
            return members;
        }
        //public static void WriteExceptionDetails(Exception exception, StringBuilder builderToFill, int level)
        //{
        //    var indent = new string(' ', level);

        //    if (level > 0)
        //    {
        //        builderToFill.AppendLine(indent + "=== INNER EXCEPTION ===");
        //    }

        //    Action<string> append = (prop) =>
        //    {
        //        var propInfo = exception.GetType().GetProperty(prop);
        //        var val = propInfo.GetValue(exception);

        //        if (val != null)
        //        {
        //            builderToFill.AppendFormat("{0}{1}: {2}{3}", indent, prop, val.ToString(), Environment.NewLine);
        //        }
        //    };

        //    append("Message");
        //    append("HResult");
        //    append("HelpLink");
        //    append("Source");
        //    append("StackTrace");
        //    append("TargetSite");

        //    foreach (DictionaryEntry de in exception.Data)
        //    {
        //        builderToFill.AppendFormat("{0} {1} = {2}{3}", indent, de.Key, de.Value, Environment.NewLine);
        //    }

        //    if (exception.InnerException != null)
        //    {
        //        WriteExceptionDetails(exception.InnerException, builderToFill, ++level);
        //    }
        //}
    }
}
