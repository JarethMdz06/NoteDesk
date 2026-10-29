using System;
using System.Windows;
using System.Windows.Media;

namespace NoteDesk
{
    /// <summary>
    /// Lógica de interacción para ajustes.xaml
    /// </summary>
    public partial class ajustes : Window
    {
        public bool ModoOscuroActivo { get; private set; }
        public bool RecordatoriosActivos { get; private set; }
        public bool SonidoActivo { get; private set; }

        public ajustes()
        {
            InitializeComponent();

            // Sincronizamos el CheckBox con lo que esté guardado en el archivo del sistema
            ChkModoOscuro.IsChecked = App.CargarEstadoModoOscuro();
        }

        private void ChkModoOscuro_Checked(object sender, RoutedEventArgs e)
        {
            ModoOscuroActivo = true;

            // 1. Guardamos la preferencia de forma permanente
            App.GuardarEstadoModoOscuro(true);

            // 2. Aplicamos los colores de noche / pastel suave
            App.AplicarTemaOscuroGlobal();
        }

        private void ChkModoOscuro_Unchecked(object sender, RoutedEventArgs e)
        {
            ModoOscuroActivo = false;

            // 1. Guardamos que se desactivó el modo
            App.GuardarEstadoModoOscuro(false);

            // 2. Revertimos a la paleta pastel clara original
            App.AplicarTemaClaroGlobal();
        }

        // =========================================================
        // RECORDATORIOS DE TAREAS
        // =========================================================
        private void ChkRecordatorios_Checked(object sender, RoutedEventArgs e)
        {
            RecordatoriosActivos = true;
        }

        private void ChkRecordatorios_Unchecked(object sender, RoutedEventArgs e)
        {
            RecordatoriosActivos = false;
        }

        // =========================================================
        // ACTIVAR SONIDO
        // =========================================================
        private void ChkSonido_Checked(object sender, RoutedEventArgs e)
        {
            SonidoActivo = true;
        }

        private void ChkSonido_Unchecked(object sender, RoutedEventArgs e)
        {
            SonidoActivo = false;
        }
    }
}