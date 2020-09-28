# StronglyApied

A very rough work in progress.

This project aims to let you decorate your input models with attributes and validate+parse a json string against the model. The project builds on top of newtonsoft.json, however it aims to provide much more detail as to what went wrong with fitting the data to the model and also provide restrictions (i.e. this string can only be 128 chars, etc). This info is formatted in a way such that it can be returned as a list to an HTTP API client where both human reader and computer can potentially easy understand.

Nuget:
https://www.nuget.org/packages/BreadTh.StronglyApied/1.0.0

Current todo:
* In ModelValidator: Add support for List<T> - T[] already works
* In ModelValidator: Add support for non-primitive value objects, primarily structs - reference objects, classes, already work.
* In ModelValidator: Add missing attributes for primitives: char, byte, short, ushort, int, uint, ulong, float, double
