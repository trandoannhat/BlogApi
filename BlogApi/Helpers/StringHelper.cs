using System.Text;
using System.Text.RegularExpressions;

namespace BlogApi.Helpers
{
    public static class StringHelper
    {
        public static string ToSlug(this string text)
        {
            string str = text.ToLower().Normalize(NormalizationForm.FormD);
            Regex reg = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
            string temp = reg.Replace(str, "").Replace('đ', 'd').Replace('Đ', 'd');
            temp = Regex.Replace(temp, @"[^a-z0-9\s-]", "");
            return Regex.Replace(temp, @"\s+", "-").Trim('-');
        }
    }
}
