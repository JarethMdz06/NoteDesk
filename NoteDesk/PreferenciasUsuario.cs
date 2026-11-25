using System;
using System.IO;
using System.Media;
using System.Text.Json;
using System.Windows;

namespace NoteDesk
{
    public sealed class PreferenciasUsuario
    {
        public bool RecordatoriosActivos { get; set; } = true;

        public bool SonidoActivo { get; set; }

        public string SonidoSeleccionado { get; set; } = SonidoNotificaciones.SonidoPredeterminado;
    }

    public static class PreferenciasUsuarioStorage
    {
        private static readonly string RutaPreferencias = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NoteDesk",
            "preferencias.json");

        public static PreferenciasUsuario Cargar()
        {
            if (!File.Exists(RutaPreferencias))
            {
                return new PreferenciasUsuario();
            }

            try
            {
                string json = File.ReadAllText(RutaPreferencias);
                PreferenciasUsuario preferencias =
                    JsonSerializer.Deserialize<PreferenciasUsuario>(json)
                    ?? new PreferenciasUsuario();

                if (!SonidoNotificaciones.EsValido(preferencias.SonidoSeleccionado))
                {
                    bool sonidoAnterior = SonidoNotificaciones.EsSonidoAnterior(
                        preferencias.SonidoSeleccionado);
                    preferencias.SonidoSeleccionado = SonidoNotificaciones.SonidoPredeterminado;

                    if (!sonidoAnterior)
                    {
                        MessageBox.Show(
                            "El sonido guardado no es válido. Se seleccionó la campana de NoteDesk.",
                            "NoteDesk",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }

                return preferencias;
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is JsonException)
            {
                MessageBox.Show(
                    "No se pudieron cargar las preferencias de notificaciones. Se usarán los valores predeterminados.\n\n" +
                    ex.Message,
                    "NoteDesk",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return new PreferenciasUsuario();
            }
        }

        public static bool Guardar(PreferenciasUsuario preferencias, out string error)
        {
            try
            {
                string? carpeta = Path.GetDirectoryName(RutaPreferencias);
                if (!string.IsNullOrEmpty(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }

                string json = JsonSerializer.Serialize(
                    preferencias,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(RutaPreferencias, json);
                error = string.Empty;
                return true;
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException)
            {
                error = ex.Message;
                return false;
            }
        }
    }

    public static class SonidoNotificaciones
    {
        public const string SonidoPredeterminado = "NoteDeskBell";

        private const string RutaRecurso =
            "pack://application:,,,/Assets/Aud/NoteDesk_Notificacion_Campana.wav";

        private static Stream? _stream;
        private static SoundPlayer? _reproductor;

        public static bool EsValido(string? sonido)
        {
            return sonido == SonidoPredeterminado;
        }

        public static bool EsSonidoAnterior(string? sonido)
        {
            return sonido is "Beep" or "Exclamation" or "Question" or "Hand" or "Asterisk";
        }

        public static void Reproducir(string sonido)
        {
            if (!EsValido(sonido))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sonido),
                    sonido,
                    "El sonido de notificación seleccionado no es válido.");
            }

            if (_reproductor == null)
            {
                Stream stream = Application.GetResourceStream(new Uri(RutaRecurso))?.Stream
                    ?? throw new FileNotFoundException(
                        "No se encontró el sonido personalizado de NoteDesk.",
                        RutaRecurso);

                SoundPlayer reproductor = new SoundPlayer(stream);
                reproductor.Load();
                _stream = stream;
                _reproductor = reproductor;
            }

            _reproductor.Play();
        }
    }
}
