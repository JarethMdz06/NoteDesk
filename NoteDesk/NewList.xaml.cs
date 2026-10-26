using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NoteDesk
{
    public partial class NewList : Window
    {
        public NewList()
        {
            InitializeComponent();
        }

        // Cierra la ventana al hacer clic en cancelar
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Muestra el panel de fecha cuando el switch está activado
        private void ToggleFecha_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelFecha != null)
            {
                PanelFecha.Visibility = Visibility.Visible;
            }
        }

        // Oculta el panel de fecha cuando el switch está desactivado
        private void ToggleFecha_Unchecked(object sender, RoutedEventArgs e)
        {
            if (PanelFecha != null)
            {
                PanelFecha.Visibility = Visibility.Collapsed;
            }
        }

        // Placeholder de los TextBox
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
    }
}