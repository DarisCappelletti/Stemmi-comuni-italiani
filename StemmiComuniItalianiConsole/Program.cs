using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StemmiComuniItalianiConsole.Models;
using System;
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


                            foreach (var coordinate in lista)
                            {
                                try
                                {
                                    try
                                    {

                                        try
                                        {
                                            // DOWNLOAD FORMATO .PNG (Se presente)

                                            var nomeFileNormale = coordinate.NomeComune + "-Stemma.png";
                                            var nomeFileSvg = coordinate.NomeComune + "-Stemma.svg";

                                            if (!File.Exists(destinazione + "\\" + nomeFileNormale) && !File.Exists(destinazione + "\\" + nomeFileSvg))
                                            {
                                                DowloadImage(nomeFileNormale, baseUrl, destinazione);
                                            }

                                        }
                                        catch (Exception exc)
                                        {
                                            // DOWNLOAD FORMATO .SVG (Se presente)

                                            var nomeFileNormale = coordinate.NomeComune + "-Stemma.svg";

                                            if (!File.Exists(destinazione + "\\" + nomeFileNormale))
                                            {
                                                DowloadImage(nomeFileNormale, baseUrl, destinazione);
                                            }

                                        }
                                    }
                                    catch (Exception exc)
                                    {
                                        // Lo stemma non è stato trovato => ricerca approfondita

                                        var nomeFileNormale = coordinate.NomeComune;
                                        var url = $"https://en.wikipedia.org/w/api.php?action=query&prop=imageinfo&iiprop=url&generator=images&titles={nomeFileNormale}&format=json";
                                        var json = new WebClient().DownloadString(url);

                                        System.IO.Directory.CreateDirectory(destinazione + "\\Stemmi ricerca avanzata");
                                        var newDest = destinazione + "\\Stemmi ricerca avanzata";
                                        var risultato = JsonConvert.DeserializeObject<Root>(json);

                                        var ricerca = risultato.query != null && risultato.query.pages != null ? risultato.query.pages : null;

                                        if(ricerca != null)
                                        {
                                            var uno = ricerca._1 != null ? ricerca._1 : null;
                                            var due = ricerca._2 != null ? ricerca._2 : null;
                                            var tre = ricerca._3 != null ? ricerca._3 : null;
                                            var quattro = ricerca._4 != null ? ricerca._4 : null;
                                            var cinque = ricerca._5 != null ? ricerca._5 : null;

                                            if (uno != null && uno.title.ToLower().Contains("stemma") && uno.imageinfo != null)
                                            {
                                                var urlImmagine = ricerca._1.imageinfo.FirstOrDefault().url;
                                                var file = nomeFileNormale + Path.GetExtension(new Uri(urlImmagine).AbsolutePath);

                                                DowloadImageAvanzata(file, urlImmagine, newDest);
                                            }
                                            else if (due != null && due.title.ToLower().Contains("stemma") && due.imageinfo != null)
                                            {
                                                var urlImmagine = due.imageinfo.FirstOrDefault().url;
                                                var file = nomeFileNormale + Path.GetExtension(new Uri(urlImmagine).AbsolutePath);

                                                DowloadImageAvanzata(file, urlImmagine, newDest);
                                            }
                                            else if (tre != null && tre.title.ToLower().Contains("stemma") && tre.imageinfo != null)
                                            {
                                                var urlImmagine = tre.imageinfo.FirstOrDefault().url;
                                                var file = nomeFileNormale + Path.GetExtension(new Uri(urlImmagine).AbsolutePath);

                                                DowloadImageAvanzata(file, urlImmagine, newDest);
                                            }
                                            else if (quattro != null && quattro.title.ToLower().Contains("stemma") && quattro.imageinfo != null)
                                            {
                                                var urlImmagine = quattro.imageinfo.FirstOrDefault().url;
                                                var file = nomeFileNormale + Path.GetExtension(new Uri(urlImmagine).AbsolutePath);

                                                DowloadImageAvanzata(file, urlImmagine, newDest);
                                            }
                                            else if (cinque != null && cinque.title.ToLower().Contains("stemma") && cinque.imageinfo != null)
                                            {
                                                var urlImmagine = cinque.imageinfo.FirstOrDefault().url;
                                                var file = nomeFileNormale + Path.GetExtension(new Uri(urlImmagine).AbsolutePath);

                                                DowloadImageAvanzata(file, urlImmagine, newDest);
                                            }
                                        }
                                    }
                                }
                                catch (Exception exc)
                                { 
                                
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

        static void DowloadImage(string nomeFileNormale, string baseUrl, string destinazione)
        {
            var nomeFileEscape = HttpUtility.UrlEncode(nomeFileNormale);
            WebClient webClient = new WebClient();
            webClient.Headers.Add(
                "user-agent", "StemmiComuni/1.0 (https://www.test.it/; test@gmail.com/) generic-library/1.0");
            byte[] downloadedBytes = webClient.DownloadData(baseUrl + nomeFileEscape);
            while (downloadedBytes.Length == 0)
            {
                Thread.Sleep(2000);
                downloadedBytes = webClient.DownloadData(baseUrl + nomeFileEscape);
            }
            Stream file = File.Open(destinazione + "\\" + nomeFileNormale, FileMode.Create);
            file.Write(downloadedBytes, 0, downloadedBytes.Length);
            file.Close();
        }

        static void DowloadImageAvanzata(string nomeFileNormale, string baseUrl, string destinazione)
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
        }
    }
}
