namespace TransactionTypeConstructor.Helper
{
    public class Utils
    {
        public static string CriptoPassword(string s)
        {
            string Str_Prl;
            Str_Prl = "";
            int i_len;
            int i_kod;
            i_len = s.Length;
            for (int i_r = 0; i_r < i_len; i_r++)
            {
                i_kod = (int)s[i_r];

                i_kod = i_kod + i_kod / 600 + i_kod / 100 + i_kod / 50;

                if (i_kod < 65)
                    i_kod = i_kod + 15;
                if (i_kod > 100)
                    i_kod = i_kod - 23;
                while (!(((i_kod > 64) && (i_kod < 91)) || ((i_kod > 96) && (i_kod < 123))))
                {
                    if (i_kod < 65)
                    {
                        while (i_kod < 65)
                            i_kod = i_kod + 25;
                        break;
                    }
                    if ((i_kod > 90) && (i_kod < 97))
                    {
                        i_kod = i_kod - 9;
                        break;
                    }
                    if (i_kod > 122)
                    {
                        while (i_kod > 122)
                            i_kod = i_kod - 26;
                        break;
                    }
                }
                Str_Prl = Str_Prl + (char)i_kod;
            }
            return Str_Prl;
        }
    }
}
