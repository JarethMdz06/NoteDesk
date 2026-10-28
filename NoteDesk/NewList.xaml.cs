using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NoteDesk
{
    public partial class NewList : Window
    {
        // =========================================================
        // TÍTULO DE LA TAREA
        // =========================================================
        //
        // Aquí se guardará el texto escrito en TxtTitulo.
        // MainWindow podrá recuperar este valor cuando
        // se cierre esta ventana.
        // =========================================================

        public string TituloTarea { get; private set; } = "";


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public NewList()
        {
            InitializeComponent();
        }


        // =========================================================
        // BOTÓN CANCELAR
        // =========================================================

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }


        // =========================================================
        // MOSTRAR PANEL DE FECHA
        // =========================================================

        private void ToggleFecha_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelFecha != null)
            {
                PanelFecha.Visibility = Visibility.Visible;
            }
        }


        // =========================================================
        // OCULTAR PANEL DE FECHA
        // =========================================================

        private void ToggleFecha_Unchecked(object sender, RoutedEventArgs e)
        {
            if (PanelFecha != null)
            {
                PanelFecha.Visibility = Visibility.Collapsed;
            }
        }


        // =========================================================
        // PLACEHOLDER DE LOS TEXTBOX
        // CUANDO EL USUARIO ENTRA AL CAMPO
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


        // =========================================================
        // PLACEHOLDER DE LOS TEXTBOX
        // CUANDO EL USUARIO SALE DEL CAMPO
        // =========================================================

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
        // BOTÓN CREAR TAREA
        // =========================================================
        //
        // Por ahora solamente toma el título.
        // =========================================================

        private void CrearTarea_Click(object sender, RoutedEventArgs e)
        {
            // Validamos que el usuario haya escrito un título.
            if (string.IsNullOrWhiteSpace(TxtTitulo.Text) ||
                TxtTitulo.Text == "Ej. Preparar presentación, Comprar pan...")
            {
                MessageBox.Show(
                    "Escribe un título para la tarea.",
                    "NoteDesk",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }


            // Tomamos el texto escrito en TxtTitulo.
            TituloTarea = TxtTitulo.Text.Trim();


            // Indicamos a MainWindow que
            // la tarea sí fue creada.
            DialogResult = true;


            // Cerramos NewList.
            Close();
        }
    }
}