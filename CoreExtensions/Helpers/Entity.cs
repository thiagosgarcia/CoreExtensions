using System;

namespace PenguinSoft.CoreExtensions.Helpers
{
    public abstract class Entity
    {
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        public override bool Equals(object obj)
        {
            if (!(obj is Entity))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (GetType() != obj.GetType())
                return false;

            var item = (Entity)obj;

            return item.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ 31;
        }
    }
}