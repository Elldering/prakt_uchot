using Aardvark.Base.Native;
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
using System.Windows.Shapes;

namespace BudGet2._0
{
    /// <summary>
    /// Логика взаимодействия для CreateType.xaml
    /// </summary>
    public partial class CreateType : Window
    {
        public CreateType()
        {
            InitializeComponent();
        }


        public void Types()
        {
            if (!MainWindow.typesOfNote.Contains(TypeNoteTBox.Text))
            {
                MainWindow.typesOfNote.Add(TypeNoteTBox.Text);
            }
            Close();
        }

        private void CreateNewType_Click(object sender, RoutedEventArgs e)
        {
            Types();
        }
    }
}
