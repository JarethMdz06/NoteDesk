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

        // Muestra el panel de fechas cuando el interruptor se enciende (verde)
        private void ToggleFecha_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelFechas != null)
            {
                PanelFechas.Visibility = Visibility.Visible;
            }
        }

        // Oculta el panel de fechas cuando el interruptor se apaga (gris)
        private void ToggleFecha_Unchecked(object sender, RoutedEventArgs e)
        {
            if (PanelFechas != null)
            {
                PanelFechas.Visibility = Visibility.Collapsed;
            }
        }

        // Lógica para el marcador de posición (Placeholder) en los TextBox
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == "Ej. Preparar presentación, Comprar pan..." ||
                    textBox.Text == "Agrega detalles adicionales...")
                {
                    textBox.Text = "";
                    textBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1A"));
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (textBox.Name == "TxtTituloTarea")
                    {
                        textBox.Text = "Ej. Preparar presentación, Comprar pan...";
                    }
                    else if (textBox.Name == "TxtDescripcion")
                    {
                        textBox.Text = "Agrega detalles adicionales...";
                    }

                    textBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888"));
                }
            }
        }
    }
}