﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;
using Dynamo.FSchemeInterop;

using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Utilities
{

    /// <summary>
    /// Used with the Auto-generator. Allows automatic conversion of inputs and outputs
    /// </summary>
    public static class DynamoTypeConverter
    {
        private static ReferenceArrayArray ConvertFSharpListListToReferenceArrayArray(FSharpList<Value> lstlst)
        {
            ReferenceArrayArray refArrArr = new ReferenceArrayArray();
            foreach (Value v in lstlst)
            {
                ReferenceArray refArr = new ReferenceArray();
                FSharpList<Value> lst = (v as Value.List).Item;

                AddReferencesToArray(refArr, lst);

                refArrArr.Append(refArr);
            }

            return refArrArr;
        }

        private static void AddReferencesToArray(ReferenceArray refArr, FSharpList<Value> lst)
        {
            dynRevitSettings.Doc.RefreshActiveView();

            foreach (Value vInner in lst)
            {
                var mc = (vInner as Value.Container).Item as ModelCurve;
                var f = (vInner as Value.Container).Item as Face;
                var p = (vInner as Value.Container).Item as Point;
                var c = (vInner as Value.Container).Item as Curve;
                var rp = (vInner as Value.Container).Item as ReferencePlane;

                if (mc != null)
                    refArr.Append(mc.GeometryCurve.Reference);
                else if (f != null)
                    refArr.Append(f.Reference);
                else if (p != null)
                    refArr.Append(p.Reference);
                else if (c != null)
                    refArr.Append(c.Reference);
                else if (c != null)
                    refArr.Append(rp.Reference);
            }

        }

        private static ReferenceArray ConvertFSharpListListToReferenceArray(FSharpList<Value> lstlst)
        {
            ReferenceArray refArr = new ReferenceArray();

            AddReferencesToArray(refArr, lstlst);

            return refArr;

        }

        private static CurveArrArray ConvertFSharpListListToCurveArrayArray(FSharpList<Value> lstlst)
        {
            CurveArrArray crvArrArr = new CurveArrArray();
            foreach (Value v in lstlst)
            {
                CurveArray crvArr = new CurveArray();
                FSharpList<Value> lst = (v as Value.List).Item;

                AddCurvesToArray(crvArr, lst);

                crvArrArr.Append(crvArr);
            }

            return crvArrArr;
        }

        private static CurveArray ConvertFSharpListListToCurveArray(FSharpList<Value> lstlst)
        {
            CurveArray crvArr = new CurveArray();

            AddCurvesToArray(crvArr, lstlst);

            return crvArr;

        }

        private static void AddCurvesToArray(CurveArray crvArr, FSharpList<Value> lst)
        {
            dynRevitSettings.Doc.RefreshActiveView();

            foreach (Value vInner in lst)
            {
                var c = (vInner as Value.Container).Item as Curve;
                crvArr.Append(c);
            }

        }
        
        public static object ConvertInput(Value input, Type output)
        {
            if (input.IsContainer)
            {
                object item = ((Value.Container)input).Item;

                #region ModelCurve
                if (item.GetType() == typeof(ModelCurve))
                {
                    ModelCurve a = (ModelCurve)item;

                    if (output == typeof(Curve))
                    {
                        return ((ModelCurve)item).GeometryCurve;
                    }
                }
                #endregion

                #region SketchPlane
                else if (item.GetType() == typeof(SketchPlane))
                {
                    SketchPlane a = (SketchPlane)item;

                    if (output == typeof(Plane))
                    {
                        return a.Plane;
                    }
                    else if (output == typeof(ReferencePlane))
                    {
                        return a.Plane;
                    }
                    else if (output == typeof(string))
                    {
                        return string.Format("{0},{1},{2},{3},{4},{5}", a.Plane.Origin.X, a.Plane.Origin.Y, a.Plane.Origin.Z,
                            a.Plane.Normal.X, a.Plane.Normal.Y, a.Plane.Normal.Z);
                    }
                }
                #endregion

                #region Point
               else if (item.GetType() == typeof(Point))
                {
                    Point a = (Point)item;

                    if (output == typeof(XYZ))
                    {
                        return a.Coord;
                    }
                    else if (output == typeof(string))
                    {
                        return string.Format("{0},{1},{2}", a.Coord.X, a.Coord.Y, a.Coord.Z);
                    }
                }
                #endregion

                #region ReferencePoint
                else if (item.GetType() == typeof(ReferencePoint))
                {
                    ReferencePoint a = (ReferencePoint)item;

                    if (output == typeof(XYZ))
                    {
                        return a.Position;
                    }
                    else if (output == typeof(Reference))
                    {
                        return a.GetCoordinatePlaneReferenceXY();
                    }
                    else if (output == typeof(Transform))
                    {
                        return a.GetCoordinateSystem();
                    }
                    else if (output == typeof(string))
                    {
                        return string.Format("{0},{1},{2}", a.Position.X, a.Position.Y, a.Position.Z);
                    }
                }
                #endregion

                return item;
            }
            else if (input.IsNumber)
            {
                double a = (double)((Value.Number)input).Item;

                if (output == typeof(bool))
                {
                    return Convert.ToBoolean(a);
                }
                else if (output == typeof(Int32))
                {
                    return Convert.ToInt32(a);
                }

                return a;
            }
            else if(input.IsString)
            {
                string a = ((Value.String)input).Item.ToString();
                return a;
            }
            else if (input.IsList)
            {
                FSharpList<Value> a = ((Value.List)input).Item;

                if (output == typeof(ReferenceArrayArray))
                {
                    return DynamoTypeConverter.ConvertFSharpListListToReferenceArrayArray(a);
                }
                else if (output == typeof(ReferenceArray))
                {
                    return DynamoTypeConverter.ConvertFSharpListListToReferenceArray(a);
                }
                else if (output == typeof(CurveArrArray))
                {
                    return DynamoTypeConverter.ConvertFSharpListListToCurveArray(a);
                }
                else if (output == typeof(CurveArray))
                {
                    return DynamoTypeConverter.ConvertFSharpListListToCurveArray(a);
                }

                return a;
            }

            //return the input by default
            return input;
        }

        public static Value ConvertToValue(object input)
        {
            if (input.GetType() == typeof(double))
            {
                return Value.NewNumber(System.Convert.ToDouble(input));
            }
            else if (input.GetType() == typeof(int))
            {
                return Value.NewNumber(System.Convert.ToDouble(input));
            }
            else if (input.GetType() == typeof(string))
            {
                return Value.NewString(System.Convert.ToString(input));
            }
            else if (input.GetType() == typeof(bool))
            {
                return Value.NewNumber(System.Convert.ToInt16(input));
            }
            else if (input.GetType() == typeof(IntersectionResultArray))
            {
                // for interesection results, send out two lists
                // a list for the XYZs and one for the UVs
                List<Value> xyzs = new List<Value>();
                List<Value> uvs = new List<Value>();
                
                foreach (IntersectionResult ir in (IntersectionResultArray)input)
                {
                    xyzs.Add(Value.NewContainer(ir.XYZPoint));
                    uvs.Add(Value.NewContainer(ir.UVPoint));
                }

                FSharpList<Value> result = FSharpList<Value>.Empty;
                result = FSharpList<Value>.Cons(
                           Value.NewList(Utils.SequenceToFSharpList(uvs)),
                           result);
                result = FSharpList<Value>.Cons(
                           Value.NewList(Utils.SequenceToFSharpList(xyzs)),
                           result);
                if (xyzs.Count > 0 || uvs.Count > 0)
                {
                    return Value.NewList(result);
                }
                else
                {
                    //TODO: if we don't have any XYZs or UVs, chances are
                    //we have just created an intersection result array to
                    //catch some values. in this case, don't convert.
                    return Value.NewContainer(input);
                }
            }
            else
            {
                return Value.NewContainer(input);
            }
        }
    }
}