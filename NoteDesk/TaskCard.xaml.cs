using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NoteDesk
{
    public partial class TaskCard : UserControl
    {
        
        // DATOS DE LA TAREA
       

        private string _titulo;
        private string _descripcion;
        private string _fecha;
        private string _color;
        private string _prioridad;
        private string _categoria;


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public TaskCard(
            string titulo,
            string descripcion,
            string fecha,
            string color,
            string prioridad,
            string categoria)
        {
            InitializeComponent();


            // =====================================================
            // GUARDAMOS LOS DATOS DE LA TAREA
            // =====================================================

            _titulo = titulo;
            _descripcion = descripcion;
            _fecha = fecha;
            _color = color;
            _prioridad = prioridad;
            _categoria = categoria;


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


        // =====================================================
        // CLICK EN LA TARJETA
        // =====================================================
        //
        // Al presionar la tarjeta enviamos:
        //
        // - Título
        // - Descripción
        // - Fecha
        // - Color
        // - Categoría
        // - Prioridad
        //
        // hacia StickyTaskWindow.
        // =====================================================

        private void TaskCard_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            StickyTaskWindow ventana =
                new StickyTaskWindow(
                    _titulo,
                    _descripcion,
                    _fecha,
                    //_color,
                    _categoria,
                    _prioridad
                );

            ventana.ShowDialog();
        }
    }
}
