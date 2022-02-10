using System;
namespace Rinzler78.NetExtension.Enums
{
    public static class EnumsHelper
    {
        public static ToEnumType Convert<FromEnumType, ToEnumType>(this FromEnumType obj)
            where FromEnumType : struct
            where ToEnumType : struct
        {
            try
            {
                var fromEnumString = obj.ToString();
                return fromEnumString.Convert<ToEnumType>();
            }
            catch (Exception ex)
            {
            }

            return default;
        }

        public static ToEnumType Convert<ToEnumType>(this string str)
            where ToEnumType : struct
        {
            try
            {
                ToEnumType result = default;

                if (Enum.TryParse(str, out result))
                    return result;
            }
            catch (Exception ex)
            {
                
            }

            return default;
        }
    }
}
