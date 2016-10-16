using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DPA_Musicsheets
{
    public class AdvancedTabControl : TabControl
    {
        static AdvancedTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AdvancedTabControl),
                   new FrameworkPropertyMetadata(typeof(AdvancedTabControl)));
        }
    }
}
