using System;
using System.Windows;
using System.Windows.Controls;

namespace NoteDesk
{
    /// <summary>
    /// Lógica de interacción para ajustes.xaml.
    ///
    /// Ahora esta sección vive dentro de MainWindow,
    /// por eso hereda de UserControl y ya no abre/cierra ventanas.
    /// </summary>
    public partial class ajustes : UserControl
    {
        public bool ModoOscuroActivo { get; private set; }

        public bool RecordatoriosActivos { get; private set; }

        public bool SonidoActivo { get; private set; }

        public event Action? NotificacionesActualizadas;

        private readonly PreferenciasUsuario _preferencias;

        private bool _inicializandoPreferencias;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public ajustes(PreferenciasUsuario preferencias)
        {
            InitializeComponent();
            _preferencias = preferencias;
            _inicializandoPreferencias = true;

            ChkModoOscuro.IsChecked = App.CargarEstadoModoOscuro();
            ChkRecordatorios.IsChecked = _preferencias.RecordatoriosActivos;
            ChkSonido.IsChecked = _preferencias.SonidoActivo;
            _preferencias.SonidoSeleccionado = SonidoNotificaciones.SonidoPredeterminado;
            PanelOpcionesSonido.IsEnabled = _preferencias.SonidoActivo;

            ModoOscuroActivo = ChkModoOscuro.IsChecked == true;
            RecordatoriosActivos = ChkRecordatorios.IsChecked == true;
            SonidoActivo = ChkSonido.IsChecked == true;
            _inicializandoPreferencias = false;
        }


        // =========================================================
        // MODO OSCURO
        // =========================================================

        private void ChkModoOscuro_Checked(
            object sender,
            RoutedEventArgs e)
        {
            ModoOscuroActivo =
                true;


            // 1. Guardamos la preferencia de forma permanente

            App.GuardarEstadoModoOscuro(
                true
            );


            // 2. Aplicamos los colores de noche / pastel suave

            App.AplicarTemaOscuroGlobal();
        }


        private void ChkModoOscuro_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            ModoOscuroActivo =
                false;


            // 1. Guardamos que se desactivó el modo

            App.GuardarEstadoModoOscuro(
                false
            );


            // 2. Revertimos a la paleta pastel clara original

            App.AplicarTemaClaroGlobal();
        }


        // =========================================================
        // RECORDATORIOS DE TAREAS
        // =========================================================

        private void ChkRecordatorios_Checked(
            object sender,
            RoutedEventArgs e)
        {
            GuardarPreferenciasNotificaciones();
        }


        private void ChkRecordatorios_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            GuardarPreferenciasNotificaciones();
        }


        // =========================================================
        // ACTIVAR SONIDO
        // =========================================================

        private void ChkSonido_Checked(
            object sender,
            RoutedEventArgs e)
        {
            PanelOpcionesSonido.IsEnabled = true;
            GuardarPreferenciasNotificaciones();
        }


        private void ChkSonido_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            PanelOpcionesSonido.IsEnabled = false;
            GuardarPreferenciasNotificaciones();
        }

        private void BtnProbarSonido_Click(
            object sender,
            RoutedEventArgs e)
        {
            SonidoNotificaciones.Reproducir(SonidoNotificaciones.SonidoPredeterminado);
        }

        private void GuardarPreferenciasNotificaciones()
        {
            if (_inicializandoPreferencias)
            {
                return;
            }

            RecordatoriosActivos = ChkRecordatorios.IsChecked == true;
            SonidoActivo = ChkSonido.IsChecked == true;
            _preferencias.RecordatoriosActivos = RecordatoriosActivos;
            _preferencias.SonidoActivo = SonidoActivo;
            _preferencias.SonidoSeleccionado = SonidoNotificaciones.SonidoPredeterminado;

            if (!PreferenciasUsuarioStorage.Guardar(_preferencias, out string error))
            {
                MessageBox.Show(
                    "No se pudieron guardar las preferencias de notificaciones.\n\n" + error,
                    "NoteDesk",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            NotificacionesActualizadas?.Invoke();
        }
    }
}
