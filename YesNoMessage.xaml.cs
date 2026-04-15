using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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

    public partial class YesNoMessage : Window
    {

        public string strType = "Default";

        public YesNoMessage(string strShortTyp)
        {
            InitializeComponent();
            this.strType = strShortTyp;
            vDecideMessage(strType);
        }

        //            this.DialogResult = true;

        public void vDecideMessage(string strShortTyp)
        {
            switch(strShortTyp) {
                case "Default":
                    TxtMessage.Text = "Bist du sicher, dass du diese Aktion ausfürhen willst?";
                    break;
                case "Reset1":
                    TxtMessage.Text = "Bist du sicher, dass du die Einstellungen zurücksetzen willst? Alle Buttons und überschriften werden zurückgesetzt. | 1. Mal";
                    break;
                case "Reset2":
                    TxtMessage.Text = "Bist du sicher, dass du die Einstellungen zurücksetzen willst? Alle Buttons und überschriften werden zurückgesetzt. | 2. Mal";
                    break;
                case "Restart":
                    TxtMessage.Text = "Die App muss neu gestartet werden, damit die Änderungen wirksam werden. Möchten Sie die App jetzt neu starten?";
                    break;
                case "ButtonClose":
                    TxtMessage.Text = "Wollen Sie wirklich dieses Fenster schließen? Alle nicht gespeicherten Änderungen gehen verloren!";
                    break;
                case "ButtonNameEmpty":
                    TxtMessage.Text = "Der Buttonname darf nicht leer sein, wenn Sie trotzdem fortfahren wird die Buttoneinstellung gelöscht \n Möchten Sie das Bearbeitungsfenster verlassen?";
                    break;
                case "ButtonChange":
                    TxtMessage.Text = "Möchten Sie den Button wirklich ändern?";
                    break;


            }
        }

        public void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        public void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
