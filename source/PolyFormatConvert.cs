using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ClipperLib;
// documentation: https://github.com/junmer/clipper-lib/blob/master/Documentation.md#clipperlibpaths
// download link clipper.cs 6.4.2 https://sourceforge.net/projects/polyclipping/
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace WktPolygonsOperations
{
    public static class PolyFormatConvert
    {
        // --------------------------------------------- Conversion methods ---------------------------------------------

        // AllWktToPaths: Converts all strwkts to Paths using for each string the method WktToPaths. WKT(List<string>) -> PATHS(List<Path>)
        // Param:   List<string> strwkts, strings of wkts used
        // Return:  Paths AllWktpaths, strwkts converted in Paths
        public static Paths AllWktToPaths(List<string> strwkts)
        {

            Paths AllWktpaths = new Paths();

            foreach (var strwkt in strwkts)
            {
                // for each string
                foreach (var wktpaths in WktToPaths(strwkt))
                {
                    AllWktpaths.Add(wktpaths);
                }
            }

            return AllWktpaths;
        }

        // WktToPaths: Converts a single string wkts(received from AllWktToPaths) to Paths. The return can be more than one Path, because the polygons holes are polygons, and holes can have polygons.
        // Param:   string wkt, string of a single wkt
        // Return:  Paths Wktpaths, wkts converted in Paths
        public static Paths WktToPaths(string wkt)
        {
            string str = wkt;

            // As we said before in README, the program (still) doesn´t allows POINT, MULTIPOINT, LINESTRING, MULTILINESTRING and GEOMETRYCOLLECTION.
            //      the polygons wkt string must have, MULTIPOLYGON or POLYGON
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (str.Contains("POINT") || str.Contains("LINESTRING") || str.Contains("GEOMETRYCOLLECTION"))
                {
                    ;
                }
                else if (str.Contains("MULTIPOLYGON"))
                {
                    str = str.Replace("MULTIPOLYGON", "");
                    str = str.Trim();
                    return MultiPToPaths(str);
                }
                else if (str.Contains("POLYGON"))
                {
                    str = str.Replace("POLYGON", "");
                    str = str.Trim();
                    return PToPaths(str);
                }

                return new Paths();
            }

            return new Paths();
        }

        // MultiPToPaths: Converts a string wkt multipolygon(received from WktToPaths) to Paths. The return can be more than one Path.
        // Param:   string multiWkt, string of a multipolygon wkt
        // Return:  Paths polys, multipolygon converted in Paths
        public static Paths MultiPToPaths(string multiWkt)
        {
            string[] separators = new[] { "))" };
            Paths polys = new Paths();

            // Remove the first and the last parentheses. (((...))) -> ((...))
            multiWkt = multiWkt.Remove(0, 1);
            multiWkt = multiWkt.Remove(multiWkt.Length - 1);

            // for each coordinate
            foreach (var polyWkt in multiWkt.Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {
                foreach (var poly in PToPaths((polyWkt + "))").Trim(',').Trim()))
                {
                    polys.Add(poly);
                }
            }
            return polys;
        }

        // PToPaths:Converts a string with coordinates from a multipolygon or polygon(received from MultiPToPaths or WktToPaths) to a pair of int using CoorToInt.
        // Param:   string polyWkt, a string of coordinates
        // Return:  Paths polys, multipolygon converted in Paths with assigned ints
        public static Paths PToPaths(string polyWkt)
        {
            string[] separators = new[] { ")" };
            string[] separators2 = new[] { "," };
            Paths polys = new Paths();
            int i = 0;

            // Remove the first and the last parentheses. -> (...)
            polyWkt = polyWkt.Remove(0, 1);
            polyWkt = polyWkt.Remove(polyWkt.Length - 1);

            // for each polygon ()
            foreach (var contour in polyWkt.Replace("(", "").Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {
                polys.Add(new Path());

                // for each coordinate ... ... , ... ...
                foreach (var coor in contour.Trim().Split(separators2, StringSplitOptions.RemoveEmptyEntries))
                {
                    polys[i].Add(CoorToInt(coor));
                }

                i++;
            }
            return polys;
        }

        // CoorToInt:   Converts a pair of double values in a pair of int values with 9 digits
        // Param:   string coor, the pair of doubles
        // return:  ClipperLib.IntPoint value, the pair of ints
        public static IntPoint CoorToInt(string coor)
        {
            string[] separators = new[] { " " };
            var aux = new StringBuilder();
            bool first = true;
            int i, j, x = 0, y = 0;

            // for each double
            foreach (var n in coor.Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {
                i = 0;
                j = 1;
                string str = n.Replace("-", "").Replace("+", "").Trim();

                // without '.'
                if (!str.Contains("."))
                    str = str + ".000000";

                // if the number have less digits before '.' than coor_mx_value value
                while (str.IndexOf('.') < PolyConstants.coor_mx_value)
                    str = "0" + str;

                // coor_mx_value must be in range [1,3] and the double value too
                if (str.IndexOf('.') > PolyConstants.coor_mx_value)
                    Console.WriteLine("Erro, coordenadas ultrapassam 180" + str);

                // making a string of the value
                aux.Insert(0, (n[0] == '-' ? '-' : '+'));

                while (j < 10)
                {
                    // if doesn't have enough digits after the '.'
                    if (i >= str.Length)
                    {
                        aux.Insert(j, '0');
                        j++;
                    }
                    // if isDigit, will be added
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

        // IntToCoor:   Converts a single int in a double value
        // Param:   int x
        // Return:  double value
        public static double IntToCoor(int x)
        {
            return x / PolyConstants.coor_div_value;
        }

        // --------------------------------------------- Métodos de conversão para string ---------------------------------------------

        // StrWktFromPolyTree: Assist to perform Breadth-first access in the PolyTree(Result from a clipper operation such as Union or Offsetting) and obtain the Wkt string
        //      PolyTree description: http://www.angusj.com/delphi/clipper/documentation/Docs/Units/ClipperLib/Classes/PolyTree/_Body.htm</summary>
        // Param:   PolyTree tree, Result from the clipper operation
        // Return:  string wkt, Polytree converted to wkt format
        public static string StrWktFromPolyTree(PolyTree tree)
        {
            if (tree == null)
                return null;

            List<PolyNode> nodeList = new List<PolyNode>();

            // starts the list to perform the Breadth-first access
            foreach (PolyNode p in tree.Childs)
                nodeList.Add(p);

            // StrPolyTreeLargura returns the inner part of MULTIPOLYGON(...)
            string wkt = StrPolyTreeBF(nodeList);

            if (wkt.Length > 0)
                wkt = "MULTIPOLYGON(" + wkt + ")";
            else
                wkt = null;

            return wkt;
        }

        // StrWktFromPolyTree: Perform iterative Breadth-first access in the PolyTree to obtain the Wkt string(Inner part of MULTIPOLYGON(...))
        // Param:   List<PolyNode> nodeList, A node list from the first nodes in the polytree
        // Return:  string str, Polytree converted to wkt format(Inner part of MULTIPOLYGON(...))
        public static string StrPolyTreeBF(List<PolyNode> nodeList)
        {
            if (nodeList.Count == 0)
                return "";

            PolyNode actualNode;
            int i;
            string str = "";

            while (nodeList.Count > 0)
            {
                // to remove nodeList[0] and visit its childs
                actualNode = nodeList[0];
                nodeList.RemoveAt(0);

                // Iterative Breadth-first, fi-fo
                i = actualNode.ChildCount - 1;
                while (i >= 0)
                {
                    // inner part of each node
                    nodeList.Insert(0, actualNode.Childs[i]);
                    i--;
                }

                // "(( x y, ... or ( x y, ... "
                if (actualNode.IsHole)
                    str = str + "(" + StrNodeTree(actualNode);
                else
                    str = str + "((" + StrNodeTree(actualNode);

                // "))", ")," or ")),"
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

        // StrNodeTree: Converts the inner part of each node ((.......)) to wkt string format
        // Param:   PolyNode node, A node from the polytree
        //          bool convert = true, if set to false, will not convert the int to double
        //          bool closed = true, if set to false, will not close the polygon (add the first coordinate to the end)
        // Return:  string wkt, Inner part of the node converted to wkt string format
        public static string StrNodeTree(PolyNode node, bool convert = true, bool closed = true)
        {
            bool first_coor = true;
            string closePoint = "";
            string str = "";

            // for each coordinate in contour
            foreach (var coor in node.Contour)
            {
                // convert
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
            if (closed)
            {
                str = str + "," + closePoint;
            }
            return str;
        }
    }
}