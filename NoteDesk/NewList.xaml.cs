using System;
using System.Windows;
using System.Windows.Input;

namespace NoteDesk
{
    public partial class NewList : Window
    {
        public NewList()
        {
            InitializeComponent();
        }

        // Permite mover la ventana flotante sin bordes haciendo clic en cualquier parte de ella
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
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
    }
}