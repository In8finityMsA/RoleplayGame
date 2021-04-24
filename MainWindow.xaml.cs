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
        private int numberOfButtons;

        public MainWindow()
        {
            InitializeComponent();
            mainwindow = this;
            adapter = new Stager(this);
            numberOfButtons = AnswerPanel.Children.Count;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            int index = Int32.Parse((e.Source as Button).Tag.ToString());
            adapter.GetInput(index);
        }

        public void ChangeNumberOfButtons(int number)
        {
            while (number > numberOfButtons)
            {
                Button button = new Button();
                button.Style = (Style)TryFindResource("AnswerButton");
                AnswerPanel.Children.Add(button);
                numberOfButtons++;
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
            if (answers.Count == 0)
            {
                (AnswerPanel.Children[0] as Button).Content = "Дальше";
                return;
            }
            
            int i = 0;
            foreach (Button child in AnswerPanel.Children)
            {
                if (i >= answers.Count)
                {
                    return;
                }
                
                child.Content = answers[i];
                i++;
            }
        }

        public void ChangeText(string text)
        {
            MainText.Text = text;
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
            string str = "";
            for (int i = 0; i < enemies.Count - 1; i++)
            {
                //TextBlock t = new TextBlock();
                //t.Margin = new Thickness(i * 50, 0, 0, 0);
                //t.Text = enemies[i].ToString()
                str += enemies[i].ToString() + '\n';
                
            }
            //ChangeText(str);
            enemy(str);
            
        }

        public void enemy(string str)
        {
            Enemy.Text = str;
        }

        public void GetInfoCharacter(Character hero)
        {
            Me.Text = hero.ToString();
        }

        public void InfoAboutCurrentConditions(string text)
        {
            ChangeText(text);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (adapter is Fighter fighter)
                {
                    fighter.GivePrevStep();
                }
            }
        }
    }
}