using System;
using System.Collections.Generic;

namespace NoteDesk
{
    public class TareaModel
    {
        // =====================================================
        // DATOS GENERALES
        // =====================================================

        public string Titulo { get; set; } = "";

        public string Descripcion { get; set; } = "";

        public string Fecha { get; set; } = "";

        public string Color { get; set; } = "#F7C84B";

        public string Prioridad { get; set; } = "";

        public string Categoria { get; set; } = "";

        public bool Completada { get; set; }

        public List<SubtareaModel> Subtareas { get; set; } = new List<SubtareaModel>();

        public DateTime UltimoAvanceSubtareasUtc { get; set; } = DateTime.UtcNow;

        public DateTime? UltimoRecordatorioSubtareasUtc { get; set; }


        // =====================================================
        // GOOGLE DRIVE
        // =====================================================

        public bool DriveActivo { get; set; } = false;


        // ID de la carpeta específica de esta tarea.
        //
        // Ejemplo:
        //
        // NoteDesk
        // └── Prueba1   <- este ID
        //
        public string DriveFolderId { get; set; } = "";


        public List<ArchivoDriveModel> ArchivosDrive { get; set; }
            = new List<ArchivoDriveModel>();
    }


    public class SubtareaModel
    {
        public string Titulo { get; set; } = "";

        public bool Completada { get; set; }
    }


    // =========================================================
    // ARCHIVO VINCULADO DE GOOGLE DRIVE
    // =========================================================

    public class ArchivoDriveModel
    {
        public string Id { get; set; } = "";

        public string Nombre { get; set; } = "";

        public string Enlace { get; set; } = "";

        public string MimeType { get; set; } = "";
    }
}