using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows; 
using System.Text;

namespace Metro.Theme.Dark
{
    partial class WindowStyleDark
    {

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Window wnd = ((FrameworkElement)sender).TemplatedParent as Window;
            if (wnd != null)
            {
                wnd.Close();
            }
        }


        private void MaximizeClick(object sender, RoutedEventArgs e)
        {
            Window wnd = ((FrameworkElement)sender).TemplatedParent as Window;
            if (wnd != null)
            {
                wnd.WindowState = System.Windows.WindowState.Maximized;
            }
        }


        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            Window wnd = ((FrameworkElement)sender).TemplatedParent as Window;
            if (wnd != null)
            {
                wnd.WindowState = System.Windows.WindowState.Minimized;
            }
        }


        private void RestoreClick(object sender, RoutedEventArgs e)
        {
            Window wnd = ((FrameworkElement)sender).TemplatedParent as Window;
            if (wnd != null)
            {
                wnd.WindowState = System.Windows.WindowState.Normal;
            }
        }

      
    }
}
