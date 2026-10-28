using System.Windows;
using System.Windows.Controls;

namespace NoteDesk
{
    public partial class TaskCard : UserControl
    {
        public TaskCard(
            string titulo,
            string prioridad,
            string categoria)
        {
            InitializeComponent();


            // =====================================================
            // TÍTULO
            // =====================================================

            TxtTitulo.Text = titulo;


            // =====================================================
            // PRIORIDAD
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


            // =====================================================
            // CATEGORÍA
            // =====================================================
            //
            // Si categoria viene vacía:
            //
            // PanelCategoria continúa Collapsed.
            //
            // Si viene una categoría:
            //
            // mostramos el panel y colocamos su icono.
            // =====================================================

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                PanelCategoria.Visibility =
                    Visibility.Visible;


                if (categoria == "Work")
                {
                    TxtIconoCategoria.Text = "💼";
                }
                else if (categoria == "School")
                {
                    TxtIconoCategoria.Text = "🎓";
                }
                else if (categoria == "Home")
                {
                    TxtIconoCategoria.Text = "🏠";
                }
                else if (categoria == "Shopping")
                {
                    TxtIconoCategoria.Text = "🛒";
                }
                else if (categoria == "Personal")
                {
                    TxtIconoCategoria.Text = "👤";
                }
                else if (categoria == "Hobbies")
                {
                    TxtIconoCategoria.Text = "🎮";
                }
                else if (categoria == "Health")
                {
                    TxtIconoCategoria.Text = "❤️";
                }
                else if (categoria == "Other")
                {
                    TxtIconoCategoria.Text = "•••";
                }
            }
        }
    }
}