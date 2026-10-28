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


        // =========================================================
        // BOTÓN AGREGAR TAREA
        // =========================================================

        private void BtnAddTask_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Creamos NewList.
            NewList ventanaNuevaTarea =
                new NewList();


            // MainWindow es la ventana propietaria.
            ventanaNuevaTarea.Owner =
                this;


            // Abrimos NewList.
            bool? resultado =
                ventanaNuevaTarea.ShowDialog();


            
            // SI SE CREÓ LA TAREA

            if (resultado == true)
            {
                // Recuperamos título.
                string titulo =
                    ventanaNuevaTarea.TituloTarea;


                // Recuperamos prioridad.
                //
                // Puede ser:
                //
                // ""
                // "Baja"
                // "Media"
                // "Alta"
                //
                string prioridad =
                    ventanaNuevaTarea.PrioridadSeleccionada;


                // Creamos la tarjeta.
                TaskCard nuevaTarea =
                    new TaskCard(
                        titulo,
                        prioridad);


                // La agregamos al MainWindow.
                ListTask.Children.Add(
                    nuevaTarea);
            }
        }
    }
}