using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NoteDesk
{
    public partial class TaskCard : UserControl
    {
        private readonly TareaModel _tarea;
        private bool _inicializando = true;
        private string _titulo;
        private string _descripcion;
        private string _fecha;
        private string _color;
        private string _prioridad;
        private string _categoria;

        public event Action<TareaModel>? EditRequested;
        public event Action<TareaModel>? DeleteRequested;
        public event Action<TareaModel, bool>? CompletionChanged;
        public event Action<TareaModel>? SelectionRequested;
        public event Func<TareaModel, bool>? SubtareasChanged;

        public TareaModel TareaAsociada => _tarea;

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
            ChkCompletada.IsChecked = tarea.Completada;
            ChkCompletada.IsEnabled = true;
            ChkCompletada.Cursor = Cursors.Hand;
            ChkCompletada.ToolTip = "Marcar o desmarcar la tarea completa";
            TxtTitulo.TextDecorations = tarea.Completada
                ? TextDecorations.Strikethrough
                : null;
            TxtTitulo.Opacity = tarea.Completada ? 0.65 : 1;

            string titulo = tarea.Titulo;
            string prioridad = tarea.Prioridad;
            string categoria = tarea.Categoria;

            TxtTitulo.Text = titulo;

            if (prioridad == "Baja")
            {
                PanelPrioridadBaja.Visibility = Visibility.Visible;
            }
            else if (prioridad == "Media")
            {
                PanelPrioridadMedia.Visibility = Visibility.Visible;
            }
            else if (prioridad == "Alta")
            {
                PanelPrioridadAlta.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                PanelCategoria.Visibility = Visibility.Visible;

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
            _inicializando = false;
        }

        public void EstablecerSeleccionada(bool seleccionada)
        {
            BordeTaskCard.BorderBrush = seleccionada
                ? (System.Windows.Media.Brush)FindResource("ColorBordeMesSeleccionado")
                : System.Windows.Media.Brushes.Transparent;
            BordeTaskCard.BorderThickness = seleccionada
                ? new Thickness(2)
                : new Thickness(1);
        }

        private void ChkCompletada_Changed(object sender, RoutedEventArgs e)
        {
            if (_inicializando)
            {
                return;
            }

            e.Handled = true;
            bool completada = ChkCompletada.IsChecked == true;
            SelectionRequested?.Invoke(_tarea);

            if (completada == _tarea.Completada)
            {
                return;
            }

            // MainWindow guarda la tarea y sus subtareas juntas.
            CompletionChanged?.Invoke(_tarea, completada);
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

        // Clic en la tarjeta: únicamente selecciona la tarea y actualiza Progreso.
        private void TaskCard_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source &&
                IsWithinInteractiveControl(source))
            {
                return;
            }

            e.Handled = true;
            SelectionRequested?.Invoke(_tarea);
        }

        // El botón de nota es la única acción que abre la ventana flotante.
        private void BtnAbrirNota_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            AbrirNotaFlotante();
        }

        private void AbrirNotaFlotante()
        {
            StickyTaskWindow? ventanaExistente =
                Application.Current.Windows
                    .OfType<StickyTaskWindow>()
                    .FirstOrDefault(
                        ventana =>
                            ReferenceEquals(
                                ventana.TareaAsociada,
                                _tarea
                            )
                    );

            if (ventanaExistente != null)
            {
                if (ventanaExistente.WindowState == WindowState.Minimized)
                {
                    ventanaExistente.WindowState = WindowState.Normal;
                }

                ventanaExistente.Activate();
                return;
            }

            StickyTaskWindow ventana =
                new StickyTaskWindow(
                    _tarea
                );
            ventana.SubtareasChanged += tarea => SubtareasChanged?.Invoke(tarea) == true;

            Window? ventanaPrincipal =
                Window.GetWindow(this);

            if (ventanaPrincipal != null)
            {
                int cantidadVentanas =
                    Application.Current.Windows
                        .OfType<StickyTaskWindow>()
                        .Count();

                int desplazamiento =
                    cantidadVentanas % 6;

                ventana.WindowStartupLocation =
                    WindowStartupLocation.Manual;

                ventana.Left =
                    ventanaPrincipal.Left
                    +
                    Math.Max(
                        20,
                        (
                            ventanaPrincipal.ActualWidth
                            -
                            ventana.Width
                        )
                        /
                        2
                    )
                    +
                    (
                        desplazamiento
                        *
                        28
                    );

                ventana.Top =
                    ventanaPrincipal.Top
                    +
                    Math.Max(
                        20,
                        (
                            ventanaPrincipal.ActualHeight
                            -
                            ventana.Height
                        )
                        /
                        2
                    )
                    +
                    (
                        desplazamiento
                        *
                        28
                    );
            }

            ventana.Show();
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
