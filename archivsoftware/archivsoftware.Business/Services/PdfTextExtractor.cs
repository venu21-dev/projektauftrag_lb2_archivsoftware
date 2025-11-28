using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.IO;
using System.Text;
using iTextPdfTextExtractor = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor;

namespace archivsoftware.Business.Services
{
    /// <summary>
    /// Service für PDF Text-Extraktion mit iText7
    /// </summary>
    public class PdfTextExtractor
    {
        /// <summary>
        /// Extrahiert Text aus einem PDF-Dokument (byte array)
        /// </summary>
        /// <param name="pdfData">PDF als byte array</param>
        /// <returns>Extrahierter Plaintext</returns>
        public string ExtractText(byte[] pdfData)
        {
            if (pdfData == null || pdfData.Length == 0)
            {
                throw new ArgumentException("PDF-Daten dürfen nicht leer sein.", nameof(pdfData));
            }

            try
            {
                var extractedText = new StringBuilder();

                using (var memoryStream = new MemoryStream(pdfData))
                using (var pdfReader = new PdfReader(memoryStream))
                using (var pdfDocument = new PdfDocument(pdfReader))
                {
                    // Über alle Seiten iterieren
                    for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
                    {
                        var pdfPage = pdfDocument.GetPage(page);

                        // Text-Extraktion mit LocationTextExtractionStrategy
                        var strategy = new LocationTextExtractionStrategy();
                        string pageText = iTextPdfTextExtractor.GetTextFromPage(pdfPage, strategy);

                        extractedText.AppendLine(pageText);
                        extractedText.AppendLine(); // Leerzeile zwischen Seiten
                    }
                }

                return extractedText.ToString().Trim();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Fehler beim Extrahieren des PDF-Textes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extrahiert Text aus einer PDF-Datei (Dateipfad)
        /// </summary>
        /// <param name="filePath">Pfad zur PDF-Datei</param>
        /// <returns>Extrahierter Plaintext</returns>
        public string ExtractTextFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("PDF-Datei nicht gefunden.", filePath);
            }

            byte[] pdfData = File.ReadAllBytes(filePath);
            return ExtractText(pdfData);
        }

        /// <summary>
        /// Prüft ob eine Datei ein gültiges PDF ist
        /// </summary>
        /// <param name="filePath">Pfad zur Datei</param>
        /// <returns>True wenn gültiges PDF</returns>
        public bool IsValidPdf(string filePath)
        {
            try
            {
                using (var pdfReader = new PdfReader(filePath))
                using (var pdfDocument = new PdfDocument(pdfReader))
                {
                    return pdfDocument.GetNumberOfPages() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Prüft ob PDF-Daten gültig sind
        /// </summary>
        /// <param name="pdfData">PDF als byte array</param>
        /// <returns>True wenn gültiges PDF</returns>
        public bool IsValidPdf(byte[] pdfData)
        {
            try
            {
                using (var memoryStream = new MemoryStream(pdfData))
                using (var pdfReader = new PdfReader(memoryStream))
                using (var pdfDocument = new PdfDocument(pdfReader))
                {
                    return pdfDocument.GetNumberOfPages() > 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}