using System;
using System.Linq;
using PenguinSoft.CoreExtensions.Extensions;

namespace PenguinSoft.CoreExtensions.Helpers.Request
{
    public class Filter<T> : IFilter<T>
    {

        public string Field { get; set; }
        public string Operation { get; set; }
        public string Value { get; set; }


        public virtual IQueryable<T> GetQuery(IQueryable<T> query)
        {
            if (Field == null)
                throw new ArgumentException(nameof(Field));
            if (Operation == null)
                throw new ArgumentException(nameof(Operation));

            var prop = typeof(T).GetProperty(Field);
            if (prop == null)
                return query;

            switch (Operation.ToLower())
            {
                case "eq":
                case "equals":
                    return query.Where(x =>
                        prop.GetValue(x).ToString()
                            .Equals(Value, StringComparison.InvariantCulture));
                case "eqi":
                case "equalsInsentitive":
                    return query.Where(x =>
                        prop.GetValue(x).ToString()
                            .EqualsIgnoreCase(Value));
                case "eqn":
                case "equalsNumber":
                    return query.Where(x =>
                        Math.Abs(prop.GetValue(x).ToString().ToDouble( ) - Value.ToDouble()) < 0.0000000001);
                case "ct":
                case "contains":
                    return query.Where(x =>
                        prop.GetValue(x).ToString()
                            .Contains(Value));
                case "cti":
                case "containsInsensitive":
                    return query.Where(x =>
                        prop.GetValue(x).ToString()
                            .ContainsIgnoreCase(Value));
                case "sw":
                case "startsWith":
                    return query.Where(x =>
                        prop.GetValue(x).ToString()
                            .StartsWith(Value, StringComparison.InvariantCulture));
                case "swi":
                case "startsWithInsensitive":
                    return query.Where(x =>
                        prop.GetValue(x).ToString()
                            .StartsWith(Value, StringComparison.InvariantCultureIgnoreCase));
                case "ew":
                case "endsWith":
                    return query.Where(x =>
                        prop.GetValue(x).ToString()
                            .EndsWith(Value, StringComparison.InvariantCulture));
                case "ewi":
                case "endsWithInsensitive":
                    return query.Where(x =>
                        prop.GetValue(x).ToString()
                            .EndsWith(Value, StringComparison.InvariantCultureIgnoreCase));
                case "gt":
                case "greatherThan":
                    return query.Where(x =>
                        prop.GetValue(x).ToString().ToDouble() > Value.ToDouble()
                    );
                case "gte":
                case "greatherThanOrEqual":
                    return query.Where(x =>
                        prop.GetValue(x).ToString().ToDouble() >= Value.ToDouble()
                    );
                case "lt":
                case "lessThan":
                    return query.Where(x =>
                        prop.GetValue(x).ToString().ToDouble() < Value.ToDouble()
                    );
                case "lte":
                case "lessThanOrEqual":
                    return query.Where(x =>
                        prop.GetValue(x).ToString().ToDouble() <= Value.ToDouble()
                    );
                case "bf":
                case "before":
                    return query.Where(x =>
                        prop.GetValue(x).ToString().ToDateTimeTicks() < Value.ToDateTimeTicks()
                    );
                case "bfi":
                case "beforeInclusive":
                    return query.Where(x =>
                        prop.GetValue(x).ToString().ToDateTimeTicks() <= Value.ToDateTimeTicks()
                    );
                case "af":
                case "after":
                    return query.Where(x =>
                        prop.GetValue(x).ToString().ToDateTimeTicks() > Value.ToDateTimeTicks()
                    );
                case "afi":
                case "afterInclusive":
                    return query.Where(x =>
                        prop.GetValue(x).ToString().ToDateTimeTicks() >= Value.ToDateTimeTicks()
                    );
                default:
                    throw new ArgumentException(nameof(Operation));
            }
        }

    }
}