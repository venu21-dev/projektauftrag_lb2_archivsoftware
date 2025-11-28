using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using System.Linq;

namespace archivsoftware.Business.Services
{
    /// <summary>
    /// Service für DOCX Text-Extraktion mit OpenXML
    /// </summary>
    public class DocxTextExtractor
    {
        /// <summary>
        /// Extrahiert Text aus einem DOCX-Dokument (byte array)
        /// </summary>
        /// <param name="docxData">DOCX als byte array</param>
        /// <returns>Extrahierter Plaintext</returns>
        public string ExtractText(byte[] docxData)
        {
            if (docxData == null || docxData.Length == 0)
            {
                throw new ArgumentException("DOCX-Daten dürfen nicht leer sein.", nameof(docxData));
            }

            try
            {
                using (var memoryStream = new MemoryStream(docxData))
                using (var wordDocument = WordprocessingDocument.Open(memoryStream, false))
                {
                    if (wordDocument.MainDocumentPart == null)
                    {
                        throw new InvalidOperationException("DOCX enthält keinen MainDocumentPart.");
                    }

                    var body = wordDocument.MainDocumentPart.Document.Body;

                    if (body == null)
                    {
                        return string.Empty;
                    }

                    // Text aus allen Paragraphen extrahieren
                    var textBuilder = new StringBuilder();

                    foreach (var paragraph in body.Elements<Paragraph>())
                    {
                        var paragraphText = GetParagraphText(paragraph);
                        if (!string.IsNullOrWhiteSpace(paragraphText))
                        {
                            textBuilder.AppendLine(paragraphText);
                        }
                    }

                    // Text aus Tabellen extrahieren
                    foreach (var table in body.Elements<Table>())
                    {
                        var tableText = GetTableText(table);
                        if (!string.IsNullOrWhiteSpace(tableText))
                        {
                            textBuilder.AppendLine(tableText);
                        }
                    }

                    return textBuilder.ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Fehler beim Extrahieren des DOCX-Textes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extrahiert Text aus einer DOCX-Datei (Dateipfad)
        /// </summary>
        /// <param name="filePath">Pfad zur DOCX-Datei</param>
        /// <returns>Extrahierter Plaintext</returns>
        public string ExtractTextFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("DOCX-Datei nicht gefunden.", filePath);
            }

            byte[] docxData = File.ReadAllBytes(filePath);
            return ExtractText(docxData);
        }

        /// <summary>
        /// Prüft ob eine Datei ein gültiges DOCX ist
        /// </summary>
        /// <param name="filePath">Pfad zur Datei</param>
        /// <returns>True wenn gültiges DOCX</returns>
        public bool IsValidDocx(string filePath)
        {
            try
            {
                using (var wordDocument = WordprocessingDocument.Open(filePath, false))
                {
                    return wordDocument.MainDocumentPart != null;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Prüft ob DOCX-Daten gültig sind
        /// </summary>
        /// <param name="docxData">DOCX als byte array</param>
        /// <returns>True wenn gültiges DOCX</returns>
        public bool IsValidDocx(byte[] docxData)
        {
            try
            {
                using (var memoryStream = new MemoryStream(docxData))
                using (var wordDocument = WordprocessingDocument.Open(memoryStream, false))
                {
                    return wordDocument.MainDocumentPart != null;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extrahiert Text aus einem Paragraph
        /// </summary>
        private string GetParagraphText(Paragraph paragraph)
        {
            var textBuilder = new StringBuilder();

            foreach (var text in paragraph.Descendants<Text>())
            {
                textBuilder.Append(text.Text);
            }

            return textBuilder.ToString();
        }

        /// <summary>
        /// Extrahiert Text aus einer Tabelle
        /// </summary>
        private string GetTableText(Table table)
        {
            var textBuilder = new StringBuilder();

            foreach (var row in table.Elements<TableRow>())
            {
                foreach (var cell in row.Elements<TableCell>())
                {
                    foreach (var paragraph in cell.Elements<Paragraph>())
                    {
                        var cellText = GetParagraphText(paragraph);
                        if (!string.IsNullOrWhiteSpace(cellText))
                        {
                            textBuilder.Append(cellText);
                            textBuilder.Append("\t"); // Tab zwischen Zellen
                        }
                    }
                }
                textBuilder.AppendLine(); // Neue Zeile nach jeder Tabellenzeile
            }

            return textBuilder.ToString();
        }
    }
}
