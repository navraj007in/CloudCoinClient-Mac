using System;
using Foundation;
using System.Linq;

namespace CloudCoinMAC
{
    public class NumberOnlyFormattter : NSNumberFormatter
    {
        public override bool IsPartialStringValid(string partialString, out string newString, out NSString error)
        {
            newString = partialString;
            error = new NSString("");
            if (partialString.Length == 0)
                return true;

            // you could allow use partialString.All(c => c >= '0' && c <= '9') if internationalization is not a concern
            if (partialString.All(char.IsDigit))
                return true;
            newString = new string(partialString.Where(char.IsDigit).ToArray());
            return false;
        }
    }
}
