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

        // 1. Método para poder arrastrar y mover la ventana flotante por la pantalla
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}