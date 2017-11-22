using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace AreaParty.routedcommand
{
    class MyRoutedCommand
    {
        private static RoutedCommand _EditCommand;
        private static RoutedCommand _RemoveCommand;
        private static RoutedCommand _AddCommand;
        private static RoutedCommand _LoginSuccessCommand;
        private static RoutedCommand _LoginFailCommand;
        public static RoutedCommand EditCommand
        {
            get
            {
                return _EditCommand != null ? _EditCommand : (_EditCommand = new RoutedCommand());
            }
        }

        public static RoutedCommand RemoveCommand
        {
            get
            {
                return _RemoveCommand != null ? _RemoveCommand : (_RemoveCommand = new RoutedCommand());
            }
        }
        public static RoutedCommand AddCommand
        {
            get
            {
                return _AddCommand != null ? _AddCommand : (_AddCommand = new RoutedCommand());
            }
        }

        public static RoutedCommand LoginSuccessCommand
        {
            get
            {
                return _LoginSuccessCommand != null ? _LoginSuccessCommand : (_LoginSuccessCommand = new RoutedCommand());
            }
        }
        public static RoutedCommand LoginFailCommand
        {
            get
            {
                return _LoginFailCommand != null ? _LoginFailCommand : (_LoginFailCommand = new RoutedCommand());
            }
        }

    }
}
