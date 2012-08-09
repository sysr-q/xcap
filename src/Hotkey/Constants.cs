using System.Windows.Forms;
namespace xcap
{
    public static class Constants
    {
        public const int 
            NOMOD = 0x0000,
            ALT   = 0x0001,
            CTRL  = 0x0002,
            SHIFT = 0x0004,
            WIN   = 0x0008,
            
            WM_HOTKEY_MSG_ID = 0x0312;

        /// <summary>
        /// A hard-coded piece of junk, which is used by the binding system.
        /// </summary>
        /// <param name="c">The character from the Option's form input.</param>
        /// <returns>The affiliated Keys object, or Keys.None if not found.</returns>
        public static Keys KeysFromChar(char c)
        {
            switch (c)
            {
                case '`':
                case '~':
                    return Keys.Oemtilde;
                case '!':
                    return Keys.D1;
                case '@':
                    return Keys.D2;
                case '#':
                    return Keys.D3;
                case '$':
                    return Keys.D4;
                case '%':
                    return Keys.D5;
                case '^':
                    return Keys.D6;
                case '&':
                    return Keys.D7;
                case '*':
                    return Keys.D8;
                case '(':
                    return Keys.D9;
                case ')':
                    return Keys.D0;
                case '-':
                case '_':
                    return Keys.Subtract;
                case '=':
                case '+':
                    return Keys.Add;
                case '[':
                case '{':
                    return Keys.OemOpenBrackets;
                case ']':
                case '}':
                    return Keys.OemCloseBrackets;
                case '\\':
                case '|':
                    return Keys.OemBackslash;
                case ';':
                case ':':
                    return Keys.OemSemicolon;
                case '\'':
                case '"':
                    return Keys.OemQuotes;
                case ',':
                case '<':
                    return Keys.Oemcomma;
                case '.':
                case '>':
                    return Keys.OemPeriod;
                case '/':
                case '?':
                    return Keys.OemQuestion;
                case ' ':
                    return Keys.Space;
            }

            if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(c.ToString()))
            {
                return (Keys)(byte)char.ToUpper(c);
            }

            if (((Keys)c) >= Keys.D0 && ((Keys)c) <= Keys.D9 || ((Keys)c) >= Keys.NumPad0 && ((Keys)c) <= Keys.NumPad9)
            {
                return (Keys)(byte)c;
            }

            return Keys.None;
        }
    }
}
