using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NoteDesk
{
    public partial class TaskCard : UserControl
    {
        
        // DATOS DE LA TAREA
       

        private readonly TareaModel _tarea;
        private string _titulo;
        private string _descripcion;
        private string _fecha;
        private string _color;
        private string _prioridad;
        private string _categoria;

        public event Action<TareaModel>? EditRequested;
        public event Action<TareaModel>? DeleteRequested;


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public TaskCard(TareaModel tarea)
        {
            InitializeComponent();

            _tarea = tarea;
            _titulo = tarea.Titulo;
            _descripcion = tarea.Descripcion;
            _fecha = tarea.Fecha;
            _color = tarea.Color;
            _prioridad = tarea.Prioridad;
            _categoria = tarea.Categoria;

            string titulo = tarea.Titulo;
            string prioridad = tarea.Prioridad;
            string categoria = tarea.Categoria;


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


        private void BtnMenuTarea_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu is ContextMenu menu)
            {
                menu.PlacementTarget = button;
                menu.IsOpen = true;
            }

            e.Handled = true;
        }

        private void EditarTarea_Click(object sender, RoutedEventArgs e)
        {
            EditRequested?.Invoke(_tarea);
            e.Handled = true;
        }

        private void EliminarTarea_Click(object sender, RoutedEventArgs e)
        {
            DeleteRequested?.Invoke(_tarea);
            e.Handled = true;
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
            if (e.OriginalSource is DependencyObject source &&
                IsWithinInteractiveControl(source))
            {
                return;
            }

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

        private static bool IsWithinInteractiveControl(DependencyObject source)
        {
            DependencyObject? current = source;

            while (current != null)
            {
                if (current is Button || current is CheckBox)
                {
                    return true;
                }

                current = current is System.Windows.Media.Visual ||
                          current is System.Windows.Media.Media3D.Visual3D
                    ? System.Windows.Media.VisualTreeHelper.GetParent(current)
                    : LogicalTreeHelper.GetParent(current);
            }

            return false;
        }
    }
}
