using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
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
        // CARPETA DONDE GOOGLE GUARDARÁ LOS TOKENS
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
        // PERMISOS SOLICITADOS
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
            return File.Exists(
                RutaCredenciales
            );
        }


        // =========================================================
        // CONECTAR CON GOOGLE DRIVE
        // =========================================================

        public static async Task<DriveService> ConectarAsync()
        {
            // =====================================================
            // COMPROBAR QUE EXISTA credentials.json
            // =====================================================

            if (!File.Exists(
                    RutaCredenciales))
            {
                throw new FileNotFoundException(
                    "No se encontró el archivo de credenciales de Google.",
                    RutaCredenciales
                );
            }


            // =====================================================
            // CREAR CARPETA PARA LOS TOKENS
            // =====================================================

            if (!Directory.Exists(
                    CarpetaTokens))
            {
                Directory.CreateDirectory(
                    CarpetaTokens
                );
            }


            // =====================================================
            // LEER CREDENCIALES
            // =====================================================

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


            // =====================================================
            // AUTORIZAR AL USUARIO
            // =====================================================

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


            // =====================================================
            // CREAR SERVICIO DE GOOGLE DRIVE
            // =====================================================

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
    }
}