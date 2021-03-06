﻿using System;
using System.Globalization;
using System.Windows.Data;
using Memenim.Core.Schema;
using Memenim.Extensions;

namespace Memenim.Converters
{
    public sealed class UserSexTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UserSexType result = UserSexType.Unknown;

            if (value is int intValue)
                result = (UserSexType)((byte)intValue);
            else if (value is UserSexType typeValue)
                result = typeValue;

            return result.GetLocalizedName();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UserSexType result = UserSexType.Unknown;

            if (value is string stringValue)
                result = UserSexType.Unknown.ParseLocalizedName<UserSexType>(stringValue);

            return result;
        }
    }
}
