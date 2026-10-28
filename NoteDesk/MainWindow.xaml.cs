using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoteDesk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnAddTask_Click(object sender, RoutedEventArgs e)
        {
            // Creamos NewList
            NewList ventanaNuevaTarea = new NewList();


            // MainWindow es la ventana propietaria
            ventanaNuevaTarea.Owner = this;


            // Abrimos NewList
            bool? resultado = ventanaNuevaTarea.ShowDialog();


            // Si se presionó Crear Tarea
            if (resultado == true)
            {
                // Recuperamos el título
                string titulo = ventanaNuevaTarea.TituloTarea;


                // Creamos la tarjeta visual
                TaskCard nuevaTarea = new TaskCard(titulo);


                // Agregamos la tarjeta a la lista
                ListTask.Children.Add(nuevaTarea);
            }
        }
    }
}

