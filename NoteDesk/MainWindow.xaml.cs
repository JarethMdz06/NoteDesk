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


            // =====================================================
            // SI SE CREÓ LA TAREA
            // =====================================================

            if (resultado == true)
            {
                // =================================================
                // TÍTULO
                // =================================================

                string titulo =
                    ventanaNuevaTarea.TituloTarea;


                // =================================================
                // DESCRIPCIÓN
                // =================================================

                string descripcion =
                    ventanaNuevaTarea.DescripcionTarea;


                // =================================================
                // FECHA
                // =================================================

                string fecha =
                    ventanaNuevaTarea.FechaTarea;


                // =================================================
                // COLOR
                // =================================================

                string color =
                    ventanaNuevaTarea.ColorTarea;


                // =================================================
                // PRIORIDAD
                // =================================================

                string prioridad =
                    ventanaNuevaTarea.PrioridadSeleccionada;


                // =================================================
                // CATEGORÍA
                // =================================================

                string categoria =
                    ventanaNuevaTarea.CategoriaSeleccionada;


                // =================================================
                // CREAMOS TASKCARD
                // =================================================
                //
                // Ahora enviamos:
                //
                // 1. Título
                // 2. Descripción
                // 3. Fecha
                // 4. Color
                // 5. Prioridad
                // 6. Categoría
                //
                // =================================================

                TaskCard nuevaTarea =
                    new TaskCard(
                        titulo,
                        descripcion,
                        fecha,
                        color,
                        prioridad,
                        categoria
                    );


                // =================================================
                // AGREGAMOS TASKCARD A MAINWINDOW
                // =================================================

                ListTask.Children.Add(
                    nuevaTarea);
            }
        }


        // =========================================================
        // BOTÓN AJUSTES (Barra Inferior)
        // =========================================================

        private void Ajustes_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            // Creamos y abrimos la ventana de ajustes
            ajustes ventanaAjustes =
                new ajustes();

            ventanaAjustes.ShowDialog();
        }


        // =========================================================
        // BOTÓN CALENDARIO (Barra Inferior)
        // =========================================================

        private void Calendario_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            // Creamos y abrimos la ventana de calendario
            Calendario ventanaCalendario =
                new Calendario();

            ventanaCalendario.ShowDialog();
        }
    }
}
