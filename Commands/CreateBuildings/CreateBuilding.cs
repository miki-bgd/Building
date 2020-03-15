using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if BRICSCAD
using Bricscad.ApplicationServices;
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.EditorInput;
using Teigha.Colors;
#endif

#if AUTOCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
#endif

namespace Building.Commands.CreateBuildings
{
    class CreateBuilding
    {
        private static ObjectId GetPolyline(Editor ed)
        {
            // Select polyline
            PromptEntityOptions entityOptions = new PromptEntityOptions("\nSelect Polyline: ");
            entityOptions.SetRejectMessage("Not a Polyline!");
            entityOptions.AllowNone = false;
            entityOptions.AddAllowedClass(typeof(Polyline), true);
            PromptEntityResult entityResult = ed.GetEntity(entityOptions);
            if (entityResult.Status != PromptStatus.OK)
                return ObjectId.Null;
            return entityResult.ObjectId;
        }
        private static bool GetNumberOfFloors(Editor ed, ref int numberOfFloors)
        {
            // Select text (or type number of floors)
            PromptEntityOptions entityOptions = new PromptEntityOptions("\nSelect Text (or Enter to skip): ");
            entityOptions.SetRejectMessage("Not Text or MText");
            entityOptions.AddAllowedClass(typeof(DBText), true);
            entityOptions.AddAllowedClass(typeof(MText), true);
            entityOptions.AllowNone = true;
            PromptEntityResult entityResult = ed.GetEntity(entityOptions);
            if (entityResult.Status == PromptStatus.None)
            {
                PromptIntegerOptions integerOptions = new PromptIntegerOptions("\nEnter number of floors:");
                integerOptions.DefaultValue = 1;
                PromptIntegerResult integerResult = ed.GetInteger(integerOptions);
                if (integerResult.Status != PromptStatus.OK)
                    return false;
                numberOfFloors = integerResult.Value;
            }
            else if (entityResult.Status != PromptStatus.OK)
                return false;
            else
            {
                using (DBObject obj = entityResult.ObjectId.Open(OpenMode.ForRead))
                {
                    DBText t1 = obj as DBText;
                    MText t2 = obj as MText;
                    string s = null;
                    if (t1 != null)
                        s = t1.TextString;
                    else if (t2 != null)
                        s = t2.Text;
                    else
                        throw new NotSupportedException("Entity not supported!");
                    if (!string.IsNullOrEmpty(s))
                    {
                        numberOfFloors = FloorCount.GetCount(s);
                    }
                    ed.WriteMessage($"\nNumber of floors: {numberOfFloors}");
                }
            }
            return true;
        }

        private static bool GetFloorHeight(Editor ed, ref double height)
        {
            // Enter height
            PromptDoubleOptions doubleOptions = new PromptDoubleOptions("\nEnter floor height:");
            doubleOptions.DefaultValue = 2.6;
            doubleOptions.AllowNegative = false;
            doubleOptions.AllowArbitraryInput = false;
            doubleOptions.AllowNone = false;
            PromptDoubleResult doubleResult = ed.GetDouble(doubleOptions);
            if (doubleResult.Status != PromptStatus.OK)
                return false;
            height = doubleResult.Value;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static bool RunCommand(Document doc)
        {
            ObjectId polylineId = GetPolyline(doc.Editor);
            if (polylineId == ObjectId.Null)
                return false;

            int numberOfFloors = 0;
            if (!GetNumberOfFloors(doc.Editor, ref numberOfFloors))
                return false;

            double floorHeight = 0;
            if (!GetFloorHeight(doc.Editor, ref floorHeight))
                return false;

            double buildingHeight = floorHeight * numberOfFloors;
            if (buildingHeight < 0.0001)
                return false;
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord ms = tr.GetObject(doc.Database.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                Polyline pl = tr.GetObject(polylineId, OpenMode.ForRead) as Polyline;
                doc.Editor.WriteMessage($"Selected polyline with surface: {pl.Area.ToString("0.00")} m2\n");

                Solid3d solid = new Solid3d();
                ms.AppendEntity(solid);
                tr.AddNewlyCreatedDBObject(solid, true);

                Line l = new Line()
                {
                    StartPoint = pl.StartPoint,
                    EndPoint = pl.StartPoint.Add(new Vector3d(0, 0, buildingHeight))
                };
                SweepOptionsBuilder sob = new SweepOptionsBuilder();
                sob.Align = SweepOptionsAlignOption.AlignSweepEntityToPath;
                sob.BasePoint = l.StartPoint;
                solid.CreateSweptSolid(pl, l, sob.ToSweepOptions());

                doc.Editor.WriteMessage($"Created Solid with volume: {(solid.MassProperties.Volume).ToString("0.000")}m3\n");

                l.Dispose();
                tr.Commit();
            }

            return true;
        }
    }
}
