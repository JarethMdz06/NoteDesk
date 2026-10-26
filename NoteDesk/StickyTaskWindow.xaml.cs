using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NoteDesk
{
    public partial class StickyTaskWindow : Window
    {
        // Indica si la ventana está en modo reducido
        private bool ventanaReducida = false;

        // Indica si los colores están desplegados
        private bool coloresVisibles = false;

        // Tamaños de la ventana
        private const double AlturaNormal = 330;
        private const double AlturaConColores = 385;
        private const double AlturaReducida = 90;

        private const double AnchoNormal = 330;
        private const double AnchoReducido = 275;

        public StickyTaskWindow()
        {
            InitializeComponent();

            // Mantiene la nota por encima de otras ventanas
            Topmost = true;
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
                    // Evita errores si el clic proviene de un control interno
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
            // Conservamos la coordenada superior
            double posicionSuperior = Top;

            if (!ventanaReducida)
            {
                // Ocultar contenido principal
                PanelContenido.Visibility = Visibility.Collapsed;

                // Ocultar selector de colores
                PanelColores.Visibility = Visibility.Collapsed;

                coloresVisibles = false;

                TxtFlechaColor.Text = "⌄";

                // Ocultar toda la sección de color
                // al reducir la ventana
                if (BtnMostrarColores.Parent is Grid gridColor &&
                    gridColor.Parent is StackPanel seccionColor)
                {
                    seccionColor.Visibility = Visibility.Collapsed;
                }

                // Tamaño reducido
                Height = AlturaReducida;
                Width = AnchoReducido;

                // Mostrar símbolo para restaurar
                BtnReducir.Content = "+";

                ventanaReducida = true;
            }
            else
            {
                // Recuperar contenido
                PanelContenido.Visibility = Visibility.Visible;

                // Recuperar sección de color
                if (BtnMostrarColores.Parent is Grid gridColor &&
                    gridColor.Parent is StackPanel seccionColor)
                {
                    seccionColor.Visibility = Visibility.Visible;
                }

                Height = AlturaNormal;
                Width = AnchoNormal;

                BtnReducir.Content = "−";

                ventanaReducida = false;
            }

            // Mantener fija la parte superior.
            // Así la ventana crece hacia abajo.
            Top = posicionSuperior;
        }

        // ============================================================
        // MOSTRAR / OCULTAR COLORES
        // ============================================================

        private void BtnMostrarColores_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Guardamos la posición superior antes de cambiar altura
            double posicionSuperior = Top;

            if (!coloresVisibles)
            {
                // Mostrar colores
                PanelColores.Visibility = Visibility.Visible;

                // Flecha hacia arriba
                TxtFlechaColor.Text = "⌃";

                // Aumentar solamente hacia abajo
                Height = AlturaConColores;

                coloresVisibles = true;
            }
            else
            {
                // Ocultar colores
                PanelColores.Visibility = Visibility.Collapsed;

                // Flecha hacia abajo
                TxtFlechaColor.Text = "⌄";

                // Regresar al tamaño normal
                Height = AlturaNormal;

                coloresVisibles = false;
            }

            // Mantiene fija la parte superior de la ventana
            Top = posicionSuperior;
        }

        // ============================================================
        // CAMBIAR COLOR DE LA NOTA
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
                StickyContainer.Background =
                    new SolidColorBrush(color.Color);

                // También ajustamos ligeramente el borde
                // para que combine con cualquier pastel.
                StickyContainer.BorderBrush =
                    new SolidColorBrush(Color.FromArgb(
                        100,
                        color.Color.R,
                        color.Color.G,
                        color.Color.B));
            }
        }
    }
}