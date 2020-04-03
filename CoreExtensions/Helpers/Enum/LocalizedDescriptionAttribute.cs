using System;
using System.ComponentModel;
using System.Reflection;

namespace PenguinSoft.CoreExtensions.Helpers.Enum
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _descriptionKey;
        private Type _nameResourceType;
        private PropertyInfo _nameProperty;

        public Type NameResourceType
        {
            get => _nameResourceType;
            set
            {
                _nameResourceType = value;
                _nameProperty =
                    _nameResourceType.GetProperty(_descriptionKey, BindingFlags.Public | BindingFlags.NonPublic);
            }
        }

        public override string Description
        {
            get
            {
                if (_nameProperty == null)
                    return base.Description;

                return (string)_nameProperty.GetValue(_nameProperty.DeclaringType, null);
            }
        }

        public LocalizedDescriptionAttribute(string descriptionKey, Type nameResource)
            : base(descriptionKey)
        {
            _descriptionKey = descriptionKey;
            NameResourceType = nameResource;
        }

    }
}