using KashTaskWPF.Adapters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using game;

namespace KashTaskWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IAdapter adapter;

        public static MainWindow mainwindow;
        private const int Max_Number_Of_Buttons = 5;

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
            if (number > Max_Number_Of_Buttons)
            {
                throw new ArgumentException("Too much buttons were requested. Do smth with that!");
            }
            foreach (UIElement child in AnswerPanel.Children)
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
            foreach (Button child in AnswerPanel.Children)
            {
                if (i >= answers.Count)
                {
                    break;
                }
                
                child.Content = answers[i];
                i++;
            }
        }

        public void ChangeText(string text)
        {
            textBlock.Text = text;
        }

        public void StartFight() { }

        public void EndFight(FightResult result) { }

        public void ChangeAdapter([NotNull] IAdapter adapter)
        {
            if (adapter != null)
            {
                this.adapter = adapter;
            }
            else
            {
                throw new NullReferenceException("Adapter can't be null. You've lost link between ui and logic.");
            }
        }

        public void GetInfo(List<string> answers, int variantsAmount)
        {
            ChangeButtonsText(answers);
            ChangeNumberOfButtons(variantsAmount);
        }

        public void GetInfoEnemies(List<Character> enemies)
        {
            
        }

        public void GetInfoCharacter(Character hero)
        {
            ChangeText(hero.Name + " " + hero.Health + " " + hero.MaxHealth);
        }
    }
}