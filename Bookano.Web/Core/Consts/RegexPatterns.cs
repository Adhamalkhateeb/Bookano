using Newtonsoft.Json.Serialization;

namespace Bookano.Web.Core.Consts
{
    public static class RegexPatterns
    {
        public const string Password = "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{8,}$";
        public const string Username = "^[a-zA-Z0-9-._@+]*$";
        public const string CharactersOnly_Eng = "^[a-zA-Z-_ ]*$";
        public const string CharactersOnly_Ar = "^[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FF ]*$";
        public const string NumbersAndChrOnly_ArEng =
            "^(?=.*[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FFa-zA-Z])[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FFa-zA-Z0-9 _-]+$";
        public const string DenySpecialCharacters = "^[^<>!#%$]*$";
        public const string MobileNumber = "^01[0125]\\d{8}$";
        public const string AreaName = "^[\u0600-\u06FFa-zA-Z0-9 _-]+$";
        public const string NationalId =
            "^[23]([0-9]{2})(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])(01|02|03|04|11|12|13|14|15|16|17|18|19|21|22|23|24|25|26|27|28|29|31|32|33|34|35|88)\\d{5}$";
    }
}
