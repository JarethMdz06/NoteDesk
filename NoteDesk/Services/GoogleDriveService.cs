using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NoteDesk.Services
{
    public static class GoogleDriveService
    {
        // =========================================================
        // NOMBRE DE LA APLICACIÓN
        // =========================================================

        private const string NombreAplicacion =
            "NoteDesk";


        // =========================================================
        // CARPETA PRINCIPAL EN GOOGLE DRIVE
        // =========================================================

        private const string NombreCarpetaDrive =
            "NoteDesk";


        private const string MimeTypeCarpeta =
            "application/vnd.google-apps.folder";


        // =========================================================
        // RUTA DE CREDENCIALES OAUTH
        // =========================================================

        private static readonly string RutaCredenciales =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData
                ),
                "NoteDesk",
                "Google",
                "credentials.json"
            );


        // =========================================================
        // CARPETA DONDE GOOGLE GUARDA LOS TOKENS
        // =========================================================

        private static readonly string CarpetaTokens =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData
                ),
                "NoteDesk",
                "Google",
                "Tokens"
            );


        // =========================================================
        // PERMISOS
        // =========================================================

        private static readonly string[] Scopes =
        {
            DriveService.Scope.DriveFile
        };


        // =========================================================
        // VERIFICAR CREDENCIALES
        // =========================================================

        public static bool CredencialesDisponibles()
        {
            return System.IO.File.Exists(
                RutaCredenciales
            );
        }


        // =========================================================
        // CONECTAR CON GOOGLE DRIVE
        // =========================================================

        public static async Task<DriveService> ConectarAsync()
        {
            if (!System.IO.File.Exists(
                    RutaCredenciales))
            {
                throw new FileNotFoundException(
                    "No se encontró el archivo de credenciales de Google.",
                    RutaCredenciales
                );
            }


            if (!Directory.Exists(
                    CarpetaTokens))
            {
                Directory.CreateDirectory(
                    CarpetaTokens
                );
            }


            GoogleClientSecrets secretos;


            using (FileStream stream =
                new FileStream(
                    RutaCredenciales,
                    FileMode.Open,
                    FileAccess.Read
                ))
            {
                secretos =
                    GoogleClientSecrets
                        .FromStream(stream);
            }


            UserCredential credencial =
                await GoogleWebAuthorizationBroker
                    .AuthorizeAsync(
                        secretos.Secrets,
                        Scopes,
                        "usuario_notedesk",
                        CancellationToken.None,
                        new FileDataStore(
                            CarpetaTokens,
                            true
                        )
                    );


            DriveService servicio =
                new DriveService(
                    new BaseClientService.Initializer
                    {
                        HttpClientInitializer =
                            credencial,

                        ApplicationName =
                            NombreAplicacion
                    }
                );


            return servicio;
        }


        // =========================================================
        // OBTENER O CREAR CARPETA PRINCIPAL NOTEDESK
        // =========================================================

        private static async Task<string>
            ObtenerOCrearCarpetaNoteDeskAsync(
                DriveService servicio)
        {
            if (servicio == null)
            {
                throw new InvalidOperationException(
                    "Google Drive no está conectado."
                );
            }


            FilesResource.ListRequest buscar =
                servicio.Files.List();


            buscar.Q =
                "name = '" +
                EscaparTextoConsulta(
                    NombreCarpetaDrive
                ) +
                "' and mimeType = '" +
                MimeTypeCarpeta +
                "' and trashed = false";


            buscar.Spaces =
                "drive";


            buscar.Fields =
                "files(id,name,mimeType)";


            Google.Apis.Drive.v3.Data.FileList resultado =
                await buscar.ExecuteAsync();


            if (resultado.Files != null &&
                resultado.Files.Count > 0)
            {
                string? idCarpetaExistente =
                    resultado.Files[0].Id;


                if (!string.IsNullOrWhiteSpace(
                        idCarpetaExistente))
                {
                    return idCarpetaExistente;
                }
            }


            Google.Apis.Drive.v3.Data.File nuevaCarpeta =
                new Google.Apis.Drive.v3.Data.File
                {
                    Name =
                        NombreCarpetaDrive,

                    MimeType =
                        MimeTypeCarpeta
                };


            FilesResource.CreateRequest crearCarpeta =
                servicio.Files.Create(
                    nuevaCarpeta
                );


            crearCarpeta.Fields =
                "id,name,mimeType,webViewLink";


            Google.Apis.Drive.v3.Data.File carpetaCreada =
                await crearCarpeta.ExecuteAsync();


            if (string.IsNullOrWhiteSpace(
                    carpetaCreada.Id))
            {
                throw new Exception(
                    "Google Drive creó la carpeta NoteDesk, pero no devolvió su identificador."
                );
            }


            return carpetaCreada.Id;
        }


        // =========================================================
        // CREAR CARPETA PARA UNA TAREA
        // =========================================================
        //
        // Estructura:
        //
        // Mi unidad
        // └── NoteDesk
        //     └── Nombre de la tarea
        //
        // Cada tarea recibe su propio ID de carpeta.
        // Aunque existan dos tareas con el mismo título,
        // cada una puede conservar una carpeta diferente.
        //
        // =========================================================

        public static async Task<
            Google.Apis.Drive.v3.Data.File>
            CrearCarpetaTareaAsync(
                DriveService servicio,
                string nombreTarea)
        {
            if (servicio == null)
            {
                throw new InvalidOperationException(
                    "Google Drive no está conectado."
                );
            }


            string nombreCarpeta =
                NormalizarNombreCarpeta(
                    nombreTarea
                );


            string idCarpetaNoteDesk =
                await ObtenerOCrearCarpetaNoteDeskAsync(
                    servicio
                );


            Google.Apis.Drive.v3.Data.File metadata =
                new Google.Apis.Drive.v3.Data.File
                {
                    Name =
                        nombreCarpeta,

                    MimeType =
                        MimeTypeCarpeta,

                    Parents =
                        new List<string>
                        {
                            idCarpetaNoteDesk
                        }
                };


            FilesResource.CreateRequest request =
                servicio.Files.Create(
                    metadata
                );


            request.Fields =
                "id,name,mimeType,webViewLink,parents";


            Google.Apis.Drive.v3.Data.File carpetaCreada =
                await request.ExecuteAsync();


            if (string.IsNullOrWhiteSpace(
                    carpetaCreada.Id))
            {
                throw new Exception(
                    "Google Drive creó la carpeta de la tarea, pero no devolvió su identificador."
                );
            }


            return carpetaCreada;
        }


        // =========================================================
        // RENOMBRAR CARPETA DE UNA TAREA
        // =========================================================

        public static async Task RenombrarCarpetaTareaAsync(
            DriveService servicio,
            string idCarpetaTarea,
            string nuevoNombre)
        {
            if (servicio == null)
            {
                throw new InvalidOperationException(
                    "Google Drive no está conectado."
                );
            }


            if (string.IsNullOrWhiteSpace(
                    idCarpetaTarea))
            {
                return;
            }


            Google.Apis.Drive.v3.Data.File cambios =
                new Google.Apis.Drive.v3.Data.File
                {
                    Name =
                        NormalizarNombreCarpeta(
                            nuevoNombre
                        )
                };


            FilesResource.UpdateRequest request =
                servicio.Files.Update(
                    cambios,
                    idCarpetaTarea
                );


            request.Fields =
                "id,name,mimeType,webViewLink,parents";


            await request.ExecuteAsync();
        }


        // =========================================================
        // OBTENER ENLACE DE LA CARPETA DE UNA TAREA
        // =========================================================

        public static string ObtenerEnlaceCarpetaTarea(
            string idCarpetaTarea)
        {
            if (string.IsNullOrWhiteSpace(
                    idCarpetaTarea))
            {
                return "";
            }


            return
                "https://drive.google.com/drive/folders/" +
                idCarpetaTarea;
        }


        // =========================================================
        // LISTAR ARCHIVOS DE UNA CARPETA DE TAREA
        // =========================================================

        public static async Task<
            IList<Google.Apis.Drive.v3.Data.File>>
            ListarArchivosCarpetaTareaAsync(
                DriveService servicio,
                string idCarpetaTarea)
        {
            if (servicio == null)
            {
                throw new InvalidOperationException(
                    "Google Drive no está conectado."
                );
            }


            if (string.IsNullOrWhiteSpace(
                    idCarpetaTarea))
            {
                return new List<
                    Google.Apis.Drive.v3.Data.File>();
            }


            FilesResource.ListRequest request =
                servicio.Files.List();


            request.Q =
                "'" +
                idCarpetaTarea +
                "' in parents and trashed = false";


            request.Spaces =
                "drive";


            request.OrderBy =
                "modifiedTime desc";


            request.Fields =
                "files(id,name,mimeType,size,webViewLink,modifiedTime,parents)";


            Google.Apis.Drive.v3.Data.FileList resultado =
                await request.ExecuteAsync();


            return resultado.Files
                ?? new List<
                    Google.Apis.Drive.v3.Data.File>();
        }


        // =========================================================
        // LISTAR ARCHIVOS DE LA CARPETA PRINCIPAL NOTEDESK
        // =========================================================
        //
        // Se conserva temporalmente para no romper el botón
        // "Buscar en Drive" actual. Más adelante ese botón
        // será reemplazado por "Abrir carpeta".
        //
        // =========================================================

        public static async Task<
            IList<Google.Apis.Drive.v3.Data.File>>
            ListarArchivosNoteDeskAsync(
                DriveService servicio)
        {
            if (servicio == null)
            {
                throw new InvalidOperationException(
                    "Google Drive no está conectado."
                );
            }


            string idCarpeta =
                await ObtenerOCrearCarpetaNoteDeskAsync(
                    servicio
                );


            FilesResource.ListRequest request =
                servicio.Files.List();


            request.Q =
                "'" +
                idCarpeta +
                "' in parents and trashed = false";


            request.Spaces =
                "drive";


            request.OrderBy =
                "modifiedTime desc";


            request.Fields =
                "files(id,name,mimeType,size,webViewLink,modifiedTime,parents)";


            Google.Apis.Drive.v3.Data.FileList resultado =
                await request.ExecuteAsync();


            return resultado.Files
                ?? new List<
                    Google.Apis.Drive.v3.Data.File>();
        }


        // =========================================================
        // SUBIR ARCHIVO A LA CARPETA DE UNA TAREA
        // =========================================================

        public static async Task<
            Google.Apis.Drive.v3.Data.File>
            SubirArchivoAsync(
                DriveService servicio,
                string rutaArchivo,
                string idCarpetaTarea)
        {
            if (servicio == null)
            {
                throw new InvalidOperationException(
                    "Google Drive no está conectado."
                );
            }


            if (string.IsNullOrWhiteSpace(
                    idCarpetaTarea))
            {
                throw new InvalidOperationException(
                    "La tarea todavía no tiene una carpeta de Google Drive."
                );
            }


            return await SubirArchivoEnCarpetaAsync(
                servicio,
                rutaArchivo,
                idCarpetaTarea
            );
        }


        // =========================================================
        // SUBIR ARCHIVO - COMPATIBILIDAD TEMPORAL
        // =========================================================
        //
        // Este método mantiene funcionando el NewList actual.
        // Todavía sube directamente a /NoteDesk.
        //
        // En el siguiente paso NewList empezará a usar el método
        // anterior con DriveFolderId y esta compatibilidad dejará
        // de ser necesaria.
        //
        // =========================================================

        public static async Task<
            Google.Apis.Drive.v3.Data.File>
            SubirArchivoAsync(
                DriveService servicio,
                string rutaArchivo)
        {
            if (servicio == null)
            {
                throw new InvalidOperationException(
                    "Google Drive no está conectado."
                );
            }


            string idCarpetaNoteDesk =
                await ObtenerOCrearCarpetaNoteDeskAsync(
                    servicio
                );


            return await SubirArchivoEnCarpetaAsync(
                servicio,
                rutaArchivo,
                idCarpetaNoteDesk
            );
        }


        // =========================================================
        // SUBIDA INTERNA A UNA CARPETA CONCRETA
        // =========================================================

        private static async Task<
            Google.Apis.Drive.v3.Data.File>
            SubirArchivoEnCarpetaAsync(
                DriveService servicio,
                string rutaArchivo,
                string idCarpetaDestino)
        {
            if (string.IsNullOrWhiteSpace(
                    rutaArchivo) ||
                !System.IO.File.Exists(
                    rutaArchivo))
            {
                throw new FileNotFoundException(
                    "No se encontró el archivo seleccionado.",
                    rutaArchivo
                );
            }


            string nombreArchivo =
                Path.GetFileName(
                    rutaArchivo
                );


            string mimeType =
                ObtenerMimeType(
                    rutaArchivo
                );


            Google.Apis.Drive.v3.Data.File metadata =
                new Google.Apis.Drive.v3.Data.File
                {
                    Name =
                        nombreArchivo,

                    Parents =
                        new List<string>
                        {
                            idCarpetaDestino
                        }
                };


            using FileStream stream =
                new FileStream(
                    rutaArchivo,
                    FileMode.Open,
                    FileAccess.Read
                );


            FilesResource.CreateMediaUpload request =
                servicio.Files.Create(
                    metadata,
                    stream,
                    mimeType
                );


            request.Fields =
                "id,name,mimeType,size,webViewLink,parents";


            IUploadProgress progreso =
                await request.UploadAsync();


            if (progreso.Status !=
                UploadStatus.Completed)
            {
                string mensaje =
                    progreso.Exception?.Message
                    ??
                    "Google Drive no pudo completar la subida.";


                throw new Exception(
                    mensaje
                );
            }


            Google.Apis.Drive.v3.Data.File? archivoSubido =
                request.ResponseBody;


            if (archivoSubido == null)
            {
                throw new Exception(
                    "Google Drive no devolvió información del archivo subido."
                );
            }


            return archivoSubido;
        }


        // =========================================================
        // NORMALIZAR NOMBRE DE CARPETA
        // =========================================================

        private static string NormalizarNombreCarpeta(
            string nombre)
        {
            if (string.IsNullOrWhiteSpace(
                    nombre))
            {
                return "Tarea sin título";
            }


            string limpio =
                nombre.Trim();


            foreach (char caracter in
                Path.GetInvalidFileNameChars())
            {
                limpio =
                    limpio.Replace(
                        caracter,
                        '-'
                    );
            }


            if (string.IsNullOrWhiteSpace(
                    limpio))
            {
                limpio =
                    "Tarea sin título";
            }


            return limpio;
        }


        // =========================================================
        // ESCAPAR TEXTO PARA CONSULTAS DE DRIVE
        // =========================================================

        private static string EscaparTextoConsulta(
            string texto)
        {
            return texto
                .Replace(
                    "\\",
                    "\\\\"
                )
                .Replace(
                    "'",
                    "\\'"
                );
        }


        // =========================================================
        // MIME TYPE
        // =========================================================

        private static string ObtenerMimeType(
            string rutaArchivo)
        {
            string extension =
                Path.GetExtension(
                    rutaArchivo
                )
                .ToLowerInvariant();


            return extension switch
            {
                ".pdf" =>
                    "application/pdf",

                ".doc" =>
                    "application/msword",

                ".docx" =>
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

                ".xls" =>
                    "application/vnd.ms-excel",

                ".xlsx" =>
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",

                ".ppt" =>
                    "application/vnd.ms-powerpoint",

                ".pptx" =>
                    "application/vnd.openxmlformats-officedocument.presentationml.presentation",

                ".txt" =>
                    "text/plain",

                ".csv" =>
                    "text/csv",

                ".json" =>
                    "application/json",

                ".jpg" =>
                    "image/jpeg",

                ".jpeg" =>
                    "image/jpeg",

                ".png" =>
                    "image/png",

                ".gif" =>
                    "image/gif",

                ".webp" =>
                    "image/webp",

                ".zip" =>
                    "application/zip",

                _ =>
                    "application/octet-stream"
            };
        }
    }
}
