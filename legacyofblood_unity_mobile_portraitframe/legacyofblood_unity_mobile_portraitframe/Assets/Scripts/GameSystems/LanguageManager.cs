namespace LegendOfBlood
{
    public enum Language
    {
        English,
        Vietnamese
    }

    public static class LanguageManager
    {
        public static Language CurrentLanguage { get; set; } = Language.Vietnamese; // Default language
    }
}