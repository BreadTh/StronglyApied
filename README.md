# StronglyApied

A very rough work in progress.

This project aims to let you decorate your input models with attributes and validate a json string against the model. It builds on top of newtonsoft.json, however it aims to provide much more detail as to what went wrong with the model and also provide restrictions. The serializer will provide much more detailed information on how exactly the string didn't fit (or didn't validate) to the model. This info is formatted in a way such that it can be returned as a list to an HTTP API client.

Current todo:
* Add support for List<T> - T[] already works
* Add support for non-primitive value objects, primarily structs - reference objects, classes, already work.
* Add attributes for primitives: char, byte, short, ushort, int, uint, ulong, float, double
