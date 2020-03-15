# Building

Autocad/Bricscad plugin: just load it with _netload_ command, and run commands from it.

## Supported commands:

createBuilding
```
Select Polyline which represents building base. 
Then select text entity which describes number of floors of the building 
> Format of text is P+[number]+Pt, where each of 3 parameters are optional 
> (allowed combination examples: P, Pt, 2, P+2, P+Pt, P+2+Pt, 2+Pt; all other values are ignored)
Then enter height of each floor.
As a result, Solid3d object is created.
```
