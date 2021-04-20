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
using KASHGAMEWPF;
using KashTask;

namespace KashTaskWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IAdapter adapter;

        public static MainWindow mainwindow;
        private const int Number_Of_Buttons = 5;

        public MainWindow()
        {
            InitializeComponent();
            mainwindow = this;
            adapter = new Stager(this);         
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            int index = Int32.Parse((e.Source as Button).Tag.ToString());
            adapter.GetInput(index);
        }

        public void ChangeNumberOfButtons(int number)
        {
            if (number > Number_Of_Buttons)
            {
                throw new ArgumentException("Too much buttons were requested. Do smth with that!");
            }
            foreach (UIElement child in grid.Children)
            {
                if (number > 0)
                {
                    number--;
                    child.Visibility = Visibility.Visible;
                }
                else
                {
                    child.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void ChangeButtonsText(List<string> answers)
        {
            int i = 0;
            foreach (Button child in grid.Children)
            {
                if (i < answers.Count)
                {
                    child.Content = answers[i];
                    i++;
                }
            }
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

        public void GetInfo(List<string> answers, int variantsAmount)
        {
            ChangeButtonsText(answers);
            ChangeNumberOfButtons(variantsAmount);
        }
    }
}