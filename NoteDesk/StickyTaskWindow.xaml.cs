using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NoteDesk.Services;

namespace NoteDesk
{
    public partial class StickyTaskWindow : Window
    {
        // ============================================================
        // TAREA ASOCIADA
        // ============================================================

        private readonly TareaModel? _tarea;


        // Esta propiedad permite que TaskCard sepa si la ventana
        // de una tarea específica ya está abierta.

        public TareaModel? TareaAsociada =>
            _tarea;


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

            PanelDrive.Visibility =
                Visibility.Collapsed;
        }


        // ============================================================
        // CONSTRUCTOR CON LA TAREA COMPLETA
        // ============================================================
        //
        // Ahora recibimos TareaModel completo.
        //
        // Esto permite conocer también:
        //
        // - DriveActivo
        // - DriveFolderId
        //
        // El color de NewList NO se aplica aquí.
        // Esta ventana continúa iniciando siempre con el color
        // definido en StickyTaskWindow.xaml:
        //
        // #FFF3B8
        //
        // ============================================================

        public StickyTaskWindow(
            TareaModel tarea)
        {
            InitializeComponent();

            Topmost = true;


            // =====================================================
            // GUARDAR LA TAREA COMPLETA
            // =====================================================

            _tarea = tarea;


            // =====================================================
            // GUARDAR DATOS
            // =====================================================

            _titulo =
                tarea.Titulo;

            _descripcion =
                tarea.Descripcion;

            _fecha =
                tarea.Fecha;

            _categoria =
                tarea.Categoria;

            _prioridad =
                tarea.Prioridad;


            // =====================================================
            // MOSTRAR TÍTULO
            // =====================================================

            TxtTituloSticky.Text =
                _titulo;


            // =====================================================
            // MOSTRAR DESCRIPCIÓN
            // =====================================================

            TxtDescripcionSticky.Text =
                _descripcion;


            // =====================================================
            // MOSTRAR FECHA
            // =====================================================

            TxtFecha.Text =
                _fecha;


            // =====================================================
            // MOSTRAR CATEGORÍA
            // =====================================================

            TxtCategoriaSticky.Text =
                _categoria;


            // =====================================================
            // GOOGLE DRIVE
            // =====================================================

            ConfigurarGoogleDrive();


            // =====================================================
            // PRIORIDAD
            // =====================================================
            //
            // Por el momento solamente se guarda.
            //
            // =====================================================
        }


        // ============================================================
        // CONFIGURAR GOOGLE DRIVE
        // ============================================================

        private void ConfigurarGoogleDrive()
        {
            if (_tarea == null)
            {
                PanelDrive.Visibility =
                    Visibility.Collapsed;

                return;
            }


            if (_tarea.DriveActivo &&
                !string.IsNullOrWhiteSpace(
                    _tarea.DriveFolderId))
            {
                PanelDrive.Visibility =
                    Visibility.Visible;


                TxtDrive.Text =
                    "Abrir carpeta en Google Drive";
            }
            else
            {
                // Si esta tarea no tiene Drive activado,
                // no mostramos un enlace que no funcionaría.

                PanelDrive.Visibility =
                    Visibility.Collapsed;
            }
        }


        // ============================================================
        // ABRIR CARPETA DE GOOGLE DRIVE DE ESTA TAREA
        // ============================================================

        private void PanelDrive_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            if (_tarea == null ||
                string.IsNullOrWhiteSpace(
                    _tarea.DriveFolderId))
            {
                MessageBox.Show(
                    "Esta tarea no tiene una carpeta de Google Drive asociada.",
                    "Google Drive",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }


            string enlaceCarpeta =
                GoogleDriveService
                    .ObtenerEnlaceCarpetaTarea(
                        _tarea.DriveFolderId
                    );


            if (string.IsNullOrWhiteSpace(
                    enlaceCarpeta))
            {
                MessageBox.Show(
                    "No fue posible obtener el enlace de la carpeta de Google Drive.",
                    "Google Drive",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }


            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName =
                            enlaceCarpeta,

                        UseShellExecute =
                            true
                    }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible abrir la carpeta de Google Drive.\n\n" +
                    ex.Message,
                    "Google Drive",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }


            e.Handled =
                true;
        }


        // ============================================================
        // MOVER LA VENTANA POR LA PANTALLA
        // ============================================================

        private void StickyContainer_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            // No iniciamos DragMove si el usuario está haciendo
            // clic sobre un control interactivo.

            if (e.OriginalSource is DependencyObject source &&
                IsWithinInteractiveControl(source))
            {
                return;
            }


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
        // DETECTAR CONTROLES INTERACTIVOS
        // ============================================================

        private bool IsWithinInteractiveControl(
            DependencyObject source)
        {
            DependencyObject? current =
                source;


            while (current != null)
            {
                if (current is Button ||
                    current is RadioButton)
                {
                    return true;
                }


                if (ReferenceEquals(
                        current,
                        PanelDrive))
                {
                    return true;
                }


                current =
                    current
                        is System.Windows.Media.Visual
                        ||
                    current
                        is System.Windows.Media.Media3D.Visual3D
                    ?
                    System.Windows.Media
                        .VisualTreeHelper
                        .GetParent(
                            current
                        )
                    :
                    LogicalTreeHelper
                        .GetParent(
                            current
                        );
            }


            return false;
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
