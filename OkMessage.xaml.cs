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

namespace vLauncher
{
    /// <summary>
    /// Interaktionslogik für OkMessage.xaml
    /// </summary>
    public partial class OkMessage : Window
    {


        public string strType = "Default";

        public OkMessage(string strShortTyp)
        {
            InitializeComponent();
            this.strType = strShortTyp;
            vDecideMessage(strType);
        }


        public void vDecideMessage(string strShortTyp)
        {
            switch (strShortTyp)
            {
                case "DirectoryNotFound":
                    TxtMessage.Text = "Saves-Ordner nicht gefunden.";
                    break;
                case "FileNotFound":
                    TxtMessage.Text = "Keine Datei gefunden.";
                    break;
                case "ButtonNameEmpty":
                    TxtMessage.Text = "Der Buttonname darf nicht leer sein, wenn Sie trotzdem fortfahren wird die Buttoneinstellung gelöscht \n Möchten Sie das Bearbeitungsfenster verlassen?";
                    break;
                case "AppNotFound":
                    TxtMessage.Text = " Anwendung/Datei/Ordner nicht gefunden.";
                    break;
                case "ButtonClose":
                    TxtMessage.Text = "Möchten Sie das Bearbeitungsfenster verlassen?";
                    break;
                case "AlreadyRunning":
                    TxtMessage.Text = "vLauncher läuft bereits!";
                    break;
                default:

                    if (strShortTyp != "Default")
                    {
                        TxtMessage.Text = "Möchten Sie diese Aktion wirklich ausführen?";
                    } else
                    {
                        TxtMessage.Text = "Es ist ein Fehler aufgetreten.";
                    }

                    break;


            }
        }

        public void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
