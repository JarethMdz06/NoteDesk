using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace NoteDesk
{
    public static class TareaStorage
    {
        // =====================================================
        // CARPETA DE NOTEDESK
        // =====================================================

        private static readonly string CarpetaNoteDesk =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData
                ),
                "NoteDesk"
            );


        // =====================================================
        // ARCHIVO JSON
        // =====================================================

        private static readonly string ArchivoTareas =
            Path.Combine(
                CarpetaNoteDesk,
                "tareas.json"
            );


        // =====================================================
        // GUARDAR
        // =====================================================

        public static void Guardar(
            List<TareaModel> tareas)
        {
            try
            {
                // Crear carpeta si no existe.

                if (!Directory.Exists(CarpetaNoteDesk))
                {
                    Directory.CreateDirectory(
                        CarpetaNoteDesk
                    );
                }


                // Formato del JSON.

                JsonSerializerOptions opciones =
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };


                // Convertir lista a JSON.

                string json =
                    JsonSerializer.Serialize(
                        tareas,
                        opciones
                    );


                // Guardar archivo.

                File.WriteAllText(
                    ArchivoTareas,
                    json
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron guardar las tareas.\n\n" +
                    ex.Message,
                    "NoteDesk",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        // =====================================================
        // CARGAR
        // =====================================================

        public static List<TareaModel> Cargar()
        {
            try
            {
                // Si todavía no existe el JSON,
                // regresamos una lista vacía.

                if (!File.Exists(ArchivoTareas))
                {
                    return new List<TareaModel>();
                }


                // Leer archivo.

                string json =
                    File.ReadAllText(
                        ArchivoTareas
                    );


                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<TareaModel>();
                }


                // Convertir JSON nuevamente en lista.

                List<TareaModel>? tareas =
                    JsonSerializer.Deserialize<
                        List<TareaModel>
                    >(json);


                return tareas
                    ?? new List<TareaModel>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron cargar las tareas.\n\n" +
                    ex.Message,
                    "NoteDesk",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );


                return new List<TareaModel>();
            }
        }


        // =====================================================
        // RUTA DEL JSON
        // =====================================================

        public static string ObtenerRuta()
        {
            return ArchivoTareas;
        }
    }
}
