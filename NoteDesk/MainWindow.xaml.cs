using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using NoteDesk.Services;

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

        private string _categoriaSeleccionada = "*";

        private TareaModel? _tareaSeleccionada;

        private readonly PreferenciasUsuario _preferencias;

        private readonly DispatcherTimer _timerRecordatorios;

        private static readonly TimeSpan IntervaloSinAvance = TimeSpan.FromDays(1);


        // =========================================================
        // SECCIONES DE LA VENTANA PRINCIPAL
        // =========================================================
        //
        // Calendario y Ajustes ya no son ventanas separadas.
        // Se cargan dentro de ContenedorSeccion.
        //
        // =========================================================

        private Calendario? _vistaCalendario;

        private ajustes? _vistaAjustes;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public MainWindow()
        {
            InitializeComponent();
            CmbFiltroCategoria.SelectionChanged += FiltroCategoria_SelectionChanged;
            _preferencias = PreferenciasUsuarioStorage.Cargar();
            _timerRecordatorios = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _timerRecordatorios.Tick += RecordatorioSinAvance_Tick;
            if (_preferencias.RecordatoriosActivos)
            {
                _timerRecordatorios.Start();
            }


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


            // =====================================================
            // NAVEGACIÓN INICIAL
            // =====================================================

            MostrarInicio();
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

            foreach (TareaModel tarea in _tareas)
            {
                tarea.Subtareas ??= new List<SubtareaModel>();
                if (tarea.Subtareas.Count > 0 && tarea.UltimoAvanceSubtareasUtc == default)
                {
                    tarea.UltimoAvanceSubtareasUtc = DateTime.UtcNow;
                }
            }

            _tareaSeleccionada = _tareas.FirstOrDefault(tarea => !tarea.Completada)
                ?? _tareas.FirstOrDefault();

            ActualizarListaVisual();
            ActualizarProgreso();
        }


        private void ActualizarListaVisual()
        {
            ListTask.Children.Clear();

            IEnumerable<TareaModel> tareasFiltradas = _categoriaSeleccionada == "*"
                ? _tareas
                : _tareas.Where(tarea => string.Equals(
                    tarea.Categoria,
                    _categoriaSeleccionada,
                    StringComparison.OrdinalIgnoreCase));

            foreach (TareaModel tarea in tareasFiltradas)
            {
                CrearTaskCard(tarea);
            }
        }

        private void FiltroCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbFiltroCategoria.SelectedValue is string categoria)
            {
                _categoriaSeleccionada = categoria;
                ActualizarListaVisual();
            }
        }


        // =========================================================
        // CREAR TASKCARD
        // =========================================================

        private void CrearTaskCard(
            TareaModel tarea)
        {
            TaskCard nuevaTarea = new TaskCard(tarea);
            nuevaTarea.EditRequested += EditarTarea;
            nuevaTarea.DeleteRequested += EliminarTarea;
            nuevaTarea.CompletionChanged += TareaCompletada_Cambiada;
            nuevaTarea.SelectionRequested += TareaSeleccionada;
            nuevaTarea.SubtareasChanged += GuardarSubtareasDesdeSticky;
            nuevaTarea.EstablecerSeleccionada(ReferenceEquals(tarea, _tareaSeleccionada));

            ListTask.Children.Add(nuevaTarea);
        }

        private void TareaSeleccionada(TareaModel tarea)
        {
            _tareaSeleccionada = tarea;
            foreach (TaskCard tarjeta in ListTask.Children.OfType<TaskCard>())
            {
                tarjeta.EstablecerSeleccionada(ReferenceEquals(tarjeta.TareaAsociada, tarea));
            }

            ActualizarProgreso();
        }

        private bool GuardarSubtareasDesdeSticky(TareaModel tarea)
        {
            bool estabaCompletada = tarea.Completada;
            DateTime ultimoAvanceAnterior = tarea.UltimoAvanceSubtareasUtc;
            DateTime? ultimoRecordatorioAnterior = tarea.UltimoRecordatorioSubtareasUtc;
            if (tarea.Subtareas.Count > 0)
            {
                tarea.Completada = tarea.Subtareas.All(subtarea => subtarea.Completada);
            }
            tarea.UltimoAvanceSubtareasUtc = DateTime.UtcNow;
            tarea.UltimoRecordatorioSubtareasUtc = null;

            if (!TareaStorage.Guardar(_tareas))
            {
                tarea.Completada = estabaCompletada;
                tarea.UltimoAvanceSubtareasUtc = ultimoAvanceAnterior;
                tarea.UltimoRecordatorioSubtareasUtc = ultimoRecordatorioAnterior;
                return false;
            }

            ActualizarProgreso();
            if (tarea.Completada != estabaCompletada)
            {
                ActualizarListaVisual();
                ActualizarCalendario();
                if (tarea.Completada)
                {
                    NotificarTareaCompletada(tarea);
                }
            }
            return true;
        }

        private void ActualizarVentanasSticky(TareaModel tarea)
        {
            foreach (StickyTaskWindow ventana in Application.Current.Windows.OfType<StickyTaskWindow>()
                         .Where(ventana => ReferenceEquals(ventana.TareaAsociada, tarea)))
            {
                ventana.ActualizarTarea(tarea);
            }
        }

        private void TareaCompletada_Cambiada(TareaModel tarea, bool completada)
        {
            tarea.Completada = completada;
            if (!TareaStorage.Guardar(_tareas))
            {
                tarea.Completada = !completada;
                ActualizarListaVisual();
                ActualizarProgreso();
                return;
            }

            ActualizarListaVisual();
            ActualizarCalendario();
            ActualizarProgreso();

            if (completada)
            {
                NotificarTareaCompletada(tarea);
            }
        }

        private void NotificarTareaCompletada(TareaModel tarea)
        {
            if (_preferencias.SonidoActivo)
            {
                SonidoNotificaciones.Reproducir(_preferencias.SonidoSeleccionado);
            }

            if (_preferencias.RecordatoriosActivos)
            {
                MessageBox.Show(
                    $"¡Tarea completada!\n\n{tarea.Titulo}",
                    "NoteDesk",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void ActualizarProgreso()
        {
            if (PanelProgresoSeleccionado == null || TxtPorcentajeProgreso == null || PathProgreso == null)
            {
                return;
            }

            bool hayTarea = _tareaSeleccionada != null;
            PanelProgresoSeleccionado.Visibility = hayTarea ? Visibility.Visible : Visibility.Collapsed;
            TxtProgresoVacio.Visibility = hayTarea ? Visibility.Collapsed : Visibility.Visible;
            if (!hayTarea)
            {
                return;
            }

            TareaModel tarea = _tareaSeleccionada!;
            tarea.Subtareas ??= new List<SubtareaModel>();
            TxtProgresoTitulo.Text = tarea.Titulo;
            TxtProgresoIcono.Text = ObtenerIconoCategoria(tarea.Categoria);

            bool tieneCategoria = !string.IsNullOrWhiteSpace(tarea.Categoria);
            PanelProgresoCategoria.Visibility = tieneCategoria ? Visibility.Visible : Visibility.Collapsed;
            TxtProgresoCategoria.Text = tarea.Categoria;
            TxtProgresoFecha.Text = string.IsNullOrWhiteSpace(tarea.Fecha) ? "" : $"▣  Fecha: {tarea.Fecha}";

            bool tienePrioridad = !string.IsNullOrWhiteSpace(tarea.Prioridad);
            PanelProgresoPrioridad.Visibility = tienePrioridad ? Visibility.Visible : Visibility.Collapsed;
            TxtProgresoPrioridad.Text = $"⚑  Prioridad {tarea.Prioridad}";
            (string prioridadFondo, string prioridadTexto) = tarea.Prioridad switch
            {
                "Alta" => ("#F1B1B1", "#882222"),
                "Media" => ("#F7D89C", "#774400"),
                _ => ("#CDECCF", "#2E6B32")
            };
            PanelProgresoPrioridad.Background = (Brush)new BrushConverter().ConvertFromString(prioridadFondo)!;
            TxtProgresoPrioridad.Foreground = (Brush)new BrushConverter().ConvertFromString(prioridadTexto)!;

            PanelSubtareasProgreso.Children.Clear();
            bool tieneSubtareas = tarea.Subtareas.Count > 0;
            TxtSubtareasVacias.Text = "Esta tarea no tiene subtareas. Su progreso depende de la casilla principal.";
            TxtSubtareasVacias.Visibility = tieneSubtareas
                ? Visibility.Collapsed
                : Visibility.Visible;
            foreach (SubtareaModel subtarea in tarea.Subtareas)
            {
                PanelSubtareasProgreso.Children.Add(CrearFilaSubtarea(tarea, subtarea));
            }

            int total = tieneSubtareas ? tarea.Subtareas.Count : 1;
            int completadas = tieneSubtareas
                ? tarea.Subtareas.Count(subtarea => subtarea.Completada)
                : tarea.Completada ? 1 : 0;
            double progreso = completadas / (double)total;
            int porcentaje = (int)Math.Round(progreso * 100);
            TxtPorcentajeProgreso.Text = $"{porcentaje}%";
            TxtTareasCompletadas.Text = tieneSubtareas
                ? $"{completadas} de {total} subtareas"
                : $"{completadas} de 1 tarea";

            PathProgreso.Data = null;
            PathProgreso.Visibility = Visibility.Collapsed;
            EllipseProgresoFondo.Stroke = (Brush)FindResource("ColorBorde");
            if (porcentaje == 100)
            {
                EllipseProgresoFondo.Stroke = (Brush)new BrushConverter().ConvertFromString("#3FA772")!;
                return;
            }

            if (porcentaje == 0)
            {
                return;
            }

            const double centro = 71;
            const double radio = 59;
            double angulo = progreso * 360;
            double radianes = angulo * Math.PI / 180;
            Point fin = new Point(
                centro + radio * Math.Sin(radianes),
                centro - radio * Math.Cos(radianes));
            PathProgreso.Data = new PathGeometry
            {
                Figures = new PathFigureCollection
                {
                    new PathFigure
                    {
                        StartPoint = new Point(centro, centro - radio),
                        IsClosed = false,
                        Segments = new PathSegmentCollection
                        {
                            new ArcSegment
                            {
                                Point = fin,
                                Size = new Size(radio, radio),
                                IsLargeArc = progreso > 0.5,
                                SweepDirection = SweepDirection.Clockwise,
                                IsStroked = true
                            }
                        }
                    }
                }
            };
            PathProgreso.Visibility = Visibility.Visible;
        }

        private static string ObtenerIconoCategoria(string categoria) => categoria switch
        {
            "Work" => "💼",
            "School" => "🎓",
            "Home" => "🏠",
            "Shopping" => "🛒",
            "Personal" => "👤",
            "Hobbies" => "🎮",
            "Health" => "❤️",
            _ => "✓"
        };

        private UIElement CrearFilaSubtarea(TareaModel tarea, SubtareaModel subtarea)
        {
            Border fila = new Border
            {
                Background = (Brush)FindResource("ColorTarjetaInterna"),
                BorderBrush = (Brush)FindResource("ColorBorde"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(9, 6, 9, 6),
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid contenido = new Grid();
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            CheckBox check = new CheckBox
            {
                Style = (Style)FindResource("SubtareaCheckBox"),
                IsChecked = subtarea.Completada,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            check.Checked += (_, _) => CambiarEstadoSubtarea(tarea, subtarea, true);
            check.Unchecked += (_, _) => CambiarEstadoSubtarea(tarea, subtarea, false);
            contenido.Children.Add(check);

            TextBlock titulo = new TextBlock
            {
                Text = subtarea.Titulo,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (Brush)FindResource("ColorTexto"),
                TextDecorations = subtarea.Completada ? TextDecorations.Strikethrough : null
            };
            check.Checked += (_, _) => titulo.TextDecorations = TextDecorations.Strikethrough;
            check.Unchecked += (_, _) => titulo.TextDecorations = null;
            Grid.SetColumn(titulo, 1);
            contenido.Children.Add(titulo);
            fila.Child = contenido;
            return fila;
        }

        private void CambiarEstadoSubtarea(TareaModel tarea, SubtareaModel subtarea, bool completada)
        {
            bool estadoAnterior = subtarea.Completada;
            bool tareaCompletadaAnterior = tarea.Completada;
            DateTime ultimoAvanceAnterior = tarea.UltimoAvanceSubtareasUtc;
            DateTime? ultimoRecordatorioAnterior = tarea.UltimoRecordatorioSubtareasUtc;

            subtarea.Completada = completada;
            if (tarea.Subtareas.Count > 0)
            {
                tarea.Completada = tarea.Subtareas.All(paso => paso.Completada);
            }
            tarea.UltimoAvanceSubtareasUtc = DateTime.UtcNow;
            tarea.UltimoRecordatorioSubtareasUtc = null;

            if (!TareaStorage.Guardar(_tareas))
            {
                subtarea.Completada = estadoAnterior;
                tarea.Completada = tareaCompletadaAnterior;
                tarea.UltimoAvanceSubtareasUtc = ultimoAvanceAnterior;
                tarea.UltimoRecordatorioSubtareasUtc = ultimoRecordatorioAnterior;
                ActualizarProgreso();
                return;
            }

            ActualizarProgreso();
            ActualizarVentanasSticky(tarea);
            if (tarea.Completada != tareaCompletadaAnterior)
            {
                ActualizarListaVisual();
                ActualizarCalendario();
                if (tarea.Completada)
                {
                    NotificarTareaCompletada(tarea);
                }
            }
        }

        private void RecordatorioSinAvance_Tick(object? sender, EventArgs e)
        {
            if (!_preferencias.RecordatoriosActivos)
            {
                return;
            }

            DateTime ahoraUtc = DateTime.UtcNow;
            List<TareaModel> sinAvance = _tareas
                .Where(tarea => !tarea.Completada &&
                                tarea.Subtareas.Count > 0 &&
                                tarea.Subtareas.Any(subtarea => !subtarea.Completada) &&
                                ahoraUtc - tarea.UltimoAvanceSubtareasUtc >= IntervaloSinAvance &&
                                (tarea.UltimoRecordatorioSubtareasUtc == null ||
                                 ahoraUtc - tarea.UltimoRecordatorioSubtareasUtc.Value >= IntervaloSinAvance))
                .ToList();

            if (sinAvance.Count == 0)
            {
                return;
            }

            Dictionary<TareaModel, DateTime?> recordatoriosAnteriores = sinAvance
                .ToDictionary(tarea => tarea, tarea => tarea.UltimoRecordatorioSubtareasUtc);
            foreach (TareaModel tarea in sinAvance)
            {
                tarea.UltimoRecordatorioSubtareasUtc = ahoraUtc;
            }

            if (!TareaStorage.Guardar(_tareas))
            {
                foreach ((TareaModel tarea, DateTime? recordatorioAnterior) in recordatoriosAnteriores)
                {
                    tarea.UltimoRecordatorioSubtareasUtc = recordatorioAnterior;
                }
                return;
            }

            if (_preferencias.SonidoActivo)
            {
                SonidoNotificaciones.Reproducir(_preferencias.SonidoSeleccionado);
            }

            string listaTareas = string.Join("\n", sinAvance.Select(tarea => $"• {tarea.Titulo}"));
            MessageBox.Show(
                $"Ha pasado 1 día sin avanzar estas subtareas:\n\n{listaTareas}",
                "Recordatorio de NoteDesk",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void NotificacionesActualizadas()
        {
            if (_preferencias.RecordatoriosActivos)
            {
                _timerRecordatorios.Start();
            }
            else
            {
                _timerRecordatorios.Stop();
            }
        }

        private void EditarTarea(TareaModel tarea)
        {
            NewList ventanaEditarTarea = new NewList(tarea)
            {
                Owner = this
            };

            if (ventanaEditarTarea.ShowDialog() != true)
            {
                return;
            }

            bool estabaCompletadaAntesDeEditar = tarea.Completada;
            tarea.Titulo = ventanaEditarTarea.TituloTarea;
            tarea.Descripcion = ventanaEditarTarea.DescripcionTarea;
            tarea.Fecha = ventanaEditarTarea.FechaTarea;
            tarea.Color = ventanaEditarTarea.ColorTarea;
            tarea.Prioridad = ventanaEditarTarea.PrioridadSeleccionada;
            tarea.Categoria = ventanaEditarTarea.CategoriaSeleccionada;
            tarea.Subtareas = ventanaEditarTarea.SubtareasTarea;
            tarea.UltimoAvanceSubtareasUtc = DateTime.UtcNow;
            tarea.UltimoRecordatorioSubtareasUtc = null;
            if (tarea.Subtareas.Count > 0)
            {
                tarea.Completada = tarea.Subtareas.All(subtarea => subtarea.Completada);
            }

            // =====================================================
            // GOOGLE DRIVE
            // =====================================================

            tarea.DriveActivo =
                ventanaEditarTarea.DriveActivo;

            tarea.DriveFolderId =
                ventanaEditarTarea.DriveFolderId;

            tarea.ArchivosDrive =
                ventanaEditarTarea
                    .ArchivosDrive
                    .Select(
                        archivo =>
                            new ArchivoDriveModel
                            {
                                Id = archivo.Id,
                                Nombre = archivo.Nombre,
                                Enlace = archivo.Enlace,
                                MimeType = archivo.MimeType
                            }
                    )
                    .ToList();


            TareaStorage.Guardar(_tareas);
            ActualizarListaVisual();
            ActualizarCalendario();
            ActualizarProgreso();
            ActualizarVentanasSticky(tarea);
            if (!estabaCompletadaAntesDeEditar && tarea.Completada)
            {
                NotificarTareaCompletada(tarea);
            }
        }


        private async void EliminarTarea(TareaModel tarea)
        {
            string mensajeConfirmacion =
                $"¿Quieres eliminar la tarea \"{tarea.Titulo}\"?";


            if (tarea.DriveActivo &&
                !string.IsNullOrWhiteSpace(
                    tarea.DriveFolderId))
            {
                mensajeConfirmacion +=
                    "\n\nLa carpeta de esta tarea en Google Drive " +
                    "también se moverá a la Papelera.";
            }


            MessageBoxResult confirmacion =
                MessageBox.Show(
                    mensajeConfirmacion,
                    "Eliminar tarea",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );


            if (confirmacion !=
                MessageBoxResult.Yes)
            {
                return;
            }


            // =====================================================
            // GOOGLE DRIVE
            // =====================================================
            //
            // Si esta tarea tiene una carpeta propia,
            // primero la mandamos a la Papelera de Drive.
            //
            // Solo después eliminamos la tarea localmente,
            // para no dejar otra carpeta huérfana.
            //
            // =====================================================

            if (tarea.DriveActivo &&
                !string.IsNullOrWhiteSpace(
                    tarea.DriveFolderId))
            {
                try
                {
                    Google.Apis.Drive.v3.DriveService servicioDrive =
                        await GoogleDriveService
                            .ConectarAsync();


                    await GoogleDriveService
                        .EnviarCarpetaAPapeleraAsync(
                            servicioDrive,
                            tarea.DriveFolderId
                        );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "No fue posible eliminar la carpeta de esta tarea en Google Drive.\n\n" +
                        "La tarea no se eliminó para evitar que vuelva a quedar una carpeta huérfana.\n\n" +
                        ex.Message,
                        "Google Drive",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    return;
                }
            }


            // =====================================================
            // ELIMINAR TAREA LOCAL
            // =====================================================

            if (!_tareas.Remove(
                    tarea))
            {
                return;
            }

            if (ReferenceEquals(_tareaSeleccionada, tarea))
            {
                _tareaSeleccionada = _tareas.FirstOrDefault(pendiente => !pendiente.Completada)
                    ?? _tareas.FirstOrDefault();
            }

            TareaStorage.Guardar(
                _tareas
            );


            ActualizarListaVisual();

            ActualizarCalendario();
            ActualizarProgreso();
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
                                .CategoriaSeleccionada,

                        Subtareas = ventanaNuevaTarea.SubtareasTarea,

                        // =========================================
                        // GOOGLE DRIVE
                        // =========================================

                        DriveActivo =
                            ventanaNuevaTarea
                                .DriveActivo,

                        DriveFolderId =
                            ventanaNuevaTarea
                                .DriveFolderId,

                        ArchivosDrive =
                            ventanaNuevaTarea
                                .ArchivosDrive
                                .Select(
                                    archivo =>
                                        new ArchivoDriveModel
                                        {
                                            Id = archivo.Id,
                                            Nombre = archivo.Nombre,
                                            Enlace = archivo.Enlace,
                                            MimeType = archivo.MimeType
                                        }
                                )
                                .ToList()
                    };


                // =================================================
                // AGREGAR A LA LISTA GENERAL
                // =================================================

                _tareas.Add(
                    nuevaTarea
                );
                _tareaSeleccionada = nuevaTarea;


                // =================================================
                // GUARDAR TODO EN JSON
                // =================================================

                TareaStorage.Guardar(
                    _tareas
                );


                // =================================================
                // ACTUALIZAR LISTA CON EL FILTRO ACTIVO
                // =================================================

                ActualizarListaVisual();
                ActualizarProgreso();


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
            return TareaFechaParser.TryParse(fechaTexto, out fecha);
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
            RoutedEventArgs e)
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
            RoutedEventArgs e)
        {
            _mesCalendarioActual =
                _mesCalendarioActual.AddMonths(
                    1
                );


            ActualizarCalendario();
        }


        // =========================================================
        // NAVEGACIÓN - INICIO
        // =========================================================

        private void Inicio_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            MostrarInicio();
        }


        private void MostrarInicio()
        {
            VistaInicio.Visibility =
                Visibility.Visible;


            ContenedorSeccion.Visibility =
                Visibility.Collapsed;


            ActualizarNavegacion(
                "Inicio"
            );
        }


        // =========================================================
        // NAVEGACIÓN - CALENDARIO
        // =========================================================

        private void Calendario_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            if (_vistaCalendario == null)
            {
                _vistaCalendario =
                    new Calendario();
            }
            else
            {
                // Refleja tareas creadas, editadas o eliminadas
                // desde Inicio.

                _vistaCalendario
                    .RecargarTareas();
            }


            ContenedorSeccion.Content =
                _vistaCalendario;


            VistaInicio.Visibility =
                Visibility.Collapsed;


            ContenedorSeccion.Visibility =
                Visibility.Visible;


            ActualizarNavegacion(
                "Calendario"
            );
        }


        // =========================================================
        // NAVEGACIÓN - AJUSTES
        // =========================================================

        private void Ajustes_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            if (_vistaAjustes == null)
            {
                _vistaAjustes =
                    new ajustes(_preferencias);
                _vistaAjustes.NotificacionesActualizadas += NotificacionesActualizadas;
            }


            ContenedorSeccion.Content =
                _vistaAjustes;


            VistaInicio.Visibility =
                Visibility.Collapsed;


            ContenedorSeccion.Visibility =
                Visibility.Visible;


            ActualizarNavegacion(
                "Ajustes"
            );
        }


        // =========================================================
        // ESTADO VISUAL DE LA BARRA DE NAVEGACIÓN
        // =========================================================

        private void ActualizarNavegacion(
            string seccionActiva)
        {
            AplicarEstadoNavegacion(
                NavInicio,
                TxtNavInicio,
                seccionActiva == "Inicio"
            );


            AplicarEstadoNavegacion(
                NavCalendario,
                TxtNavCalendario,
                seccionActiva == "Calendario"
            );


            AplicarEstadoNavegacion(
                NavAjustes,
                TxtNavAjustes,
                seccionActiva == "Ajustes"
            );
        }


        private void AplicarEstadoNavegacion(
            Border contenedor,
            TextBlock texto,
            bool activo)
        {
            if (activo)
            {
                contenedor.SetResourceReference(
                    Border.BackgroundProperty,
                    "ColorTarjetaInterna"
                );


                texto.SetResourceReference(
                    TextBlock.ForegroundProperty,
                    "ColorTexto"
                );


                texto.FontWeight =
                    FontWeights.Bold;
            }
            else
            {
                contenedor.Background =
                    Brushes.Transparent;


                texto.SetResourceReference(
                    TextBlock.ForegroundProperty,
                    "ColorTextoHeader"
                );


                texto.FontWeight =
                    FontWeights.SemiBold;
            }
        }

    }
}