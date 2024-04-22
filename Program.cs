using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace HelloPdf {
    partial class Program {
        static void Main(string[] args) {
            using var reader = new PdfReader("/Users/vladdr/sandbox/PowerShell/pdf_parser/#2518 Group 10 JC Ledger.pdf");
            using var pdfDoc = new PdfDocument(reader);

            for (int page = 1; page <= pdfDoc.GetNumberOfPages(); ++page)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                string pageContent = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);

                ProcessPdfPage(pageContent);
            }
        }

        static void ProcessPdfPage(string pageContent) {
            using var sr = new StringReader(pageContent);
            if (sr == null) {
                return;
            }
            
            string line;
            while ((line = sr.ReadLine()??String.Empty) != null) {
                string amount = ExtractAmount(line);
                if (!String.IsNullOrEmpty(amount)) {
                    Console.WriteLine("{0}\t{1}", line, amount);
                }
            }
        }

        static string ExtractAmount(string line) {
            
            if (string.IsNullOrEmpty(line)) {
                return string.Empty;
            }

            var items = line.Split(" ");
            if (items.Length < 2) {
                return string.Empty;
            }

            if (items[^2].EndsWith("Total")) {
                return string.Empty;
            }

            string sum_str = items[^1];

            if (sum_str.StartsWith("(") && sum_str.EndsWith(")")) {
                string just_number = sum_str[1..^1];
                if (IsValidNumber(just_number)) {
                    return sum_str;
                }
            }

            if (IsValidNumber(sum_str)) {
                return sum_str;
            }

            return string.Empty;
        }

        static bool IsValidNumber(string str) {
            Match match = NumericSumRegex().Match(str);
            return match.Success && match.Value.Length == str.Length;
        }

        [GeneratedRegex("\\d+(,\\d{3})*\\.(\\d{1,2})", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex NumericSumRegex();
    }
}