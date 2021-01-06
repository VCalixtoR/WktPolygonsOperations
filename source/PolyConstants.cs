using System;

namespace WktPolygonsOperations
{
    public static class PolyConstants
    {
        // Defines  the numbers after ',' to coordinates, in Brasil the coordinates numbers doesn´t pass 2 digits. Must be in range [1,3] and the same as to the class PolygonOperations
        public static int coor_mx_value = 3;

        // Used in  conversion, int(clipper) to double(wkt coordinates)
        public static double coor_div_value = Math.Pow(10, 9 - coor_mx_value);

        // Delta offseting constant, the value 4.5... was obtained with a rule of 3 using the distance between extreme points in meters between a polygon
        //      and its offseting. The delta constant, make the offseting in meters, givem a param
        public static double multiplier_offseting = 4.58715596 * Math.Pow(10, 3 - coor_mx_value);
    }
}