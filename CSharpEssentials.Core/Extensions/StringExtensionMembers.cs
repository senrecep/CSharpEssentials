#if NET10_0_OR_GREATER
namespace CSharpEssentials;

public static class StringExtensionMembers
{
    extension(string str)
    {
        public bool IsPalindrome
        {
            get
            {
                if (string.IsNullOrEmpty(str))
                    return true;
                for (int i = 0, j = str.Length - 1; i < j; i++, j--)
                {
                    if (str[i] != str[j])
                        return false;
                }
                return true;
            }
        }
    }
}
#endif
