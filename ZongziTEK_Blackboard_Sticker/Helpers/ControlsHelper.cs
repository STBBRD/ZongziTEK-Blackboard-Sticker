using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ZongziTEK_Blackboard_Sticker.Helpers
{
    public class ControlsHelper
    {
        public static void SetDynamicResource(DependencyObject obj, DependencyProperty dp, object resourceKey)
        {
            var dynamicResource = new DynamicResourceExtension(resourceKey);
            obj.SetValue(dp, dynamicResource.ProvideValue(null));
        }
    }
}
