using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ship_Progress
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent(); // partial 키워드가 빠져있으면 안 됩니다!
        }
    }
}
