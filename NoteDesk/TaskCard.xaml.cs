using System.Windows;
using System.Windows.Controls;

namespace NoteDesk
{
    public partial class TaskCard : UserControl
    {
        public TaskCard(string titulo, string prioridad)
        {
            InitializeComponent();


            // =====================================================
            // TÍTULO
            // =====================================================

            TxtTitulo.Text = titulo;


            // =====================================================
            // PRIORIDAD
            // =====================================================
            //
            // Por defecto todas están ocultas desde XAML.
            //
            // Aquí solamente decidimos cuál mostrar.
            // =====================================================

            if (prioridad == "Baja")
            {
                PanelPrioridadBaja.Visibility =
                    Visibility.Visible;
            }
            else if (prioridad == "Media")
            {
                PanelPrioridadMedia.Visibility =
                    Visibility.Visible;
            }
            else if (prioridad == "Alta")
            {
                PanelPrioridadAlta.Visibility =
                    Visibility.Visible;
            }
        }
    }
}