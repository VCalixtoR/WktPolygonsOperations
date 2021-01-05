using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
// Documentation: https://github.com/junmer/clipper-lib/blob/master/Documentation.md#clipperlibpaths
// Download link cs clipper 6.4.2 https://sourceforge.net/projects/polyclipping/
using ClipperLib;
// Clipper Path
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using CsvHelper;
using System.Diagnostics;
using System.Threading;

namespace Poly
{
	// Need To make comments and organizate this file
	
    public static class PolyFormatConvert
    {
        private static int coor_mx_value = 3;
        private static double coor_div_value = Math.Pow(10, 9 - coor_mx_value);

        public static double multiplicador_offseting = 4.58715596 * Math.Pow(10, 3 - coor_mx_value);

        // --------------------------------------------- Conversion to Paths Methods ---------------------------------------------
		
        public static Paths AllWktToPaths(List<string> strwkts)
        {

            Paths AllWktpaths = new Paths();

            foreach (var strwkt in strwkts)
            {
                foreach (var wktpaths in WktToPaths(strwkt))
                {
                    AllWktpaths.Add(wktpaths);
                }
            }

            return AllWktpaths;
        }

        public static Paths WktToPaths(string wkt)
        {
            string str = wkt;

            if (!string.IsNullOrWhiteSpace(str))
            {
                if (str.Contains("MULTIPOLYGON"))
                {
                    str = str.Replace("MULTIPOLYGON", "");
                    str = str.Trim();
                    return MultiPToPaths(str);
                }

                if (str.Contains("POLYGON"))
                {
                    str = str.Replace("POLYGON", "");
                    str = str.Trim();
                    return PToPaths(str);
                }

                return new Paths();
            }

            return new Paths();
        }

        public static Paths MultiPToPaths(string multiWkt)
        {
            string[] separators = new[] { "))" };
            Paths polys = new Paths();

            multiWkt = multiWkt.Remove(0, 1);
            multiWkt = multiWkt.Remove(multiWkt.Length - 1);

            foreach (var polyWkt in multiWkt.Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {
                foreach (var poly in PToPaths((polyWkt + "))").Trim(',').Trim()))
                {
                    polys.Add(poly);
                }
            }
            return polys;
        }

        public static Paths PToPaths(string polyWkt)
        {
            string[] separators = new[] { ")" };
            string[] separators2 = new[] { "," };
            Paths polys = new Paths();
            int i = 0;

            polyWkt = polyWkt.Remove(0, 1);
            polyWkt = polyWkt.Remove(polyWkt.Length - 1);

            foreach (var contour in polyWkt.Replace("(", "").Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {

                polys.Add(new Path());

                foreach (var coor in contour.Trim().Split(separators2, StringSplitOptions.RemoveEmptyEntries))
                {
                    polys[i].Add(CoorToInt(coor));
                }

                i++;
            }
            return polys;
        }

        public static IntPoint CoorToInt(string coor)
        {
            string[] separators = new[] { " " };
            var aux = new StringBuilder();
            bool first = true;
            int i, j, x = 0, y = 0;

            foreach (var n in coor.Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {
                i = 0;
                j = 1;
                string str = n.Replace("-", "").Replace("+", "").Trim();

                if (!str.Contains("."))
                    str = str + ".000000";

                while (str.IndexOf('.') < coor_mx_value)
                    str = "0" + str;

                if (str.IndexOf('.') > coor_mx_value)
                    Console.WriteLine("Erro, coordenadas ultrapassam 180");

                aux.Insert(0, (n[0] == '-' ? '-' : '+'));

                while (j < 10)
                {
                    if (i >= str.Length)
                    {
                        aux.Insert(j, '0');
                        j++;
                    }
                    else if (Char.IsDigit(str[i]))
                    {
                        aux.Insert(j, str[i]);
                        j++;
                    }
                    i++;
                }

                var _aux = aux.ToString();

                // x or y
                if (first)
                    x = int.TryParse(_aux, out var value) ? value : 0;
                else
                    y = int.TryParse(_aux, out var value) ? value : 0;
                first = !first;
                aux.Clear();
            }

            return new IntPoint(x, y);
        }

        public static double IntToCoor(int x)
        {
            return x / coor_div_value;
        }

        // --------------------------------------------- Conversion to String Methods ---------------------------------------------

        public static string StrWktFromPolyTree(PolyTree tree)
        {
            if (tree == null)
                return null;

            List<PolyNode> nodeList = new List<PolyNode>();

            foreach (PolyNode p in tree.Childs)
                nodeList.Add(p);

            string wkt = "MULTIPOLYGON(" + StrPolyTreeLargura(nodeList) + ")";

            return wkt;
        }

        public static string StrPolyTreeLargura(List<PolyNode> nodeList)
        {
            if (nodeList.Count == 0)
                return "";

            PolyNode atualNode;

            int i;
            string str = "";

            while (nodeList.Count > 0)
            {
                atualNode = nodeList[0];
                nodeList.RemoveAt(0);

                i = atualNode.ChildCount - 1;
                while (i >= 0)
                {
                    nodeList.Insert(0, atualNode.Childs[i]);
                    i--;
                }

                if (atualNode.IsHole)
                    str = str + "(" + StrNodeTree(atualNode);
                else
                    str = str + "((" + StrNodeTree(atualNode);

                if (nodeList.Count == 0)
                {
                    str = str + "))";
                    break;
                }
                else if (nodeList[0].IsHole)
                    str = str + "),";
                else
                    str = str + ")),";
            }
            return str;
        }

        public static string StrNodeTree(PolyNode node, bool convert = true)
        {
            bool first_coor = true;
            string closePoint = "";
            string str = "";

            foreach (var coor in node.Contour)
            {
                if (first_coor)
                {
                    first_coor = !first_coor;
                    if (convert)
                        closePoint = String.Format(IntToCoor((int)coor.X).ToString(CultureInfo.InvariantCulture) +
                            " " + IntToCoor((int)coor.Y).ToString(CultureInfo.InvariantCulture));
                    else
                        closePoint = String.Format(coor.X.ToString() + " " + coor.Y.ToString());
                    str = str + closePoint;
                }
                else
                {
                    if (convert)
                        str = str + String.Format("," + IntToCoor((int)coor.X).ToString(CultureInfo.InvariantCulture) +
                            " " + IntToCoor((int)coor.Y).ToString(CultureInfo.InvariantCulture));
                    else
                        str = str + String.Format("," + coor.X.ToString() + " " + coor.Y.ToString());
                }
            }
            str = str + "," + closePoint;
            return str;
        }
    }

    
    

    class Program
    {
        // --------------------------------------------- Polygon Operations Methods and Main ---------------------------------------------

        // Union
        public static string WktUnion(List<string> poligons)
        {
            string resul;
            Paths polys = PolyFormatConvert.AllWktToPaths(poligons);
            Clipper c = new Clipper();
            PolyTree tree = new PolyTree();

            if (poligons.Count == 0)
                return null;

            c.AddPaths(polys, PolyType.ptSubject, true);
            c.Execute(ClipType.ctUnion, tree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            resul = PolyFormatConvert.StrWktFromPolyTree(tree);

            return resul;
        }

        // Offseting
        public static string WktOffseting(List<string> poligons, double comprimento)
        {
            string resul;
            Paths polys = PolyFormatConvert.AllWktToPaths(poligons);
            ClipperOffset co = new ClipperOffset();
            PolyTree tree = new PolyTree();

            if (poligons.Count == 0)
                return null;

            co.AddPaths(polys, JoinType.jtRound, EndType.etClosedPolygon);

            co.Execute(ref tree, (double)(comprimento * PolyFormatConvert.multiplicador_offseting));

            resul = PolyFormatConvert.StrWktFromPolyTree(tree);

            return resul;
        }

		// Change Directory paths if you need
        public static string wktCsvPath = @"C:\Users\vinic\Desktop\source\c#\Poly\Poly\polydatabase.txt"; // csv path
        public static string wktResulPath = @"C:\Users\vinic\Desktop\source\c#\Poly\Poly\result.txt"; // result path

        // ------------------------------------------------------ Main ------------------------------------------------------
        static void Main(string[] args)
        {
            using (var reader = new StreamReader(wktCsvPath)) { 

                Stopwatch stopwatch = new Stopwatch();
                List<string> strwkts = new List<string>();
                // offseting param
                double parametro = 20000.0;
                string str = "";

                while (reader.Peek() > -1)
                    strwkts.Add(reader.ReadLine());

                if (strwkts.Count == 0) 
                {
                    Console.WriteLine("Error, invalid WKT file!");
                    return;
                }

                stopwatch.Start();

                str = str + String.Format("--------------ClipperOffsetting-------------- param = " + parametro +"\n");
                str = str + WktOffseting(strwkts,parametro);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;

                str = str + String.Format("\nTime spent {0:00}:{1:00}:{2:00}.{3}\n",
                                    ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);

                string txt = String.Format(wktResulPath);
                File.WriteAllText(txt,str);
            }
        }
    }
}