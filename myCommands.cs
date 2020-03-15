// (C) Copyright 2015 by Microsoft 
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

#if BRICSCAD
using Bricscad.Runtime;
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.EditorInput;
using Teigha.Runtime;
#endif
#if AUTOCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
#endif

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(Building.MyCommands))]

namespace Building
{
    //This class shows how specific operations can be done both in bcad and acad.
    public class MyCommands
    {
        
        [CommandMethod("CreateBuilding")]
        public void CreateBuilding()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            if (!Building.Commands.CreateBuildings.CreateBuilding.RunCommand(doc))
            {
                //doc.Editor.WriteMessage("Canceled!\n");
            }
        }
    }
   

}
