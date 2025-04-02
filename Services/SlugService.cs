using System.Text.RegularExpressions;

namespace RealWorldApp.Services
{
    public static class SlugService
    {
        public static string GenerateSlug(string title)
        {
            return Regex.Replace(title.ToLower().Trim(), @"[^a-z0-9]+", "-").Trim('-');
        }
    }
}
