using System;
using System.Collections.Generic;
using DrMuscle.Entity;

namespace DrMuscle.Helpers
{
    public static class Languages
    {
        public static List<Language> GetAppLanguages()
        {
            return new List<Language>{
            new Language("Flag_English.png", "English", "en", false),
            new Language("Flag_French.png", "Français", "fr-FR", false),
            new Language("Flag_Swedish.png", "Svenska", "sv", false),
            new Language("Flag_Germany.png", "Deutsch", "de-CH", false),
            };
        }
    }
}
