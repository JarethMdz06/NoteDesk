using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace NoteDesk
{
    public partial class Calendario : Window
    {
        private readonly List<TareaModel> _tareas;
        private DateTime _mesActual;
        private bool _actualizandoControles;

        public Calendario()
        {
            InitializeComponent();

            _tareas = TareaStorage.Cargar();
            _mesActual = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            CargarAnosDisponibles();
            ActualizarVista();
        }

        private void CargarAnosDisponibles()
        {
            _actualizandoControles = true;
            CmbAnio.Items.Clear();

            SortedSet<int> anos = new SortedSet<int>();
            for (int ano = Math.Max(1, DateTime.Today.Year - 10);
                 ano <= Math.Min(9999, DateTime.Today.Year + 10);
                 ano++)
            {
                anos.Add(ano);
            }

            foreach (TareaModel tarea in _tareas)
            {
                if (TareaFechaParser.TryParse(tarea.Fecha, out DateTime fecha))
                {
                    anos.Add(fecha.Year);
                }
            }

            foreach (int ano in anos)
            {
                CmbAnio.Items.Add(ano.ToString(CultureInfo.InvariantCulture));
            }

            CmbAnio.SelectedItem = _mesActual.Year.ToString(CultureInfo.InvariantCulture);
            _actualizandoControles = false;
        }

        private void ActualizarVista()
        {
            CultureInfo cultura = new CultureInfo("es-MX");
            string nombreMes = _mesActual.ToString("MMMM yyyy", cultura);
            TxtMesCalendario.Text = char.ToUpper(nombreMes[0], cultura) + nombreMes.Substring(1);

            _actualizandoControles = true;
            CmbAnio.SelectedItem = _mesActual.Year.ToString(CultureInfo.InvariantCulture);
            _actualizandoControles = false;

            ActualizarBotonesMes();
            ActualizarDias();
            ActualizarListaTareas();
        }

        private void ActualizarBotonesMes()
        {
            Brush fondoNormal = (Brush)FindResource("ColorSuperficie");
            Brush bordeNormal = (Brush)FindResource("ColorBorde");
            Brush fondoSeleccionado = (Brush)FindResource("ColorMesSeleccionado");
            Brush bordeSeleccionado = (Brush)FindResource("ColorBordeMesSeleccionado");

            foreach (Button boton in MesesDelAnio.Children.OfType<Button>())
            {
                bool seleccionado = int.Parse((string)boton.Tag, CultureInfo.InvariantCulture) == _mesActual.Month;
                boton.Background = seleccionado ? fondoSeleccionado : fondoNormal;
                boton.BorderBrush = seleccionado ? bordeSeleccionado : bordeNormal;
                boton.BorderThickness = seleccionado ? new Thickness(2) : new Thickness(1);
            }
        }

        private void ActualizarDias()
        {
            CalendarioDias.Children.Clear();
            CalendarioDias.Rows = 6;

            DateTime primerDiaMes = new DateTime(_mesActual.Year, _mesActual.Month, 1);
            DateTime primerDiaVisible = primerDiaMes.AddDays(-(int)primerDiaMes.DayOfWeek);
            Dictionary<DateTime, List<TareaModel>> tareasPorDia = new Dictionary<DateTime, List<TareaModel>>();

            foreach (TareaModel tarea in _tareas)
            {
                if (!TareaFechaParser.TryParse(tarea.Fecha, out DateTime fecha))
                {
                    continue;
                }

                DateTime dia = fecha.Date;
                if (!tareasPorDia.TryGetValue(dia, out List<TareaModel>? tareasDelDia))
                {
                    tareasDelDia = new List<TareaModel>();
                    tareasPorDia.Add(dia, tareasDelDia);
                }

                tareasDelDia.Add(tarea);
            }

            for (int indice = 0; indice < 42; indice++)
            {
                DateTime fecha = primerDiaVisible.AddDays(indice);
                tareasPorDia.TryGetValue(fecha.Date, out List<TareaModel>? tareasDelDia);
                CalendarioDias.Children.Add(CrearCelda(fecha, tareasDelDia ?? new List<TareaModel>()));
            }
        }

        private Border CrearCelda(DateTime fecha, List<TareaModel> tareasDelDia)
        {
            Border celda = new Border
            {
                Style = (Style)FindResource("CalendarCell"),
                ToolTip = tareasDelDia.Count == 0
                    ? null
                    : string.Join(Environment.NewLine, tareasDelDia.Select(tarea => tarea.Titulo))
            };

            StackPanel contenido = new StackPanel();
            TextBlock numero = new TextBlock
            {
                Text = fecha.Day.ToString(CultureInfo.InvariantCulture),
                FontSize = 12,
                FontWeight = fecha.Date == DateTime.Today ? FontWeights.Bold : FontWeights.Normal,
                Foreground = (Brush)FindResource("ColorTexto")
            };

            if (fecha.Month != _mesActual.Month)
            {
                numero.Foreground = (Brush)FindResource("ColorDiasOtroMes");
            }

            if (fecha.Date == DateTime.Today)
            {
                Border indicadorHoy = new Border
                {
                    Width = 26,
                    Height = 26,
                    CornerRadius = new CornerRadius(13),
                    Background = new SolidColorBrush(Color.FromRgb(45, 102, 215)),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Child = new TextBlock
                    {
                        Text = numero.Text,
                        FontSize = numero.FontSize,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                contenido.Children.Add(indicadorHoy);
            }
            else
            {
                contenido.Children.Add(numero);
            }

            foreach (TareaModel tarea in tareasDelDia.Take(2))
            {
                contenido.Children.Add(CrearEtiquetaTarea(tarea));
            }

            if (tareasDelDia.Count > 2)
            {
                contenido.Children.Add(new TextBlock
                {
                    Text = $"+{tareasDelDia.Count - 2} más",
                    FontSize = 9,
                    Foreground = (Brush)FindResource("ColorTextoSecundario"),
                    ToolTip = string.Join(Environment.NewLine, tareasDelDia.Skip(2).Select(tarea => tarea.Titulo))
                });
            }

            celda.Child = contenido;
            return celda;
        }

        private Border CrearEtiquetaTarea(TareaModel tarea)
        {
            Border etiqueta = new Border
            {
                Background = new SolidColorBrush(ObtenerColorCategoria(tarea)),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(4, 2, 4, 2),
                Margin = new Thickness(0, 4, 0, 0),
                ToolTip = tarea.Titulo
            };

            etiqueta.Child = new TextBlock
            {
                Text = $"{ObtenerIconoCategoria(tarea.Categoria)} {tarea.Titulo}",
                FontSize = 9,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Foreground = (Brush)FindResource("ColorTexto")
            };

            return etiqueta;
        }

        private void ActualizarListaTareas()
        {
            PanelTareasMes.Children.Clear();

            CultureInfo cultura = new CultureInfo("es-MX");
            string nombreMes = _mesActual.ToString("MMMM", cultura);
            nombreMes = char.ToUpper(nombreMes[0], cultura) + nombreMes.Substring(1);

            PanelTareasMes.Children.Add(new TextBlock
            {
                Text = $"Tareas de {nombreMes}",
                FontSize = 17,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 12)
            });

            StackPanel lista = new StackPanel();
            List<(TareaModel Tarea, DateTime Fecha)> tareasDelMes = _tareas
                .Select(tarea => TareaFechaParser.TryParse(tarea.Fecha, out DateTime fecha)
                    ? (Tarea: tarea, Fecha: fecha.Date)
                    : (Tarea: tarea, Fecha: DateTime.MinValue))
                .Where(item => item.Fecha != DateTime.MinValue &&
                               item.Fecha.Year == _mesActual.Year &&
                               item.Fecha.Month == _mesActual.Month)
                .OrderBy(item => item.Fecha)
                .ThenBy(item => item.Tarea.Titulo)
                .ToList();

            if (tareasDelMes.Count == 0)
            {
                lista.Children.Add(CrearTextoVacio("No hay tareas para este mes."));
            }
            else
            {
                foreach ((TareaModel tarea, DateTime fecha) in tareasDelMes)
                {
                    lista.Children.Add(CrearTarjetaTarea(tarea, fecha.ToString("d 'de' MMMM", cultura)));
                }
            }

            List<TareaModel> tareasSinFecha = _tareas
                .Where(tarea => !TareaFechaParser.TryParse(tarea.Fecha, out _))
                .OrderBy(tarea => tarea.Titulo)
                .ToList();

            if (tareasSinFecha.Count > 0)
            {
                lista.Children.Add(new TextBlock
                {
                    Text = "Sin fecha",
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 8)
                });

                foreach (TareaModel tarea in tareasSinFecha)
                {
                    lista.Children.Add(CrearTarjetaTarea(tarea, "Sin fecha"));
                }
            }

            PanelTareasMes.Children.Add(new ScrollViewer
            {
                MaxHeight = 390,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = lista
            });
        }

        private Border CrearTarjetaTarea(TareaModel tarea, string textoFecha)
        {
            Border icono = new Border
            {
                Width = 34,
                Height = 34,
                Background = new SolidColorBrush(ObtenerColorCategoria(tarea)),
                CornerRadius = new CornerRadius(8),
                Child = new TextBlock
                {
                    Text = ObtenerIconoCategoria(tarea.Categoria),
                    FontFamily = new FontFamily("Segoe UI Emoji"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            StackPanel informacion = new StackPanel
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            informacion.Children.Add(new TextBlock
            {
                Text = tarea.Titulo,
                FontWeight = FontWeights.SemiBold,
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = tarea.Titulo
            });
            informacion.Children.Add(new TextBlock
            {
                Text = textoFecha,
                FontSize = 11,
                Foreground = (Brush)FindResource("ColorTextoSecundario")
            });

            Grid contenido = new Grid();
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contenido.Children.Add(icono);
            Grid.SetColumn(informacion, 1);
            contenido.Children.Add(informacion);

            return new Border
            {
                Style = (Style)FindResource("TaskItemStyle"),
                ToolTip = tarea.Descripcion,
                Child = contenido
            };
        }

        private static TextBlock CrearTextoVacio(string texto)
        {
            return new TextBlock
            {
                Text = texto,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 4, 0, 10)
            };
        }

        private static string ObtenerIconoCategoria(string categoria)
        {
            return categoria switch
            {
                "Work" => "💼",
                "School" => "🎓",
                "Home" => "⌂",
                "Shopping" => "🛒",
                "Personal" => "👤",
                "Hobbies" => "🎮",
                "Health" => "♥",
                "Other" => "•••",
                _ => "•"
            };
        }

        private static Color ObtenerColorCategoria(TareaModel tarea)
        {
            string? color = tarea.Categoria switch
            {
                "Work" => "#FFF3CE",
                "School" => "#F1E6FF",
                "Home" => "#E6F1FF",
                "Shopping" => "#E1F6E8",
                "Personal" => "#FFE4EA",
                "Hobbies" => "#F0E2FF",
                "Health" => "#FFE3E8",
                "Other" => "#ECECEC",
                _ => tarea.Color
            };

            return (Color)ColorConverter.ConvertFromString(color)!;
        }

        private void MesAnterior_Click(object sender, RoutedEventArgs e)
        {
            _mesActual = _mesActual.AddMonths(-1);
            ActualizarVista();
        }

        private void MesSiguiente_Click(object sender, RoutedEventArgs e)
        {
            _mesActual = _mesActual.AddMonths(1);
            ActualizarVista();
        }

        private void Hoy_Click(object sender, RoutedEventArgs e)
        {
            _mesActual = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ActualizarVista();
        }

        private void Mes_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && int.TryParse((string)boton.Tag, out int mes))
            {
                _mesActual = new DateTime(_mesActual.Year, mes, 1);
                ActualizarVista();
            }
        }

        private void Anio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_actualizandoControles || CmbAnio.SelectedItem is not string textoAnio ||
                !int.TryParse(textoAnio, NumberStyles.None, CultureInfo.InvariantCulture, out int ano))
            {
                return;
            }

            _mesActual = new DateTime(ano, _mesActual.Month, 1);
            ActualizarVista();
        }
    }
}
