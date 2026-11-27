using System.ComponentModel.DataAnnotations;

namespace DirectoryService.Api;

internal static class DirectoryRequestValidation
{
    public static Dictionary<string, string[]>? Validate<T>(T request) where T : notnull
    {
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true))
        {
            return null;
        }

        return results
            .GroupBy(result => result.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(
                group => group.Key,
                group => group.Select(result => result.ErrorMessage ?? "Invalid value.").ToArray());
    }
}
