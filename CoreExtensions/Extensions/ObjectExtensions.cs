using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PenguinSoft.CoreExtensions.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToXml<T>(this T value, bool isIndent = false, bool omitXmlDeclaration = false, string xmlRootAttribute = null, int codePageDefault = 65001)
        {
            var xmlSerializer = string.IsNullOrWhiteSpace(xmlRootAttribute) ?
                new XmlSerializer(typeof(T)) :
                new XmlSerializer(typeof(T), new XmlRootAttribute(xmlRootAttribute));

            using (var stringWriter = new StringWriter())
            {
                var xNameSpace = new XmlSerializerNamespaces();
                xNameSpace.Add("", ""); //para remover referencias de namespace
                using (var xmlWriter = XmlWriter.Create(stringWriter,
                    new XmlWriterSettings
                    {
                        OmitXmlDeclaration = omitXmlDeclaration,
                        Indent = isIndent,
                        NamespaceHandling = NamespaceHandling.OmitDuplicates,
                        Encoding = Encoding.GetEncoding(codePageDefault)
                    }))
                {
                    xmlSerializer.Serialize(xmlWriter, value, xNameSpace);
                    return stringWriter.ToString();
                }
            }
        }

        public static IDictionary<string, object> ToDictionary(this object @object)
        {
            var dictionary = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            if (@object != null)
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(@object))
                    dictionary.Add(property.Name, property.GetValue(@object));

            return dictionary;
        }

        public static IDictionary<string, string> ToStringDictionary(this object @object)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            if (@object != null)
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(@object))
                    dictionary.Add(property.Name, (string)property.GetValue(@object));

            return dictionary;
        }

        public static Dictionary<string, string> CopyStringDictionary(this Dictionary<string, string> original)
        {
            var newDictionary = new Dictionary<string, string>();
            if (original == null)
                return newDictionary;

            foreach (var item in original)
                newDictionary.Add(item.Key, item.Value);

            return newDictionary;
        }

        public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs(this object @object)
        {
            if (@object != null)
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(@object))
                    yield return new KeyValuePair<string, object>(property.Name, property.GetValue(@object));
        }

        public static object GetValue(this object model, string property)
        {
            return model.GetType().GetProperty(property)?.GetValue(model, new object[0]);
        }

        public static dynamic PopulateProperties(this object obj, params object[] anotherObject)
        {
            var properties = anotherObject.Where(x => x != null)
                .SelectMany(o => o?.GetType()?.GetProperties(), (o, p) => (o: o, p: p));

            foreach (var prop in properties)
            {
                var pp = obj.GetType().GetProperty(prop.p.Name);
                if (pp == null) continue;
                if (pp.PropertyType == prop.p.PropertyType)
                    pp.SetValue(obj, prop.p.GetValue(prop.o));
            }

            return obj;
        }

        public static bool IsNullOrDefault(this int? val)
        {
            return !val.HasValue || val.Value.IsNullOrDefault();
        }
        public static bool IsNullOrDefault(this int val)
        {
            return val == new int();
        }
        public static bool IsNullOrDefault(this long? val)
        {
            return !val.HasValue || val.Value.IsNullOrDefault();
        }
        public static bool IsNullOrDefault(this long val)
        {
            return val == new long();
        }
        public static bool IsNullOrDefault(this double? val)
        {
            return !val.HasValue || val.Value.IsNullOrDefault();
        }
        public static bool IsNullOrDefault(this double val)
        {
            return val.CompareTo(new double()) == 0;
        }
        public static bool IsNullOrDefault(this float? val)
        {
            return !val.HasValue || val.Value.IsNullOrDefault();
        }
        public static bool IsNullOrDefault(this float val)
        {
            return val.CompareTo(new float()) == 0;
        }
        public static bool IsNullOrDefault(this byte? val)
        {
            return !val.HasValue || val.Value.IsNullOrDefault();
        }
        public static bool IsNullOrDefault(this byte val)
        {
            return val == new byte();
        }
        public static bool IsNullOrDefault(this char? val)
        {
            return !val.HasValue || val.Value.IsNullOrDefault();
        }
        public static bool IsNullOrDefault(this char val)
        {
            return val == new char();
        }
        public static bool IsNullOrDefault(this decimal? val)
        {
            return !val.HasValue || val.Value.IsNullOrDefault();
        }
        public static bool IsNullOrDefault(this decimal val)
        {
            return val == new decimal();
        }
        public static bool IsNullOrDefault(this uint? val)
        {
            return !val.HasValue || val.Value.IsNullOrDefault();
        }
        public static bool IsNullOrDefault(this uint val)
        {
            return val == new uint();
        }
        public static bool IsNullOrDefault(this ulong? val)
        {
            return !val.HasValue || val.Value.IsNullOrDefault();
        }
        public static bool IsNullOrDefault(this ulong val)
        {
            return val == new ulong();
        }
        public static bool IsNullOrDefault(this short? val)
        {
            return !val.HasValue || val.Value.IsNullOrDefault();
        }
        public static bool IsNullOrDefault(this short val)
        {
            return val == new short();
        }
        public static bool IsNullOrDefault(this ushort? val)
        {
            return !val.HasValue || val.Value.IsNullOrDefault();
        }
        public static bool IsNullOrDefault(this ushort val)
        {
            return val == new ushort();
        }
        public static bool IsNullOrDefault(this sbyte? val)
        {
            return !val.HasValue || val.Value.IsNullOrDefault();
        }
        public static bool IsNullOrDefault(this sbyte val)
        {
            return val == new sbyte();
        }
        public static bool IsSimpleType( //TODO There's a better way to do this. Refactor
            this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new Type[] {
                    typeof(string),
                    typeof(decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }
    }
}