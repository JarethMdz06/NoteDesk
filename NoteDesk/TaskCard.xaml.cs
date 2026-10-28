using System.Windows.Controls;

namespace NoteDesk
{
    public partial class TaskCard : UserControl
    {
        public TaskCard(string titulo)
        {
            InitializeComponent();

            TxtTitulo.Text = titulo;
        }
    }
}