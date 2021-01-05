# WktPolygonsOperations
Welcome to my first contribution on Git! :blush:
In this implementation we use the :sparkles: open source [Clipper Library](https://github.com/junmer/clipper-lib/) to make the operations in the polygons. 

How it works:
Given a string or a string list of [WKTs](https://en.wikipedia.org/wiki/Well-known_text_representation_of_geometry), the program converts the given value to the clipper format data(Paths),then make operations(Union or Offsetting) using clipper and converts again to a single result string Wkt.

The Clipper doesn't allow processing coordenates in double values. Because of this, we convert the double coordinates to int coordinates considering a fractional part of each number.
