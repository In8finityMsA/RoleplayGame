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
            numberOfButtons = AnswerPanel.Children.Count;
            adapter = new Stager(this);

            EnemyTextBorder.Visibility = Visibility.Collapsed;
            HeroTextBorder.Visibility = Visibility.Collapsed;
            EnemyLabel.Visibility = Visibility.Collapsed;
            HeroLabel.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int index = Int32.Parse((e.Source as Button).Tag.ToString());
            adapter.GetInput(index);
        }

        public string GetUserInputText()
        {
            return TextBoxUserInput.Text;
        }

        public void DisplayTextBox()
        {
            TextBoxUserInput.Text = "";
            TextBoxUserInput.Visibility = Visibility.Visible;
        }

        public void HideTextBox()
        {
            TextBoxUserInput.Text = "";
            TextBoxUserInput.Visibility = Visibility.Collapsed;
        }
        
        public void GetInfo(List<string> answers, int variantsAmount)
        {
            ChangeNumberOfButtons(variantsAmount);
            ChangeButtonsText(answers);
        }

        public void ChangeNumberOfButtons(int number) //Does it need to delete unused buttons?
        {
            while (number > numberOfButtons)
            {
                Button button = new Button();
                button.Style = (Style)TryFindResource("AnswerButton");
                button.Tag = numberOfButtons.ToString();
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
        
        public void StartFight()
        {
            EnemyTextBorder.Visibility = Visibility.Visible;
            HeroTextBorder.Visibility = Visibility.Visible;
            EnemyLabel.Visibility = Visibility.Visible;
            HeroLabel.Visibility = Visibility.Visible;
        }
        
        public void EndFight(FightResult result)
        {
            EnemyLabel.Visibility = Visibility.Collapsed;
            HeroLabel.Visibility = Visibility.Collapsed;
            EnemyTextBorder.Visibility = Visibility.Collapsed;
            HeroTextBorder.Visibility = Visibility.Collapsed;
            EnemyText.Text = "";
            HeroText.Text = "";
        }

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

        public void GetInfoEnemies(List<Character> enemies)
        {
            string str = "";
            for (int i = 0; i < enemies.Count - 1; i++)
            {
                str += enemies[i].ToString() + '\n';
                
            }
            EnemyText.Text = str;
            
        }
        
        public void GetInfoCharacter(Character hero)
        {
            HeroText.Text = hero.ToString();
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