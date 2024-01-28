namespace OnlineCoursePlatform.Extensions
{
    public static class StringExtensions
    {
        public static bool ContainsWords(this string text, string words)
        {
            var wordsArray = words.Trim().Split(' ');
            return wordsArray.All(word => text.Contains(word, StringComparison.OrdinalIgnoreCase));
        }
    }
}