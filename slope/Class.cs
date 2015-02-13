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
            
            double step1 = 1;   //��ͨʾ���߼�ࣨ������ΪĬ��ֵ��
            double step2 = 1;   //׶��ʾ���߼�ࣨ������ΪĬ��ֵ��
            bool top1 = true;   //��ͨʾ���ߴ�ֱ���¶������µף�������ΪĬ��ֵ��
            bool top2 = true;   //׶�¶�����Ϊ�¶������µף�������ΪĬ��ֵ��
            double maxLen = double.MaxValue;    //ʾ�³�����󳤶�

            
            DotNetArxLH.LhDraw.LhPublic lhpub = new DotNetArxLH.LhDraw.LhPublic();
            bool isDrillSlope = false;  //�Ƿ�׶��
            Point3d drillPt;            //׶�¶���
            Curve topCurve = null, bottomCurve; //�¶��ߺ��µ���
            int myColor = 3;

            #region ѡ���¶���

            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityOptions proEntOpt = new PromptEntityOptions("\n��ѡ���¶��߻� ([����S]/[׶��Z])");
            proEntOpt.Keywords.Add("S");
            proEntOpt.Keywords.Add("Z");
            proEntOpt.Keywords.Default = "S";
            proEntOpt.SetRejectMessage("\n��ѡ��������ʵ��");
            proEntOpt.AddAllowedClass(typeof(Curve), false);
            proEntOpt.AllowNone = true;
            proEntOpt.AppendKeywordsToMessage = false;
            PromptEntityResult proEntRes = ed.GetEntity(proEntOpt);
            #endregion
            
            if (proEntRes.Status == PromptStatus.Keyword)
            {
                if (proEntRes.StringResult == "S")
                {
                    #region ���ô�ֱ���¶������µ�

                    PromptKeywordOptions proKeyOpt = new PromptKeywordOptions("\nʾ���ߴ�ֱ��([�¶�(A)]/[�µ�(B)])");
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

                    #region ����ʾ���߼��

                    PromptDoubleOptions proDouOpt = new PromptDoubleOptions("\nʾ���߼��");
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

                    #region ����ʾ������󳤶�

                    proDouOpt.Message = "\n�½�����󳤶�";
                    proDouOpt.DefaultValue = maxLen;
                    proDouRes = ed.GetDouble(proDouOpt);
                    if (proDouRes.Status == PromptStatus.OK)
                    {
                        maxLen = proDouRes.Value;
                    }

                    #endregion

                    #region ���ú�ѡ���¶���

                    proEntOpt = new PromptEntityOptions("\n��ѡ���¶���");
                    proEntOpt.SetRejectMessage("\n��ѡ��������ʵ��");
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
                    #region ѡ��׶�¶���

                    PromptPointOptions proPtOpt = new PromptPointOptions("\n��ѡ��׶�¶���� ([׶������(S)])");
                    proPtOpt.Keywords.Add("S");
                    proPtOpt.Keywords.Default = "S";
                    proPtOpt.AppendKeywordsToMessage = false;
                    PromptPointResult proPtRes = ed.GetPoint(proPtOpt);
                    #endregion

                    if (proPtRes.Status == PromptStatus.Keyword)
                    {
                        #region ���ô�ֱ���¶������µ�

                        PromptKeywordOptions proKeyOpt = new PromptKeywordOptions("\n׶�¶�����Ϊ([�¶�(A)]/[�µ�(B)])");
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

                        #region ����ʾ���߼��

                        PromptDoubleOptions proDouOpt = new PromptDoubleOptions("\nʾ���߼��");
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

                        #region ���ú�ѡ��׶�¶���

                        proPtRes = ed.GetPoint("\n��ѡ��׶�¶���");
                        if (proPtRes.Status == PromptStatus.OK)
                        {
                            isDrillSlope = true;
                            drillPt = proPtRes.Value;
                        }
                        else
                        {
                            ed.WriteMessage("\nѡ������!");
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
                        ed.WriteMessage("\nѡ������!");
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

            #region ѡ���µ���

            if (isDrillSlope)
                proEntOpt.Message = "\n��ѡ��׶����";
            else
                proEntOpt.Message = "\n��ѡ���µ���";

            proEntOpt.SetRejectMessage("\n��ѡ��������ʵ��");
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
        /// ����ָ�����¶����µ��ߣ�����ʾ�����е�С����
        /// </summary>
        /// <param name="topCurve">�¶���</param>
        /// <param name="bottomCurve">�µ���</param>
        /// <param name="step">С���߲���</param>
        /// <param name="colorIndex">С������ɫ</param>
        /// <param name="top">С���ߴ�ֱ���¶���Ϊtrue����ֱ���µ���Ϊfalse</param>
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
        /// ����׶���е�С����
        /// </summary>
        /// <param name="pt">׶�¶���</param>
        /// <param name="bottomCurve">׶����</param>
        /// <param name="step">С���߲���</param>
        /// <param name="colorIndex">С������ɫ</param>
        /// <param name="top">׶�¶�����Ϊ�¶���Ϊtrue����Ϊ�µ���Ϊfalse</param>
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