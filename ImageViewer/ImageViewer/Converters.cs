using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace ImageViewer
{
    //! 論理値trueをVisibility.Visibleに、falseとnullをVisibility.Corrapsedに変換するコンバータクラス。
    public class FalseCollapseConverter : IValueConverter
    {
        //! 論理値trueをVisibility.Visibleに、falseとnullをVisibility.Collapsedに変換する。
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => ((bool?)value ?? false) ? Visibility.Visible : Visibility.Collapsed;

        //! 逆の変換をサポートしない。
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    //! 論理値trueとnullをVisibility.Collapsed、falseをVisibility.Visibleに変換するクラス。
    public class TrueCollapseConverter : IValueConverter
    {
        //! 論理値trueとnullをVisibility.Collapsed、falseをVisibility.Visibleに変換する。
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => ((bool?)value ?? true) ? Visibility.Collapsed : Visibility.Visible;

        //! 逆の変換をサポートしない。
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    //! 整数型の数値が0以上ならTrue、負値ならFalseに変換するクラス。
    public class IntIsZeroOrPlusConverter : IValueConverter
    {
        //! 論理値trueをVisibility.Collapsed、falseをVisibility.Visibleに変換する。
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => (int)value >= 0;

        //! 逆の変換をサポートしない。
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }
}
