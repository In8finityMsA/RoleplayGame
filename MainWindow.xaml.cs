using KASHGAMEWPF;
using KashTaskWPF.Adapters;
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

namespace KashTaskWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IAdapter adapter;

        public static MainWindow mainwindow;

        public MainWindow()
        {
            InitializeComponent();
            mainwindow = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int index = Int32.Parse((sender as Button).Tag.ToString());
            adapter.GetInput(index);
        }

        public void ChangeText(string text)
        {
            textBlock.Text = text;
        }

        public void StartFight() { }

        public void EndFight(FightResult result) { }

        public void ChangeAdapter(IAdapter adapter)
        {
            this.adapter = adapter;
        }

        public void GetInfo(string text, int variantsAmount)
        {

        }
    }
}