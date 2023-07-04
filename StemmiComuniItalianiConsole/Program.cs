using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StemmiComuniItalianiConsole.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using static StemmiComuniItalianiConsole.Models.WikiSearch;

namespace StemmiComuniItalianiConsole
{
    class Program
    {
        static string _destinationUrl = "";
        static StringBuilder log = new StringBuilder();

        static void Main(string[] args)
        {
            // Inizializzo la stringa di log

            try
            {
                if (args.Count() > 0)
                {
                    string param = args[0];
                    if (param == "-d")
                    {
                        if (args.Count() > 1)
                        {
                            string baseUrl = "https://it.wikipedia.org/w/index.php?title=Special:Redirect/file/File:";
                            string destinazione = args[1];

                            String getStrinFromFile = File.ReadAllText("coordinate.json");
                            var lista = JsonConvert.DeserializeObject<List<Coordinate>>(getStrinFromFile);

                            System.IO.Directory.CreateDirectory(destinazione);
                            System.IO.Directory.CreateDirectory(destinazione + "\\PNG\\");
                            System.IO.Directory.CreateDirectory(destinazione + "\\SVG\\");

                            int i = 0;
                            foreach (var coordinate in lista)
                            {
                                i++;

                                var nomeFile = coordinate.NomeComune + "-Stemma";

                                try
                                {
                                    // DOWNLOAD FORMATO .PNG o .SVG (Se presente)

                                    if (
                                        !File.Exists(destinazione + "\\PNG\\" + nomeFile + ".png") && 
                                        !File.Exists(destinazione + "\\SVG\\" + nomeFile + ".svg") &&
                                        !File.Exists(destinazione + "\\Stemmi ricerca avanzata\\" + coordinate.NomeComune + ".png") &&
                                        !File.Exists(destinazione + "\\Stemmi ricerca avanzata\\" + coordinate.NomeComune + ".svg")
                                        )
                                    {
                                        if (!DowloadImage(i, lista.Count, coordinate.NomeComune, baseUrl, destinazione))
                                        {
                                            // Lo stemma non è stato trovato => ricerca approfondita
                                            coordinate.StemmaTrovato = RicercaAvanzataStemma(destinazione, coordinate);

                                            Console.WriteLine(
                                                coordinate.StemmaTrovato
                                                ? $"Trovato: {nomeFile}"
                                                : $"NON TROVATO: {nomeFile} - {i} di {lista.Count}"
                                            );
                                        }
                                        else
                                        {
                                            coordinate.StemmaTrovato = true;
                                        }
                                    }
                                    else
                                    {
                                        coordinate.StemmaTrovato = true;
                                    }
                                }
                                catch (Exception exc)
                                {
                                    Console.WriteLine($"NON TROVATO: {nomeFile} - {i} di {lista.Count}");
                                }
                            }

                            // Genero una lista di stemmi non trovati
                            List<string> logData = lista.Where(x => !x.StemmaTrovato).Select(x => x.NomeComune).ToList();

                            string filePath = $"{destinazione}\\log.txt"; // Specifica il percorso del file di log

                            using (StreamWriter writer = new StreamWriter(filePath))
                            {
                                foreach (string logEntry in logData)
                                {
                                    writer.WriteLine(logEntry);
                                }
                            }
                        }
                        else
                        {
                            // Parametro non corretto
                            System.Console.WriteLine("Il parametro passato non è corretto. Impostare \"-d\" prima del path.");
                        }
                    }
                    else
                    {
                        // Parametro non corretto
                        System.Console.WriteLine("Il parametro passato non è corretto.");
                    }
                }
                else
                {
                    // Nessun 
                    System.Console.WriteLine(
                        "Non è stato definito un percorso per il salvataggio degli allegati. " +
                        "Impostare il parametro \"-d\" ed il path dove salvare i file."
                    );
                }
            }
            catch (Exception exc)
            {
                System.Console.WriteLine("Errore: errore durante la procedura. Controllare i parametri inseriti.");
            }
        }

        private static bool RicercaAvanzataStemma(string destinazione, Coordinate coordinate)
        {
            try
            {
                var nomeFileNormale = coordinate.NomeComune;
                var url = $"https://en.wikipedia.org/w/api.php?action=query&prop=imageinfo&iiprop=url&generator=images&titles={nomeFileNormale}&format=json";
                var json = new WebClient().DownloadString(url);

                System.IO.Directory.CreateDirectory(destinazione + "\\Stemmi ricerca avanzata");
                var newDest = destinazione + "\\Stemmi ricerca avanzata";
                var risultato = JsonConvert.DeserializeObject<Root>(json);

                var ricerca = risultato.query != null && risultato.query.pages != null ? risultato.query.pages : null;

                if (ricerca != null)
                {
                    var uno = ricerca._1 != null ? ricerca._1 : null;
                    var due = ricerca._2 != null ? ricerca._2 : null;
                    var tre = ricerca._3 != null ? ricerca._3 : null;
                    var quattro = ricerca._4 != null ? ricerca._4 : null;
                    var cinque = ricerca._5 != null ? ricerca._5 : null;

                    if (
                        uno != null &&
                        uno.imageinfo != null &&
                        !uno.title.ToLower().Contains("provincia") &&
                            (
                                uno.title.ToLower().Contains("stemma") ||
                                uno.title.ToLower().Contains("comune di " + nomeFileNormale.ToLower())
                            )
                        )
                    {
                        var urlImmagine = ricerca._1.imageinfo.FirstOrDefault().url;
                        var file = nomeFileNormale + Path.GetExtension(new Uri(urlImmagine).AbsolutePath);

                        return DowloadImageAvanzata(file, urlImmagine, newDest);
                    }
                    else if (
                        due != null &&
                        due.imageinfo != null &&
                        !due.title.ToLower().Contains("provincia") &&
                            (
                                due.title.ToLower().Contains("stemma") ||
                                due.title.ToLower().Contains("comune di " + nomeFileNormale.ToLower())
                            )
                        )
                    {
                        var urlImmagine = due.imageinfo.FirstOrDefault().url;
                        var file = nomeFileNormale + Path.GetExtension(new Uri(urlImmagine).AbsolutePath);

                        return DowloadImageAvanzata(file, urlImmagine, newDest);
                    }
                    else if (
                        tre != null &&
                        tre.imageinfo != null &&
                        !tre.title.ToLower().Contains("provincia") &&
                            (
                                tre.title.ToLower().Contains("stemma") ||
                                tre.title.ToLower().Contains("comune di " + nomeFileNormale.ToLower())
                            )
                        )
                    {
                        var urlImmagine = tre.imageinfo.FirstOrDefault().url;
                        var file = nomeFileNormale + Path.GetExtension(new Uri(urlImmagine).AbsolutePath);

                        return DowloadImageAvanzata(file, urlImmagine, newDest);
                    }
                    else if (
                        quattro != null &&
                        quattro.imageinfo != null &&
                        !quattro.title.ToLower().Contains("provincia") &&
                            (
                                quattro.title.ToLower().Contains("stemma") ||
                                quattro.title.ToLower().Contains("comune di " + nomeFileNormale.ToLower())
                            )
                        )
                    {
                        var urlImmagine = quattro.imageinfo.FirstOrDefault().url;
                        var file = nomeFileNormale + Path.GetExtension(new Uri(urlImmagine).AbsolutePath);

                        return DowloadImageAvanzata(file, urlImmagine, newDest);
                    }
                    else if (
                        cinque != null &&
                        cinque.imageinfo != null &&
                        !cinque.title.ToLower().Contains("provincia") &&
                            (
                                cinque.title.ToLower().Contains("stemma") ||
                                cinque.title.ToLower().Contains("comune di " + nomeFileNormale.ToLower())
                            )
                        )
                    {
                        var urlImmagine = cinque.imageinfo.FirstOrDefault().url;
                        var file = nomeFileNormale + Path.GetExtension(new Uri(urlImmagine).AbsolutePath);

                        return DowloadImageAvanzata(file, urlImmagine, newDest);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        static bool DowloadImage(int numero, int totale, string nomeComune, string baseUrl, string destinazione)
        {
            try
            {
                // imposto la destinazione in base al file
                string destinazionePNG = destinazione + "\\PNG";
                string destinazioneSVG = destinazione + "\\SVG";

                // provo a verificare se è presente l'immagine .png
                var nomeFileEscape = HttpUtility.UrlEncode(nomeComune);

                // file png
                var pngUrl = baseUrl + nomeFileEscape + "-Stemma.png";
                var pngUrl_Tratto = pngUrl.Replace("+", "-");
                var pngUrl_Underscore = pngUrl.Replace("+", "_");

                // file svg
                var svgUrl = baseUrl + nomeFileEscape + "-Stemma.svg";
                var svgUrl_Tratto = svgUrl.Replace("+", "-");
                var svgUrl_Underscore = svgUrl.Replace("+", "_");

                // file con (italia)
                var pngItaliaUrl = baseUrl + nomeFileEscape + " (Italia)-Stemma.png";
                var svgItaliaUrl = baseUrl + nomeFileEscape + " (Italia)-Stemma.svg";

                // prima provo a vedere se ci sono stemmi con estensione .svg, altrimenti cerco i png
                if (EsisteImmagine(svgUrl))
                {
                    ScaricaImmagine(svgUrl, destinazioneSVG, $"{nomeComune}-Stemma", "svg");

                    Console.WriteLine($"Trovato: {nomeComune}-Stemma.svg - {numero} di {totale}");
                    return true;
                }
                else if (EsisteImmagine(svgUrl_Tratto))
                {
                    ScaricaImmagine(svgUrl_Tratto, destinazioneSVG, $"{nomeComune}-Stemma", "svg");

                    Console.WriteLine($"Trovato: {nomeComune}-Stemma.svg - {numero} di {totale}");
                    return true;
                }
                else if (EsisteImmagine(svgUrl_Underscore))
                {
                    ScaricaImmagine(svgUrl_Underscore, destinazioneSVG, $"{nomeComune}-Stemma", "svg");

                    Console.WriteLine($"Trovato: {nomeComune}-Stemma.svg - {numero} di {totale}");
                    return true;
                }
                else if (EsisteImmagine(svgItaliaUrl))
                {
                    ScaricaImmagine(svgItaliaUrl, destinazioneSVG, $"{nomeComune}-Stemma", "svg");

                    Console.WriteLine($"Trovato: {nomeComune}-Stemma.svg - {numero} di {totale}");
                    return true;
                }
                else if (EsisteImmagine(pngUrl))
                {
                    ScaricaImmagine(pngUrl, destinazionePNG, $"{nomeComune}-Stemma", "png");

                    Console.WriteLine($"Trovato: {nomeComune}-Stemma.png - {numero} di {totale}");
                    return true;
                }
                else if (EsisteImmagine(pngUrl_Tratto))
                {
                    ScaricaImmagine(pngUrl_Tratto, destinazionePNG, $"{nomeComune}-Stemma", "png");

                    Console.WriteLine($"Trovato: {nomeComune}-Stemma.png - {numero} di {totale}");
                    return true;
                }
                else if (EsisteImmagine(pngUrl_Underscore))
                {
                    ScaricaImmagine(pngUrl_Underscore, destinazionePNG, $"{nomeComune}-Stemma", "png");

                    Console.WriteLine($"Trovato: {nomeComune}-Stemma.png - {numero} di {totale}");
                    return true;
                }
                else if (EsisteImmagine(pngItaliaUrl))
                {
                    ScaricaImmagine(pngItaliaUrl, destinazionePNG, $"{nomeComune}-Stemma", "png");

                    Console.WriteLine($"Trovato: {nomeComune}-Stemma.png - {numero} di {totale}");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool EsisteImmagine(string pngUrl)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(pngUrl);
                request.Headers.Add(
                "user-agent", "StemmiComuni/1.0 (https://www.test.it/; test@gmail.com/) generic-library/1.0");
                request.Method = "HEAD";
                var test = request.GetResponse();

                return true;
            }
            catch
            {
                // not found
                return false;
            }
        }

        private static void ScaricaImmagine(string pngUrl, string destinazione, string nomeFile, string estensione)
        {
            WebClient webClient = new WebClient();
            webClient.Headers.Add(
                "user-agent", "StemmiComuni/1.0 (https://www.test.it/; test@gmail.com/) generic-library/1.0");
            byte[] downloadedBytes = webClient.DownloadData(pngUrl);
            while (downloadedBytes.Length == 0)
            {
                Thread.Sleep(2000);
                downloadedBytes = webClient.DownloadData(pngUrl);
            }
            Stream file = File.Open($"{destinazione}\\{nomeFile}.{estensione}", FileMode.Create);
            file.Write(downloadedBytes, 0, downloadedBytes.Length);
            file.Close();
        }

        static bool DowloadImageAvanzata(string nomeFileNormale, string baseUrl, string destinazione)
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.Headers.Add(
                    "user-agent", "StemmiComuni/1.0 (https://www.test.it/; test@gmail.com/) generic-library/1.0");
                byte[] downloadedBytes = webClient.DownloadData(baseUrl);
                while (downloadedBytes.Length == 0)
                {
                    Thread.Sleep(2000);
                    downloadedBytes = webClient.DownloadData(baseUrl);
                }
                Stream file = File.Open(destinazione + "\\" + nomeFileNormale, FileMode.Create);
                file.Write(downloadedBytes, 0, downloadedBytes.Length);
                file.Close();

                Console.WriteLine($"Trovato: {nomeFileNormale}");
                return true;
            }
            catch
            {
                Console.WriteLine($"--- NON TROVATO: {nomeFileNormale}");
                return false;
            }
        }
    }
}
