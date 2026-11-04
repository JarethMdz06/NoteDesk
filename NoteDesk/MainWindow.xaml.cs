using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NoteDesk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // =========================================================
        // MES QUE SE ESTÁ MOSTRANDO EN EL CALENDARIO
        // =========================================================

        private DateTime _mesCalendarioActual;


        // =========================================================
        // LISTA GENERAL DE TAREAS
        // =========================================================
        //
        // Esta lista contiene todas las tareas cargadas
        // desde el archivo JSON.
        //
        // =========================================================

        private List<TareaModel> _tareas =
            new List<TareaModel>();


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public MainWindow()
        {
            InitializeComponent();


            // =====================================================
            // MES ACTUAL
            // =====================================================

            _mesCalendarioActual =
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1
                );


            // =====================================================
            // CARGAR TAREAS GUARDADAS
            // =====================================================

            CargarTareas();


            // =====================================================
            // GENERAR CALENDARIO
            // =====================================================

            ActualizarCalendario();
        }


        // =========================================================
        // CARGAR TAREAS DESDE JSON
        // =========================================================

        private void CargarTareas()
        {
            // =====================================================
            // LEER ARCHIVO JSON
            // =====================================================

            _tareas =
                TareaStorage.Cargar();


            // =====================================================
            // LIMPIAR LISTA VISUAL
            // =====================================================

            ListTask.Children.Clear();


            // =====================================================
            // RECREAR TODAS LAS TASKCARD
            // =====================================================

            foreach (TareaModel tarea in _tareas)
            {
                CrearTaskCard(
                    tarea
                );
            }
        }


        // =========================================================
        // CREAR TASKCARD
        // =========================================================

        private void CrearTaskCard(
            TareaModel tarea)
        {
            TaskCard nuevaTarea =
                new TaskCard(
                    tarea.Titulo,
                    tarea.Descripcion,
                    tarea.Fecha,
                    tarea.Color,
                    tarea.Prioridad,
                    tarea.Categoria
                );


            ListTask.Children.Add(
                nuevaTarea
            );
        }


        // =========================================================
        // BOTÓN AGREGAR TAREA
        // =========================================================

        private void BtnAddTask_Click(
            object sender,
            RoutedEventArgs e)
        {
            // =====================================================
            // ABRIR NEWLIST
            // =====================================================

            NewList ventanaNuevaTarea =
                new NewList();


            ventanaNuevaTarea.Owner =
                this;


            bool? resultado =
                ventanaNuevaTarea.ShowDialog();


            // =====================================================
            // SI SE CREÓ LA TAREA
            // =====================================================

            if (resultado == true)
            {
                // =================================================
                // CREAR MODELO DE LA TAREA
                // =================================================

                TareaModel nuevaTarea =
                    new TareaModel
                    {
                        Titulo =
                            ventanaNuevaTarea
                                .TituloTarea,

                        Descripcion =
                            ventanaNuevaTarea
                                .DescripcionTarea,

                        Fecha =
                            ventanaNuevaTarea
                                .FechaTarea,

                        Color =
                            ventanaNuevaTarea
                                .ColorTarea,

                        Prioridad =
                            ventanaNuevaTarea
                                .PrioridadSeleccionada,

                        Categoria =
                            ventanaNuevaTarea
                                .CategoriaSeleccionada
                    };


                // =================================================
                // AGREGAR A LA LISTA GENERAL
                // =================================================

                _tareas.Add(
                    nuevaTarea
                );


                // =================================================
                // GUARDAR TODO EN JSON
                // =================================================

                TareaStorage.Guardar(
                    _tareas
                );


                // =================================================
                // MOSTRAR TASKCARD
                // =================================================

                CrearTaskCard(
                    nuevaTarea
                );


                // =================================================
                // SI TIENE FECHA
                // =================================================

                DateTime fechaConvertida;


                if (IntentarConvertirFecha(
                        nuevaTarea.Fecha,
                        out fechaConvertida))
                {
                    // Mostrar automáticamente
                    // el mes de la tarea recién creada.

                    _mesCalendarioActual =
                        new DateTime(
                            fechaConvertida.Year,
                            fechaConvertida.Month,
                            1
                        );
                }


                // =================================================
                // ACTUALIZAR CALENDARIO EN TIEMPO REAL
                // =================================================

                ActualizarCalendario();
            }
        }


        // =========================================================
        // CONVERTIR FECHA ESCRITA
        // =========================================================

        private bool IntentarConvertirFecha(
            string fechaTexto,
            out DateTime fecha)
        {
            fecha =
                DateTime.MinValue;


            if (string.IsNullOrWhiteSpace(fechaTexto))
            {
                return false;
            }


            string fechaLimpia =
                fechaTexto.Trim();


            // =====================================================
            // LIMPIAR FORMATO
            // =====================================================
            //
            // Ejemplo:
            //
            // 12 de Noviembre, 2026
            //
            // pasa a:
            //
            // 12 Noviembre 2026
            //
            // =====================================================

            fechaLimpia =
                fechaLimpia.Replace(
                    " de ",
                    " "
                );


            fechaLimpia =
                fechaLimpia.Replace(
                    ",",
                    ""
                );


            // =====================================================
            // CULTURA ESPAÑOL MÉXICO
            // =====================================================

            CultureInfo cultura =
                new CultureInfo(
                    "es-MX"
                );


            // =====================================================
            // INTENTO NORMAL
            // =====================================================

            if (DateTime.TryParse(
                    fechaLimpia,
                    cultura,
                    DateTimeStyles.AllowWhiteSpaces,
                    out fecha))
            {
                return true;
            }


            // =====================================================
            // FORMATOS ALTERNATIVOS
            // =====================================================

            string[] formatos =
            {
                "d MMMM yyyy",
                "dd MMMM yyyy",
                "d/M/yyyy",
                "dd/MM/yyyy",
                "d-MM-yyyy",
                "dd-MM-yyyy"
            };


            return DateTime.TryParseExact(
                fechaLimpia,
                formatos,
                cultura,
                DateTimeStyles.AllowWhiteSpaces,
                out fecha
            );
        }


        // =========================================================
        // ACTUALIZAR CALENDARIO
        // =========================================================

        private void ActualizarCalendario()
        {
            // =====================================================
            // SEGURIDAD
            // =====================================================

            if (CalendarioDias == null ||
                TxtMesCalendario == null)
            {
                return;
            }


            // =====================================================
            // LIMPIAR DÍAS ANTERIORES
            // =====================================================

            CalendarioDias.Children.Clear();


            // =====================================================
            // MOSTRAR MES Y AÑO
            // =====================================================

            CultureInfo cultura =
                new CultureInfo(
                    "es-MX"
                );


            string nombreMes =
                _mesCalendarioActual.ToString(
                    "MMMM yyyy",
                    cultura
                );


            if (!string.IsNullOrWhiteSpace(nombreMes))
            {
                nombreMes =
                    char.ToUpper(
                        nombreMes[0],
                        cultura
                    )
                    +
                    nombreMes.Substring(1);
            }


            TxtMesCalendario.Text =
                nombreMes;


            // =====================================================
            // PRIMER DÍA DEL MES
            // =====================================================

            DateTime primerDiaMes =
                new DateTime(
                    _mesCalendarioActual.Year,
                    _mesCalendarioActual.Month,
                    1
                );


            // =====================================================
            // CALCULAR PRIMER DÍA VISIBLE
            // =====================================================

            int diasHaciaAtras =
                (int)primerDiaMes.DayOfWeek;


            DateTime primerDiaVisible =
                primerDiaMes.AddDays(
                    -diasHaciaAtras
                );


            // =====================================================
            // CREAR 42 CASILLAS
            // =====================================================
            //
            // 7 columnas
            // 6 filas
            //
            // =====================================================

            for (int i = 0; i < 42; i++)
            {
                DateTime fechaCelda =
                    primerDiaVisible.AddDays(i);


                Border celda =
                    CrearCeldaCalendario(
                        fechaCelda
                    );


                CalendarioDias.Children.Add(
                    celda
                );
            }
        }


        // =========================================================
        // CREAR CADA DÍA DEL CALENDARIO
        // =========================================================

        private Border CrearCeldaCalendario(
            DateTime fecha)
        {
            // =====================================================
            // CONTENEDOR
            // =====================================================

            Border contenedor =
                new Border
                {
                    MinHeight = 38,

                    Margin =
                        new Thickness(1),

                    CornerRadius =
                        new CornerRadius(12),

                    Background =
                        Brushes.Transparent
                };


            // =====================================================
            // CONTENIDO
            // =====================================================

            StackPanel contenido =
                new StackPanel
                {
                    HorizontalAlignment =
                        HorizontalAlignment.Center,

                    VerticalAlignment =
                        VerticalAlignment.Center
                };


            // =====================================================
            // NÚMERO DEL DÍA
            // =====================================================

            TextBlock numeroDia =
                new TextBlock
                {
                    Text =
                        fecha.Day.ToString(),

                    FontSize = 11,

                    HorizontalAlignment =
                        HorizontalAlignment.Center,

                    VerticalAlignment =
                        VerticalAlignment.Center
                };


            Brush colorTexto =
                FindResource(
                    "ColorTexto"
                ) as Brush
                ?? Brushes.Black;


            numeroDia.Foreground =
                colorTexto;


            // =====================================================
            // DÍAS DE OTRO MES
            // =====================================================

            if (fecha.Month !=
                _mesCalendarioActual.Month)
            {
                numeroDia.Opacity =
                    0.35;
            }


            // =====================================================
            // DÍA ACTUAL
            // =====================================================

            if (fecha.Date ==
                DateTime.Today.Date)
            {
                contenedor.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            43,
                            83,
                            180
                        )
                    );


                numeroDia.Foreground =
                    Brushes.White;


                numeroDia.FontWeight =
                    FontWeights.Bold;


                numeroDia.Opacity =
                    1;
            }


            contenido.Children.Add(
                numeroDia
            );


            // =====================================================
            // BUSCAR TAREAS DEL DÍA
            // =====================================================

            List<TareaModel> tareasDelDia =
                _tareas
                    .Where(
                        tarea =>
                        {
                            DateTime fechaTarea;


                            if (!IntentarConvertirFecha(
                                    tarea.Fecha,
                                    out fechaTarea))
                            {
                                return false;
                            }


                            return
                                fechaTarea.Date ==
                                fecha.Date;
                        }
                    )
                    .ToList();


            // =====================================================
            // MOSTRAR COLORES
            // =====================================================

            if (tareasDelDia.Count > 0)
            {
                WrapPanel panelIndicadores =
                    new WrapPanel
                    {
                        HorizontalAlignment =
                            HorizontalAlignment.Center,

                        Margin =
                            new Thickness(
                                0,
                                2,
                                0,
                                0
                            )
                    };


                // =================================================
                // MÁXIMO 4 INDICADORES
                // =================================================

                foreach (
                    TareaModel tarea
                    in tareasDelDia.Take(4))
                {
                    try
                    {
                        Color colorTarea =
                            (Color)
                            ColorConverter.ConvertFromString(
                                tarea.Color
                            );


                        Ellipse indicador =
                            new Ellipse
                            {
                                Width = 6,

                                Height = 6,

                                Margin =
                                    new Thickness(
                                        1,
                                        0,
                                        1,
                                        0
                                    ),

                                Fill =
                                    new SolidColorBrush(
                                        colorTarea
                                    ),

                                ToolTip =
                                    tarea.Titulo
                            };


                        panelIndicadores
                            .Children
                            .Add(
                                indicador
                            );
                    }
                    catch
                    {
                        // Si el color fuera inválido,
                        // simplemente no mostramos
                        // ese indicador.
                    }
                }


                contenido.Children.Add(
                    panelIndicadores
                );
            }


            contenedor.Child =
                contenido;


            return contenedor;
        }


        // =========================================================
        // MES ANTERIOR
        // =========================================================

        private void MesAnterior_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            _mesCalendarioActual =
                _mesCalendarioActual.AddMonths(
                    -1
                );


            ActualizarCalendario();
        }


        // =========================================================
        // MES SIGUIENTE
        // =========================================================

        private void MesSiguiente_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            _mesCalendarioActual =
                _mesCalendarioActual.AddMonths(
                    1
                );


            ActualizarCalendario();
        }


        // =========================================================
        // BOTÓN AJUSTES
        // =========================================================

        private void Ajustes_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            ajustes ventanaAjustes =
                new ajustes();


            ventanaAjustes.ShowDialog();
        }


        // =========================================================
        // BOTÓN CALENDARIO
        // =========================================================

        private void Calendario_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            Calendario ventanaCalendario =
                new Calendario();


            ventanaCalendario.ShowDialog();
        }
    }
}