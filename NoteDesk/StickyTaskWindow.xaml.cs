using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NoteDesk
{
    public partial class StickyTaskWindow : Window
    {
        // ============================================================
        // DATOS DE LA TAREA
        // ============================================================

        private string _titulo = "";
        private string _descripcion = "";
        private string _fecha = "";
        private string _categoria = "";
        private string _prioridad = "";


        // ============================================================
        // ESTADO DE LA VENTANA
        // ============================================================

        private bool ventanaReducida = false;

        private bool coloresVisibles = false;


        // ============================================================
        // TAMAÑOS DE LA VENTANA
        // ============================================================

        private const double AlturaNormal = 330;
        private const double AlturaConColores = 385;
        private const double AlturaReducida = 90;

        private const double AnchoNormal = 330;
        private const double AnchoReducido = 275;


        // ============================================================
        // CONSTRUCTOR NORMAL
        // ============================================================

        public StickyTaskWindow()
        {
            InitializeComponent();

            Topmost = true;
        }


        // ============================================================
        // CONSTRUCTOR CON DATOS DE LA TAREA
        // ============================================================
        //
        // Recibe:
        //
        // - título
        // - descripción
        // - fecha
        // - categoría
        // - prioridad
        //
        // El color de NewList NO se aplica aquí.
        // Esta ventana inicia siempre con el color
        // definido en StickyTaskWindow.xaml:
        //
        // #FFF3B8
        //
        // ============================================================

        public StickyTaskWindow(
            string titulo,
            string descripcion,
            string fecha,
            string categoria,
            string prioridad)
        {
            InitializeComponent();

            Topmost = true;


            // =====================================================
            // GUARDAR DATOS
            // =====================================================

            _titulo = titulo;
            _descripcion = descripcion;
            _fecha = fecha;
            _categoria = categoria;
            _prioridad = prioridad;


            // =====================================================
            // MOSTRAR TÍTULO
            // =====================================================

            TxtTituloSticky.Text = _titulo;


            // =====================================================
            // MOSTRAR DESCRIPCIÓN
            // =====================================================

            TxtDescripcionSticky.Text = _descripcion;


            // =====================================================
            // MOSTRAR FECHA
            // =====================================================

            TxtFecha.Text = _fecha;


            // =====================================================
            // MOSTRAR CATEGORÍA
            // =====================================================

            TxtCategoriaSticky.Text = _categoria;


            // =====================================================
            // PRIORIDAD
            // =====================================================
            //
            // Por el momento solamente se guarda.
            //
            // =====================================================
        }


        // ============================================================
        // MOVER LA VENTANA POR LA PANTALLA
        // ============================================================

        private void StickyContainer_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();
                }
                catch
                {
                    // Evita errores si el clic proviene
                    // de un control interno
                }
            }
        }


        // ============================================================
        // CERRAR
        // ============================================================

        private void BtnCerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }


        // ============================================================
        // REDUCIR / EXPANDIR
        // ============================================================

        private void BtnReducir_Click(
            object sender,
            RoutedEventArgs e)
        {
            double posicionSuperior = Top;

            if (!ventanaReducida)
            {
                PanelContenido.Visibility =
                    Visibility.Collapsed;

                PanelColores.Visibility =
                    Visibility.Collapsed;

                coloresVisibles = false;

                TxtFlechaColor.Text = "⌄";

                if (BtnMostrarColores.Parent is Grid gridColor &&
                    gridColor.Parent is StackPanel seccionColor)
                {
                    seccionColor.Visibility =
                        Visibility.Collapsed;
                }

                Height = AlturaReducida;
                Width = AnchoReducido;

                BtnReducir.Content = "+";

                ventanaReducida = true;
            }
            else
            {
                PanelContenido.Visibility =
                    Visibility.Visible;

                if (BtnMostrarColores.Parent is Grid gridColor &&
                    gridColor.Parent is StackPanel seccionColor)
                {
                    seccionColor.Visibility =
                        Visibility.Visible;
                }

                Height = AlturaNormal;
                Width = AnchoNormal;

                BtnReducir.Content = "−";

                ventanaReducida = false;
            }

            Top = posicionSuperior;
        }


        // ============================================================
        // MOSTRAR / OCULTAR COLORES
        // ============================================================

        private void BtnMostrarColores_Click(
            object sender,
            RoutedEventArgs e)
        {
            double posicionSuperior = Top;

            if (!coloresVisibles)
            {
                PanelColores.Visibility =
                    Visibility.Visible;

                TxtFlechaColor.Text = "⌃";

                Height = AlturaConColores;

                coloresVisibles = true;
            }
            else
            {
                PanelColores.Visibility =
                    Visibility.Collapsed;

                TxtFlechaColor.Text = "⌄";

                Height = AlturaNormal;

                coloresVisibles = false;
            }

            Top = posicionSuperior;
        }


        // ============================================================
        // CAMBIAR COLOR DE LA NOTA
        // ============================================================
        //
        // Este color pertenece solamente al StickyTaskWindow.
        //
        // El usuario puede cambiarlo usando los colores
        // definidos dentro del propio StickyTaskWindow.xaml.
        //
        // ============================================================

        private void Color_Checked(
            object sender,
            RoutedEventArgs e)
        {
            if (StickyContainer == null)
            {
                return;
            }


            if (sender is RadioButton radioButton &&
                radioButton.Background is SolidColorBrush color)
            {
                // Cambiar fondo de la nota

                StickyContainer.Background =
                    new SolidColorBrush(
                        color.Color);


                // Cambiar borde utilizando el mismo color

                StickyContainer.BorderBrush =
                    new SolidColorBrush(
                        Color.FromArgb(
                            140,
                            color.Color.R,
                            color.Color.G,
                            color.Color.B
                        )
                    );
            }
        }
    }
}
