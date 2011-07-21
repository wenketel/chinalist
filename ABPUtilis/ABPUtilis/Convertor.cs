using Microsoft.VisualBasic;

namespace ABPUtils
{
    static class Convertor
    {
        public static string ToSimplified(string source)
        {
            System.Globalization.CultureInfo cl = new System.Globalization.CultureInfo("zh-CN", false);

            return Strings.StrConv(source, VbStrConv.SimplifiedChinese, cl.LCID); 
        }
    }
}
