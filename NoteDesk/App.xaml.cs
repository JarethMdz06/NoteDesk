using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace NoteDesk
{
    public partial class App : Application
    {
        // Ruta donde se guardará la preferencia del usuario en su computadora
        private static readonly string RutaConfig = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NoteDesk_ModoOscuro.txt");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Al iniciar la app, revisamos si el archivo dice que estaba activo
            if (CargarEstadoModoOscuro())
            {
                AplicarTemaOscuroGlobal();
            }
        }

        // Métodos auxiliares para leer y guardar sin errores
        public static bool CargarEstadoModoOscuro()
        {
            try
            {
                if (File.Exists(RutaConfig))
                {
                    string contenido = File.ReadAllText(RutaConfig);
                    return contenido.Trim() == "True";
                }
            }
            catch { }
            return false;
        }

        public static void GuardarEstadoModoOscuro(bool activo)
        {
            try
            {
                File.WriteAllText(RutaConfig, activo.ToString());
            }
            catch { }
        }

        public static void AplicarTemaOscuroGlobal()
        {
            Current.Resources["ColorVentana"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C8BFAB"));
            Current.Resources["ColorTarjeta"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDB4A0"));
            Current.Resources["ColorTarjetaInterna"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D8CFBC"));
            Current.Resources["ColorHeader"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1A22C"));
            Current.Resources["ColorTexto"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222"));
            Current.Resources["ColorTextoHeader"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111111"));
            Current.Resources["ColorTextoSecundario"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555046"));
            Current.Resources["ColorSuperficie"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C8BFAB"));
            Current.Resources["ColorBorde"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#938A77"));
            Current.Resources["ColorFondoDias"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C4BAA5"));
            Current.Resources["ColorDiasOtroMes"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#716958"));
            Current.Resources["ColorFondoPopup"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D8CFBC"));
            Current.Resources["ColorMesSeleccionado"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B28A2E"));
            Current.Resources["ColorBordeMesSeleccionado"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B7420"));
            Current.Resources["ColorBarraNavegacion"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B89A3D"));
            Current.Resources["ColorNavegacionActiva"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A88F59"));
        }

        public static void AplicarTemaClaroGlobal()
        {
            Current.Resources["ColorVentana"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EBE2CD"));
            Current.Resources["ColorTarjeta"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECE3D0"));
            Current.Resources["ColorTarjetaInterna"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F4EA"));
            Current.Resources["ColorHeader"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EBB83A"));
            Current.Resources["ColorTexto"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222"));
            Current.Resources["ColorTextoHeader"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111111"));
            Current.Resources["ColorTextoSecundario"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777"));
            Current.Resources["ColorSuperficie"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFDF7"));
            Current.Resources["ColorBorde"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DED4BD"));
            Current.Resources["ColorFondoDias"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F0E6"));
            Current.Resources["ColorDiasOtroMes"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));
            Current.Resources["ColorFondoPopup"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4EEE0"));
            Current.Resources["ColorMesSeleccionado"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF2C3"));
            Current.Resources["ColorBordeMesSeleccionado"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8B32D"));
            Current.Resources["ColorBarraNavegacion"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E7CD63"));
            Current.Resources["ColorNavegacionActiva"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF2C3"));
        }
    }
}