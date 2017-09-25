using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace PowerSupplyNotifier.Code.ValidationRules
{
    public class LowerLevelsFieldRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = (string) value;

            var split = str.Split(',').Select(it => it.Trim());

            foreach (var item in split)
            {
                if (!int.TryParse(item, out int val))
                    return new ValidationResult(false, "");

                if (val < 0 || val > 99)
                    return new ValidationResult(false, "");
            }

            return ValidationResult.ValidResult;
        }
    }
}
