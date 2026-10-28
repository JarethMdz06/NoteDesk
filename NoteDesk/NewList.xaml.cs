using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NoteDesk
{
    public partial class NewList : Window
    {
        // =========================================================
        // DATOS QUE SE ENVIARÁN A MAINWINDOW
        // =========================================================

        public string TituloTarea { get; private set; } = "";

        public string PrioridadSeleccionada { get; private set; } = "";


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public NewList()
        {
            InitializeComponent();
        }


        // =========================================================
        // CANCELAR
        // =========================================================

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }


        // =========================================================
        // FECHA
        // =========================================================

        private void ToggleFecha_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelFecha != null)
            {
                PanelFecha.Visibility = Visibility.Visible;
            }
        }


        private void ToggleFecha_Unchecked(object sender, RoutedEventArgs e)
        {
            if (PanelFecha != null)
            {
                PanelFecha.Visibility = Visibility.Collapsed;
            }
        }


        // =========================================================
        // PLACEHOLDER
        // =========================================================

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == "Ej. Preparar presentación, Comprar pan..." ||
                    textBox.Text == "Agrega detalles adicionales..." ||
                    textBox.Text == "Ej. 25 de Abril, 2026" ||
                    textBox.Text == "Pega un enlace de Google Drive...")
                {
                    textBox.Text = "";

                    textBox.Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#1A1A1A"));
                }
            }
        }


        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (textBox.Name == "TxtTitulo")
                    {
                        textBox.Text =
                            "Ej. Preparar presentación, Comprar pan...";
                    }
                    else if (textBox.Name == "TxtDescripcion")
                    {
                        textBox.Text =
                            "Agrega detalles adicionales...";
                    }
                    else if (textBox.Name == "TxtFecha")
                    {
                        textBox.Text =
                            "Ej. 25 de Abril, 2026";
                    }
                    else if (textBox.Name == "TxtEnlaceDrive")
                    {
                        textBox.Text =
                            "Pega un enlace de Google Drive...";
                    }

                    textBox.Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#888888"));
                }
            }
        }


        // =========================================================
        // CREAR TAREA
        // =========================================================

        private void CrearTarea_Click(object sender, RoutedEventArgs e)
        {
            // =====================================================
            // VALIDAR TÍTULO
            // =====================================================

            if (string.IsNullOrWhiteSpace(TxtTitulo.Text) ||
                TxtTitulo.Text ==
                "Ej. Preparar presentación, Comprar pan...")
            {
                MessageBox.Show(
                    "Escribe un título para la tarea.",
                    "NoteDesk",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }


            // =====================================================
            // GUARDAR TÍTULO
            // =====================================================

            TituloTarea =
                TxtTitulo.Text.Trim();


            // =====================================================
            // PRIORIDAD
            // =====================================================
            //
            // Si el switch está apagado:
            //
            // PrioridadSeleccionada = ""
            //
            // por lo tanto TaskCard no mostrará ninguna.
            // =====================================================

            PrioridadSeleccionada = "";


            if (SwitchPrioridad.IsChecked == true)
            {
                if (PrioridadBaja.IsChecked == true)
                {
                    PrioridadSeleccionada = "Baja";
                }
                else if (PrioridadMedia.IsChecked == true)
                {
                    PrioridadSeleccionada = "Media";
                }
                else if (PrioridadAlta.IsChecked == true)
                {
                    PrioridadSeleccionada = "Alta";
                }
            }


            // =====================================================
            // TERMINAR
            // =====================================================

            DialogResult = true;

            Close();
        }
    }
}