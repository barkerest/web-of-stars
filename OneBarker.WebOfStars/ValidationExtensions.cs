using System.ComponentModel.DataAnnotations;

namespace OneBarker.WebOfStars;

public static class ValidationExtensions
{
    /// <summary>
    /// Gets all of the errors from the item.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static IDictionary<string, string[]> GetErrors(this IValidatableObject item)
    {
        var ctx = new ValidationContext(item);
        var ret = new Dictionary<string, List<string>>();

        foreach (var err in item.Validate(ctx))
        {
            var mems = err.MemberNames.ToArray();
            if (!mems.Any())
            {
                mems = new[] { "@" };
            }

            foreach (var mem in mems)
            {
                if (!ret.ContainsKey(mem)) ret[mem] = new List<string>();
                ret[mem].Add(err.ErrorMessage ?? "(no error provided)");
            }
        }

        return ret.ToDictionary(x => x.Key, x => x.Value.ToArray());
    }

    /// <summary>
    /// Tests the item for validity.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static bool IsValid(this IValidatableObject item) 
        => !item.Validate(new ValidationContext(item)).Any();
}
