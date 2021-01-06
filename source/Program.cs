using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace WktPolygonsOperations
{
    public class Program { 

        // change this to your file paths
        public static string wktStrPath = @"C:\Users\vinic\Desktop\source\c#\Poly\Poly\data\polydatabase.txt";  // csv path
        public static string wktResulPath = @"C:\Users\vinic\Desktop\source\c#\Poly\Poly\data\result.txt";      // result path

        // ------------------------------------------------------ Main ------------------------------------------------------
        // to make tests, here an offseting operation given a string wkt file, each string in each line
        static void Main(string[] args)
        {
            using (var reader = new StreamReader(wktStrPath)) { 

                Stopwatch stopwatch = new Stopwatch();
                List<string> strwkts = new List<string>();
                
                // offseting param
                double param = 400.0;
                string str = "";

                while (reader.Peek() > -1)
                    strwkts.Add(reader.ReadLine());

                if (strwkts.Count == 0) 
                {
                    Console.WriteLine("Error, invalid filepath!");
                    return;
                }

                // time counter
                stopwatch.Start();

                str = str + String.Format("--------------ClipperOffsetting-------------- param = " + param +"\n");
                str = str + PolyOperations.WktOffseting(strwkts,param);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;

                str = str + String.Format("\nTime spent {0:00}:{1:00}:{2:00}.{3}\n",
                                    ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);

                // saving results
                string txt = String.Format(wktResulPath);
                File.WriteAllText(txt,str);
            }
        }
    }
}