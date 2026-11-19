using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Google.Apis.Drive.v3;
using NoteDesk.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace NoteDesk
{
    public partial class NewList : Window
    {
        // =========================================================
        // DATOS DE LA TAREA
        // =========================================================

        public string TituloTarea { get; private set; } = "";

        public string DescripcionTarea { get; private set; } = "";

        public string FechaTarea { get; private set; } = "";

        public string ColorTarea { get; private set; } = "#F7C84B";

        public string PrioridadSeleccionada { get; private set; } = "";

        public string CategoriaSeleccionada { get; private set; } = "";


        // =========================================================
        // DATOS DE GOOGLE DRIVE QUE REGRESAN A MAINWINDOW
        // =========================================================

        public bool DriveActivo { get; private set; } = false;

        public string DriveFolderId { get; private set; } = "";

        public List<ArchivoDriveModel> ArchivosDrive { get; private set; }
            = new List<ArchivoDriveModel>();


        // =========================================================
        // GOOGLE DRIVE
        // =========================================================

        private DriveService? _driveService;

        private readonly ObservableCollection<
            Google.Apis.Drive.v3.Data.File> _archivosDrive =
                new ObservableCollection<
                    Google.Apis.Drive.v3.Data.File>();


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public NewList()
        {
            InitializeComponent();

            ListaArchivosDrive.ItemsSource =
                _archivosDrive;
        }

        public NewList(TareaModel tarea) : this()
        {
            Title = "Editar Tarea";
            TxtTituloVentana.Text = "Editar Tarea";
            TxtAccionTarea.Text = "Guardar Cambios";

            TxtTitulo.Text = tarea.Titulo;
            TxtDescripcion.Text = string.IsNullOrWhiteSpace(tarea.Descripcion)
                ? "Agrega detalles adicionales..."
                : tarea.Descripcion;
            TxtFecha.Text = string.IsNullOrWhiteSpace(tarea.Fecha)
                ? "Ej. 25 de Abril, 2026"
                : tarea.Fecha;

            SwitchFecha.IsChecked = !string.IsNullOrWhiteSpace(tarea.Fecha);
            PanelFecha.Visibility = SwitchFecha.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;

            SwitchPrioridad.IsChecked = !string.IsNullOrWhiteSpace(tarea.Prioridad);
            SetRadioSeleccionado(
                tarea.Prioridad,
                ("Baja", PrioridadBaja),
                ("Media", PrioridadMedia),
                ("Alta", PrioridadAlta));

            SwitchCategoria.IsChecked = !string.IsNullOrWhiteSpace(tarea.Categoria);
            SetRadioSeleccionado(
                tarea.Categoria,
                ("Work", CategoriaWork),
                ("School", CategoriaSchool),
                ("Home", CategoriaHome),
                ("Shopping", CategoriaShopping),
                ("Personal", CategoriaPersonal),
                ("Hobbies", CategoriaHobbies),
                ("Health", CategoriaHealth),
                ("Other", CategoriaOther));

            ColorTarea = tarea.Color;
            string colorGuardado = tarea.Color.TrimStart('#');
            RadioButton[] colores = ObtenerRadiosColor();
            foreach (RadioButton radio in colores)
            {
                radio.IsChecked = false;
            }

            foreach (RadioButton radio in colores)
            {
                if (radio.Background is SolidColorBrush brush)
                {
                    string colorOpcion = brush.Color.ToString().TrimStart('#');
                    if (string.Equals(colorGuardado, colorOpcion, StringComparison.OrdinalIgnoreCase) ||
                        (colorGuardado.Length == 6 && colorOpcion.EndsWith(colorGuardado, StringComparison.OrdinalIgnoreCase)))
                    {
                        radio.IsChecked = true;
                        break;
                    }
                }
            }


            // =====================================================
            // RESTAURAR GOOGLE DRIVE DE ESTA TAREA
            // =====================================================

            DriveActivo =
                tarea.DriveActivo;

            DriveFolderId =
                tarea.DriveFolderId
                ?? "";

            SwitchDrive.IsChecked =
                tarea.DriveActivo;


            if (tarea.ArchivosDrive != null)
            {
                foreach (ArchivoDriveModel archivo
                    in tarea.ArchivosDrive)
                {
                    _archivosDrive.Add(
                        new Google.Apis.Drive.v3.Data.File
                        {
                            Id =
                                archivo.Id,

                            Name =
                                archivo.Nombre,

                            WebViewLink =
                                archivo.Enlace,

                            MimeType =
                                archivo.MimeType
                        }
                    );
                }
            }


            PanelArchivosDrive.Visibility =
                _archivosDrive.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;


            // Si esta tarea ya usaba Drive, intentamos reconectar
            // automáticamente usando el token guardado.
            if (tarea.DriveActivo)
            {
                Loaded +=
                    NewList_DriveLoaded;
            }
        }

        // =========================================================
        // RECONECTAR DRIVE AUTOMÁTICAMENTE AL EDITAR
        // =========================================================

        private async void NewList_DriveLoaded(
            object sender,
            RoutedEventArgs e)
        {
            Loaded -=
                NewList_DriveLoaded;


            try
            {
                _driveService =
                    await GoogleDriveService
                        .ConectarAsync();


                BtnConectarDrive.ApplyTemplate();


                TextBlock? txtEstadoDrive =
                    BtnConectarDrive.Template.FindName(
                        "TxtEstadoDrive",
                        BtnConectarDrive
                    ) as TextBlock;


                TextBlock? txtSubEstadoDrive =
                    BtnConectarDrive.Template.FindName(
                        "TxtSubEstadoDrive",
                        BtnConectarDrive
                    ) as TextBlock;


                if (txtEstadoDrive != null)
                {
                    txtEstadoDrive.Text =
                        "Google Drive conectado";
                }


                if (txtSubEstadoDrive != null)
                {
                    txtSubEstadoDrive.Text =
                        "Tu cuenta está lista para usar";
                }


                PanelAccionesDrive.Visibility =
                    Visibility.Visible;
            }
            catch
            {
                // El switch permanece activado porque la tarea sí usa Drive,
                // pero dejamos disponible el botón para reconectar manualmente.

                BtnConectarDrive.ApplyTemplate();


                TextBlock? txtEstadoDrive =
                    BtnConectarDrive.Template.FindName(
                        "TxtEstadoDrive",
                        BtnConectarDrive
                    ) as TextBlock;


                TextBlock? txtSubEstadoDrive =
                    BtnConectarDrive.Template.FindName(
                        "TxtSubEstadoDrive",
                        BtnConectarDrive
                    ) as TextBlock;


                if (txtEstadoDrive != null)
                {
                    txtEstadoDrive.Text =
                        "Google Drive no está conectado";
                }


                if (txtSubEstadoDrive != null)
                {
                    txtSubEstadoDrive.Text =
                        "Haz clic para reconectar tu cuenta";
                }


                PanelAccionesDrive.Visibility =
                    Visibility.Collapsed;
            }
        }


        private static void SetRadioSeleccionado(
            string valor,
            params (string Valor, RadioButton Radio)[] opciones)
        {
            foreach ((string valorOpcion, RadioButton radio) in opciones)
            {
                radio.IsChecked = string.Equals(
                    valor,
                    valorOpcion,
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        // =========================================================
        // CANCELAR
        // =========================================================

        private void BtnCancelar_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }


        // =========================================================
        // FECHA
        // =========================================================

        private void ToggleFecha_Checked(
            object sender,
            RoutedEventArgs e)
        {
            if (PanelFecha != null)
            {
                PanelFecha.Visibility =
                    Visibility.Visible;
            }
        }


        private void ToggleFecha_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            if (PanelFecha != null)
            {
                PanelFecha.Visibility =
                    Visibility.Collapsed;
            }
        }


        // =========================================================
        // PLACEHOLDER
        // =========================================================

        private void TextBox_GotFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text ==
                        "Ej. Preparar presentación, Comprar pan..." ||

                    textBox.Text ==
                        "Agrega detalles adicionales..." ||

                    textBox.Text ==
                        "Ej. 25 de Abril, 2026" ||

                    textBox.Text ==
                        "Pega un enlace de Google Drive...")
                {
                    textBox.Text = "";

                    textBox.Foreground =
                        new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString(
                                "#1A1A1A"));
                }
            }
        }


        private void TextBox_LostFocus(
            object sender,
            RoutedEventArgs e)
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


                    textBox.Foreground =
                        new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString(
                                "#888888"));
                }
            }
        }


        private RadioButton[] ObtenerRadiosColor()
        {
            return new[]
            {
                ColorRojo,
                ColorNaranja,
                ColorAmarillo,
                ColorAmarilloClaro,
                ColorVerde,
                ColorVerdeAgua,
                ColorAzulClaro,
                ColorAzul,
                ColorAzulMorado,
                ColorMorado,
                ColorRosa,
                ColorRosaFuerte,
                ColorRosaClaro,
                ColorSalmon,
                ColorDurazno,
                ColorBeige,
                ColorCafe,
                ColorGrisClaro,
                ColorGris,
                ColorGrisMuyClaro
            };
        }

        // =========================================================
        // CONECTAR GOOGLE DRIVE
        // =========================================================

        private async void BtnConectarDrive_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Los TextBlock están dentro del ControlTemplate del botón,
            // por eso se obtienen mediante Template.FindName.

            BtnConectarDrive.ApplyTemplate();


            TextBlock? txtEstadoDrive =
                BtnConectarDrive.Template.FindName(
                    "TxtEstadoDrive",
                    BtnConectarDrive
                ) as TextBlock;


            TextBlock? txtSubEstadoDrive =
                BtnConectarDrive.Template.FindName(
                    "TxtSubEstadoDrive",
                    BtnConectarDrive
                ) as TextBlock;


            if (txtEstadoDrive == null ||
                txtSubEstadoDrive == null)
            {
                MessageBox.Show(
                    "No fue posible localizar los controles visuales de Google Drive.",
                    "Google Drive",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return;
            }


            try
            {
                BtnConectarDrive.IsEnabled =
                    false;


                txtEstadoDrive.Text =
                    "Conectando con Google Drive...";


                txtSubEstadoDrive.Text =
                    "Se abrirá tu navegador para autorizar NoteDesk.";


                _driveService =
                    await GoogleDriveService.ConectarAsync();


                txtEstadoDrive.Text =
                    "Google Drive conectado";


                txtSubEstadoDrive.Text =
                    "Tu cuenta está lista para usar";


                PanelAccionesDrive.Visibility =
                    Visibility.Visible;
            }
            catch (Exception ex)
            {
                txtEstadoDrive.Text =
                    "Google Drive no está conectado";


                txtSubEstadoDrive.Text =
                    "Haz clic para intentar nuevamente";


                PanelAccionesDrive.Visibility =
                    Visibility.Collapsed;


                MessageBox.Show(
                    "No fue posible conectar NoteDesk con Google Drive.\n\n" +
                    ex.Message,
                    "Google Drive",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                BtnConectarDrive.IsEnabled =
                    true;
            }
        }


        // =========================================================
        // SUBIR ARCHIVO A GOOGLE DRIVE
        // =========================================================

        private async void BtnSubirArchivoDrive_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_driveService == null)
            {
                MessageBox.Show(
                    "Primero conecta tu cuenta de Google Drive.",
                    "Google Drive",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }


            OpenFileDialog selectorArchivo =
                new OpenFileDialog
                {
                    Title =
                        "Seleccionar archivo para Google Drive",

                    Filter =
                        "Todos los archivos|*.*|" +
                        "PDF|*.pdf|" +
                        "Documentos de Word|*.doc;*.docx|" +
                        "Hojas de cálculo|*.xls;*.xlsx|" +
                        "Presentaciones|*.ppt;*.pptx|" +
                        "Imágenes|*.jpg;*.jpeg;*.png;*.gif;*.webp|" +
                        "Archivos comprimidos|*.zip"
                };


            bool? resultado =
                selectorArchivo.ShowDialog();


            if (resultado != true)
            {
                return;
            }


            try
            {
                BtnSubirArchivoDrive.IsEnabled =
                    false;

                BtnSubirArchivoDrive.Content =
                    "Subiendo...";


                // =================================================
                // CREAR LA CARPETA DE ESTA TAREA SI TODAVÍA NO EXISTE
                // =================================================

                if (string.IsNullOrWhiteSpace(
                        DriveFolderId))
                {
                    string nombreCarpeta =
                        string.IsNullOrWhiteSpace(
                            TxtTitulo.Text) ||
                        TxtTitulo.Text ==
                            "Ej. Preparar presentación, Comprar pan..."
                        ? "Tarea sin título"
                        : TxtTitulo.Text.Trim();


                    Google.Apis.Drive.v3.Data.File carpetaTarea =
                        await GoogleDriveService
                            .CrearCarpetaTareaAsync(
                                _driveService,
                                nombreCarpeta
                            );


                    DriveFolderId =
                        carpetaTarea.Id
                        ?? "";
                }


                if (string.IsNullOrWhiteSpace(
                        DriveFolderId))
                {
                    throw new Exception(
                        "No fue posible obtener la carpeta de Google Drive de esta tarea."
                    );
                }


                Google.Apis.Drive.v3.Data.File archivoSubido =
                    await GoogleDriveService
                        .SubirArchivoAsync(
                            _driveService,
                            selectorArchivo.FileName,
                            DriveFolderId
                        );


                if (string.IsNullOrWhiteSpace(
                        archivoSubido.Name))
                {
                    archivoSubido.Name =
                        System.IO.Path.GetFileName(
                            selectorArchivo.FileName
                        );
                }


                _archivosDrive.Add(
                    archivoSubido
                );


                PanelArchivosDrive.Visibility =
                    Visibility.Visible;


                MessageBox.Show(
                    "El archivo se subió correctamente a Google Drive.",
                    "Google Drive",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible subir el archivo a Google Drive.\n\n" +
                    ex.Message,
                    "Google Drive",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                BtnSubirArchivoDrive.IsEnabled =
                    true;

                BtnSubirArchivoDrive.Content =
                    "+ Subir archivo";
            }
        }


        // =========================================================
        // ABRIR CARPETA DE ESTA TAREA EN GOOGLE DRIVE
        // =========================================================

        private async void BtnAbrirCarpetaDrive_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                BtnAbrirCarpetaDrive.IsEnabled =
                    false;


                // Si la tarea todavía no tiene carpeta,
                // la creamos en este momento.
                if (string.IsNullOrWhiteSpace(
                        DriveFolderId))
                {
                    if (_driveService == null)
                    {
                        MessageBox.Show(
                            "Primero conecta tu cuenta de Google Drive.",
                            "Google Drive",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );

                        return;
                    }


                    string nombreCarpeta =
                        string.IsNullOrWhiteSpace(
                            TxtTitulo.Text) ||
                        TxtTitulo.Text ==
                            "Ej. Preparar presentación, Comprar pan..."
                        ? "Tarea sin título"
                        : TxtTitulo.Text.Trim();


                    Google.Apis.Drive.v3.Data.File carpetaTarea =
                        await GoogleDriveService
                            .CrearCarpetaTareaAsync(
                                _driveService,
                                nombreCarpeta
                            );


                    DriveFolderId =
                        carpetaTarea.Id
                        ?? "";
                }


                string enlaceCarpeta =
                    GoogleDriveService
                        .ObtenerEnlaceCarpetaTarea(
                            DriveFolderId
                        );


                if (string.IsNullOrWhiteSpace(
                        enlaceCarpeta))
                {
                    MessageBox.Show(
                        "No fue posible obtener el enlace de la carpeta de esta tarea.",
                        "Google Drive",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    return;
                }


                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName =
                            enlaceCarpeta,

                        UseShellExecute =
                            true
                    }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible abrir la carpeta de esta tarea en Google Drive.\n\n" +
                    ex.Message,
                    "Google Drive",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                BtnAbrirCarpetaDrive.IsEnabled =
                    true;
            }
        }


        // =========================================================
        // ABRIR ARCHIVO EN GOOGLE DRIVE
        // =========================================================

        private void BtnAbrirArchivoDrive_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button boton ||
                boton.Tag is not string enlace ||
                string.IsNullOrWhiteSpace(
                    enlace))
            {
                MessageBox.Show(
                    "Todavía no hay un enlace disponible para este archivo.",
                    "Google Drive",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }


            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName =
                            enlace,

                        UseShellExecute =
                            true
                    }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible abrir el archivo en Google Drive.\n\n" +
                    ex.Message,
                    "Google Drive",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        // =========================================================
        // CREAR TAREA
        // =========================================================

        private async void CrearTarea_Click(
            object sender,
            RoutedEventArgs e)
        {
            // =====================================================
            // VALIDAR TÍTULO
            // =====================================================

            if (string.IsNullOrWhiteSpace(TxtTitulo.Text) ||
                TxtTitulo.Text ==
                "Ej. Preparar presentación, Comprar pan...")
            {
                MessageBox.Show(
                    "Escribe un título para la tarea.",
                    "NoteDesk",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }


            // =====================================================
            // TÍTULO
            // =====================================================

            TituloTarea =
                TxtTitulo.Text.Trim();


            // =====================================================
            // DESCRIPCIÓN
            // =====================================================

            DescripcionTarea = "";

            if (!string.IsNullOrWhiteSpace(
                    TxtDescripcion.Text) &&
                TxtDescripcion.Text !=
                    "Agrega detalles adicionales...")
            {
                DescripcionTarea =
                    TxtDescripcion.Text.Trim();
            }


            // =====================================================
            // FECHA
            // =====================================================

            FechaTarea = "";

            if (SwitchFecha.IsChecked == true)
            {
                if (!string.IsNullOrWhiteSpace(
                        TxtFecha.Text) &&
                    TxtFecha.Text !=
                        "Ej. 25 de Abril, 2026")
                {
                    FechaTarea =
                        TxtFecha.Text.Trim();
                }
            }


            // =====================================================
            // PRIORIDAD
            // =====================================================

            PrioridadSeleccionada = "";

            if (SwitchPrioridad.IsChecked == true)
            {
                if (PrioridadBaja.IsChecked == true)
                {
                    PrioridadSeleccionada =
                        "Baja";
                }

                else if (PrioridadMedia.IsChecked == true)
                {
                    PrioridadSeleccionada =
                        "Media";
                }

                else if (PrioridadAlta.IsChecked == true)
                {
                    PrioridadSeleccionada =
                        "Alta";
                }
            }


            // =====================================================
            // CATEGORÍA
            // =====================================================

            CategoriaSeleccionada = "";

            if (SwitchCategoria.IsChecked == true)
            {
                if (CategoriaWork.IsChecked == true)
                {
                    CategoriaSeleccionada =
                        "Work";
                }

                else if (CategoriaSchool.IsChecked == true)
                {
                    CategoriaSeleccionada =
                        "School";
                }

                else if (CategoriaHome.IsChecked == true)
                {
                    CategoriaSeleccionada =
                        "Home";
                }

                else if (CategoriaShopping.IsChecked == true)
                {
                    CategoriaSeleccionada =
                        "Shopping";
                }

                else if (CategoriaPersonal.IsChecked == true)
                {
                    CategoriaSeleccionada =
                        "Personal";
                }

                else if (CategoriaHobbies.IsChecked == true)
                {
                    CategoriaSeleccionada =
                        "Hobbies";
                }

                else if (CategoriaHealth.IsChecked == true)
                {
                    CategoriaSeleccionada =
                        "Health";
                }

                else if (CategoriaOther.IsChecked == true)
                {
                    CategoriaSeleccionada =
                        "Other";
                }
            }


            // =====================================================
            // COLOR DE LA TAREA
            // =====================================================

            RadioButton[] colores = ObtenerRadiosColor();


            foreach (RadioButton radio in colores)
            {
                if (radio.IsChecked == true &&
                    radio.Background is SolidColorBrush brush)
                {
                    ColorTarea =
                        brush.Color.ToString();

                    break;
                }
            }


            // =====================================================
            // GOOGLE DRIVE
            // =====================================================

            DriveActivo =
                SwitchDrive.IsChecked == true;


            // =====================================================
            // CREAR / RENOMBRAR LA CARPETA DE LA TAREA
            // =====================================================

            if (DriveActivo &&
                _driveService != null)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(
                            DriveFolderId))
                    {
                        Google.Apis.Drive.v3.Data.File carpetaTarea =
                            await GoogleDriveService
                                .CrearCarpetaTareaAsync(
                                    _driveService,
                                    TituloTarea
                                );


                        DriveFolderId =
                            carpetaTarea.Id
                            ?? "";
                    }
                    else
                    {
                        await GoogleDriveService
                            .RenombrarCarpetaTareaAsync(
                                _driveService,
                                DriveFolderId,
                                TituloTarea
                            );
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "La tarea no se pudo guardar porque hubo un problema con su carpeta de Google Drive.\n\n" +
                        ex.Message,
                        "Google Drive",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    return;
                }
            }


            ArchivosDrive =
                _archivosDrive
                    .Select(
                        archivo =>
                            new ArchivoDriveModel
                            {
                                Id =
                                    archivo.Id
                                    ?? "",

                                Nombre =
                                    archivo.Name
                                    ?? "",

                                Enlace =
                                    archivo.WebViewLink
                                    ?? "",

                                MimeType =
                                    archivo.MimeType
                                    ?? ""
                            }
                    )
                    .ToList();


            // =====================================================
            // TERMINAR
            // =====================================================

            DialogResult = true;

            Close();
        }
    }
}
