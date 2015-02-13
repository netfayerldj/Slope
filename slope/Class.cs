// (C) Copyright 2002-2005 by Autodesk, Inc. 
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to 
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

using System;
using System.Data;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

[assembly: CommandClass(typeof(ClassLibrary.Class))]

namespace ClassLibrary
{
    /// <summary>
    /// Summary description for Class.
    /// </summary>
    public class Class
    {
        public Class()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [CommandMethod("spx")]
        static public void spx2()
        {
            DrawSlope myDSS = new DrawSlope();
            
            double step1 = 1;   //普通示坡线间距（可设置为默认值）
            double step2 = 1;   //锥坡示坡线间距（可设置为默认值）
            bool top1 = true;   //普通示坡线垂直于坡顶还是坡底（可设置为默认值）
            bool top2 = true;   //锥坡顶点作为坡顶还是坡底（可设置为默认值）
            double maxLen = double.MaxValue;    //示坡长线最大长度

            
            DotNetArxLH.LhDraw.LhPublic lhpub = new DotNetArxLH.LhDraw.LhPublic();
            bool isDrillSlope = false;  //是否画锥坡
            Point3d drillPt;            //锥坡顶点
            Curve topCurve = null, bottomCurve; //坡顶线和坡底线
            int myColor = 3;

            #region 选择坡顶线

            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityOptions proEntOpt = new PromptEntityOptions("\n请选择坡顶线或 ([设置S]/[锥坡Z])");
            proEntOpt.Keywords.Add("S");
            proEntOpt.Keywords.Add("Z");
            proEntOpt.Keywords.Default = "S";
            proEntOpt.SetRejectMessage("\n所选不是线型实体");
            proEntOpt.AddAllowedClass(typeof(Curve), false);
            proEntOpt.AllowNone = true;
            proEntOpt.AppendKeywordsToMessage = false;
            PromptEntityResult proEntRes = ed.GetEntity(proEntOpt);
            #endregion
            
            if (proEntRes.Status == PromptStatus.Keyword)
            {
                if (proEntRes.StringResult == "S")
                {
                    #region 设置垂直于坡顶还是坡底

                    PromptKeywordOptions proKeyOpt = new PromptKeywordOptions("\n示坡线垂直于([坡顶(A)]/[坡底(B)])");
                    proKeyOpt.Keywords.Add("A");
                    proKeyOpt.Keywords.Add("B");
                    if (top1)
                        proKeyOpt.Keywords.Default = "A";
                    else
                        proKeyOpt.Keywords.Default = "B";
                    proKeyOpt.AllowNone = true;
                    PromptResult proKeyRes = ed.GetKeywords(proKeyOpt);
                    if (proKeyRes.Status == PromptStatus.OK)
                    {
                        if (proKeyRes.StringResult == "A")
                            top1 = true;
                        else
                            top1 = false;
                    }
                    #endregion

                    #region 设置示坡线间距

                    PromptDoubleOptions proDouOpt = new PromptDoubleOptions("\n示坡线间距");
                    proDouOpt.AllowNegative = false;
                    proDouOpt.AllowZero = false;
                    proDouOpt.DefaultValue = step1;
                    proDouOpt.UseDefaultValue = true;
                    PromptDoubleResult proDouRes = ed.GetDouble(proDouOpt);
                    if (proDouRes.Status == PromptStatus.OK)
                    {
                        step1 = proDouRes.Value;
                    }
                    #endregion

                    #region 设置示坡线最大长度

                    proDouOpt.Message = "\n坡脚线最大长度";
                    proDouOpt.DefaultValue = maxLen;
                    proDouRes = ed.GetDouble(proDouOpt);
                    if (proDouRes.Status == PromptStatus.OK)
                    {
                        maxLen = proDouRes.Value;
                    }

                    #endregion

                    #region 设置后选择坡顶线

                    proEntOpt = new PromptEntityOptions("\n请选择坡顶线");
                    proEntOpt.SetRejectMessage("\n所选不是线型实体");
                    proEntOpt.AddAllowedClass(typeof(Curve), false);
                    proEntRes = ed.GetEntity(proEntOpt);
                    if (proEntRes.Status == PromptStatus.OK)
                    {
                        lhpub.LhObjId0 = proEntRes.ObjectId;
                        topCurve = (Curve)lhpub.LhIdToEntity();
                    }
                    else
                        return;
                    #endregion
                }
                else
                {
                    #region 选择锥坡顶点

                    PromptPointOptions proPtOpt = new PromptPointOptions("\n请选择锥坡顶点或 ([锥坡设置(S)])");
                    proPtOpt.Keywords.Add("S");
                    proPtOpt.Keywords.Default = "S";
                    proPtOpt.AppendKeywordsToMessage = false;
                    PromptPointResult proPtRes = ed.GetPoint(proPtOpt);
                    #endregion

                    if (proPtRes.Status == PromptStatus.Keyword)
                    {
                        #region 设置垂直于坡顶还是坡底

                        PromptKeywordOptions proKeyOpt = new PromptKeywordOptions("\n锥坡顶点作为([坡顶(A)]/[坡底(B)])");
                        proKeyOpt.Keywords.Add("A");
                        proKeyOpt.Keywords.Add("B");
                        if (top2)
                            proKeyOpt.Keywords.Default = "A";
                        else
                            proKeyOpt.Keywords.Default = "B";
                        
                        proKeyOpt.AllowNone = true;
                        PromptResult proKeyRes = ed.GetKeywords(proKeyOpt);
                        if (proKeyRes.Status == PromptStatus.OK)
                        {
                            if (proKeyRes.StringResult == "A")
                                top2 = true;
                            else
                                top2 = false;
                        }
                        #endregion

                        #region 设置示坡线间距

                        PromptDoubleOptions proDouOpt = new PromptDoubleOptions("\n示坡线间距");
                        proDouOpt.AllowNegative = false;
                        proDouOpt.AllowZero = false;
                        proDouOpt.DefaultValue = step2;
                        proDouOpt.UseDefaultValue = true;
                        PromptDoubleResult proDouRes = ed.GetDouble(proDouOpt);
                        if (proDouRes.Status == PromptStatus.OK)
                        {
                            step1 = proDouRes.Value;
                        }
                        #endregion

                        #region 设置后选择锥坡顶点

                        proPtRes = ed.GetPoint("\n请选择锥坡顶点");
                        if (proPtRes.Status == PromptStatus.OK)
                        {
                            isDrillSlope = true;
                            drillPt = proPtRes.Value;
                        }
                        else
                        {
                            ed.WriteMessage("\n选点有误!");
                            return;
                        }
                        #endregion
                    }
                    else if (proPtRes.Status == PromptStatus.OK)
                    {
                        isDrillSlope = true;
                        drillPt = proPtRes.Value;
                    }
                    else
                    {
                        ed.WriteMessage("\n选点有误!");
                        return;
                    }
                    
                }
            }
            else if (proEntRes.Status == PromptStatus.OK)
            {
                lhpub.LhObjId0 = proEntRes.ObjectId;
                topCurve = (Curve)lhpub.LhIdToEntity();
            }
            else
                return;

            #region 选择坡底线

            if (isDrillSlope)
                proEntOpt.Message = "\n请选择锥坡线";
            else
                proEntOpt.Message = "\n请选择坡底线";

            proEntOpt.SetRejectMessage("\n所选不是线型实体");
            proEntOpt.AddAllowedClass(typeof(Curve), false);
            proEntRes = ed.GetEntity(proEntOpt);
            if (proEntRes.Status == PromptStatus.OK)
            {
                lhpub.LhObjId0 = proEntRes.ObjectId;
                bottomCurve = (Curve)lhpub.LhIdToEntity();
            }
            else
                return;
            #endregion

            if (isDrillSlope)
                myDSS.DrawShortLine2(drillPt, bottomCurve, step2, myColor, top2, maxLen);
            else
                myDSS.DrawShortLine1(topCurve, bottomCurve, step1, myColor, top1, maxLen);

        }

    }



    public class DrawSlope
    {
        /// <summary>
        /// 根据指定的坡顶和坡底线，绘制示坡线中的小短线
        /// </summary>
        /// <param name="topCurve">坡顶线</param>
        /// <param name="bottomCurve">坡底线</param>
        /// <param name="step">小短线步长</param>
        /// <param name="colorIndex">小短线颜色</param>
        /// <param name="top">小短线垂直于坡顶线为true，垂直于坡底线为false</param>
        public void DrawShortLine1(Curve topCurve, Curve bottomCurve, double step, int colorIndex, bool top, double maxLen)
        {
            DotNetArxLH.LhDraw.LhLine lhline = new DotNetArxLH.LhDraw.LhLine();
            Curve myCurve1, myCurve2;
            double tmpDis, tmpLen, topCurveLen;
            Point3d pt1, pt2;
            Vector3d v1, v2, v;
            Line tmpLine;
            bool isWhole = true;
            List<Entity> myLines = new List<Entity>();

            if (top)
            {
                myCurve1 = topCurve;
                myCurve2 = bottomCurve;
            }
            else
            {
                myCurve1 = bottomCurve;
                myCurve2 = topCurve;
            }

            tmpDis = 0;
            topCurveLen = myCurve1.GetDistanceAtParameter(myCurve1.EndParam);

            while (tmpDis <= topCurveLen)
            {
                pt1 = myCurve1.GetClosestPointTo(myCurve1.GetPointAtDist(tmpDis), false);

                v1 = lhline.LhGetFirstDerivative(myCurve1, myCurve1.GetClosestPointTo(pt1, false)).GetNormal();

                v2 = v1.GetPerpendicularVector().GetNormal();
                v = pt1.GetAsVector() + v2;
                tmpLine = new Line(pt1, new Point3d(v.X, v.Y, 0));
                Point3dCollection interPt = lhline.LhIntersectWith(tmpLine, myCurve2, Intersect.ExtendThis);

                if (interPt.Count == 0)
                    pt2 = myCurve2.GetClosestPointTo(pt1, false);
                else
                {
                    pt2 = interPt[0];
                    tmpLen = pt1.DistanceTo(interPt[0]);
                    for (int j = 1; j < interPt.Count; j++)
                    {
                        if (pt1.DistanceTo(interPt[j]) < tmpLen)
                        {
                            pt2 = interPt[j];
                            tmpLen = pt1.DistanceTo(interPt[j]);
                        }
                    }
                }

                v = pt1.GetVectorTo(pt2).GetNormal();
                if (pt1.DistanceTo(pt2) > maxLen)
                {
                    if (top)
                        pt2 = pt1.Add(v * maxLen);
                    else
                        pt1 = pt2.Add(-v * maxLen);
                }

                if (isWhole)
                {
                    tmpLine = new Line(pt1, pt2);
                    tmpLine.ColorIndex = colorIndex;
                    myLines.Add(tmpLine);
                    isWhole = false;
                }
                else
                {
                    if (top)
                        tmpLine = new Line(pt1, new Point3d((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2, 0));
                    else
                        tmpLine = new Line(pt2, new Point3d((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2, 0));
                    tmpLine.ColorIndex = colorIndex;
                    myLines.Add(tmpLine);
                    isWhole = true;
                }

                tmpDis += step;
            }


            Entity[] myLineArray = new Entity[myLines.Count];
            myLines.CopyTo(myLineArray, 0);
            lhline.LhAddToDb(myLineArray);
        }



        /// <summary>
        /// 绘制锥坡中的小短线
        /// </summary>
        /// <param name="pt">锥坡顶点</param>
        /// <param name="bottomCurve">锥坡线</param>
        /// <param name="step">小短线步长</param>
        /// <param name="colorIndex">小短线颜色</param>
        /// <param name="top">锥坡顶点作为坡顶线为true，作为坡底线为false</param>
        public void DrawShortLine2(Point3d pt, Curve myCurve, double step, int colorIndex, bool top, double maxLen)
        {
            DotNetArxLH.LhDraw.LhLine lhline = new DotNetArxLH.LhDraw.LhLine();
            double tmpDis, myCurveLen;
            Point3d tmpPt1, tmpPt2;
            Line tmpLine;
            bool isWhole = true;
            Vector3d v;
            List<Entity> myLines = new List<Entity>();

            tmpDis = 0;
            myCurveLen = myCurve.GetDistanceAtParameter(myCurve.EndParam);

            while (tmpDis <= myCurveLen)
            {
                tmpPt1 = myCurve.GetClosestPointTo(myCurve.GetPointAtDist(tmpDis), false);
                tmpPt2 = pt;

                v = tmpPt2.GetVectorTo(tmpPt1).GetNormal();
                if (tmpPt1.DistanceTo(tmpPt2) > maxLen)
                {
                    if (top)
                        tmpPt1 = tmpPt2.Add(v * maxLen);
                    else
                        tmpPt2 = tmpPt1.Add(-v * maxLen);
                }

                if (isWhole)
                {
                    tmpLine = new Line(tmpPt1, tmpPt2);
                    tmpLine.ColorIndex = colorIndex;
                    myLines.Add(tmpLine);
                    isWhole = false;
                }
                else
                {
                    if (top)
                        tmpLine = new Line(tmpPt2, new Point3d((tmpPt1.X + tmpPt2.X) / 2, (tmpPt1.Y + tmpPt2.Y) / 2, 0));
                    else
                        tmpLine = new Line(tmpPt1, new Point3d((tmpPt1.X + tmpPt2.X) / 2, (tmpPt1.Y + tmpPt2.Y) / 2, 0));
                    tmpLine.ColorIndex = colorIndex;
                    myLines.Add(tmpLine);
                    isWhole = true;
                }

                tmpDis += step;
            }

            Entity[] myLineArray = new Entity[myLines.Count];
            myLines.CopyTo(myLineArray, 0);
            lhline.LhAddToDb(myLineArray);
        }
    }
}