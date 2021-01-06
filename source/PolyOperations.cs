using System.Collections.Generic;
using ClipperLib;
// documentation: https://github.com/junmer/clipper-lib/blob/master/Documentation.md#clipperlibpaths
// download link clipper.cs 6.4.2 https://sourceforge.net/projects/polyclipping/
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace WktPolygonsOperations
{
    public static class PolyOperations
    {
        // --------------------------------------------- Polygon Operations methods ---------------------------------------------

        // WktUnion:    Makes the polygons union operation using a list of string wkts
        // Param:   List<string> poligons: string wkts to operate
        // Return:  string result: result of the operation in wkt string format
        public static string WktUnion(List<string> poligons)
        {
            string result;
            Paths polys = PolyFormatConvert.AllWktToPaths(poligons);
            Clipper c = new Clipper();
            PolyTree tree = new PolyTree();

            if (poligons.Count == 0)
                return null;

            // makes the union using clipper library
            c.AddPaths(polys, PolyType.ptSubject, true);
            c.Execute(ClipType.ctUnion, tree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            result = PolyFormatConvert.StrWktFromPolyTree(tree);

            return result;
        }

        // WktOffseting:    Makes the polygons offseting operation using a list of string wkts
        // Param:   List<string> poligons: string wkts to operate
        //          double meters: meters to be offseted
        // Return:  string result: result of the operation in wkt string format
        public static string WktOffseting(List<string> poligons, double meters)
        {
            string result;
            Paths polys = PolyFormatConvert.AllWktToPaths(poligons);
            ClipperOffset co = new ClipperOffset();
            PolyTree tree = new PolyTree();

            if (poligons.Count == 0)
                return null;

            // Makes the offseting using clipper library
            co.AddPaths(polys, JoinType.jtRound, EndType.etClosedPolygon);
            // PolyConstants.multiplier_offseting makes the operation an operation in meters given a param
            co.Execute(ref tree, (double)(meters * PolyConstants.multiplier_offseting));

            result = PolyFormatConvert.StrWktFromPolyTree(tree);

            return result;
        }
    }
}