using System.IO;

namespace Path.Services.Step214
{
    /// <summary>
    /// STEP 214 示例文件生成器（用于测试）
    /// </summary>
    public class Step214SampleGenerator
    {
   /// <summary>
   /// 生成包含多种曲线类型的示例 STEP 文件
        /// </summary>
        public static void GenerateSampleFile(string filePath)
    {
     var content = @"ISO-10303-21;
HEADER;
FILE_DESCRIPTION(('Sample STEP file with curves'),'2;1');
FILE_NAME('sample_curves.step','2024-01-01T12:00:00',('PathEditor'),('Demo'),'','Preprocessor','');
FILE_SCHEMA(('AUTOMOTIVE_DESIGN { 1 0 10303 214 1 1 1 1 }'));
ENDSEC;

DATA;
/* 笛卡尔坐标点 */
#10 = CARTESIAN_POINT('',(0.0,0.0,0.0));
#11 = CARTESIAN_POINT('',(10.0,0.0,0.0));
#12 = CARTESIAN_POINT('',(20.0,0.0,0.0));
#13 = CARTESIAN_POINT('',(30.0,0.0,0.0));

#20 = CARTESIAN_POINT('',(0.0,10.0,0.0));
#21 = CARTESIAN_POINT('',(5.0,15.0,0.0));
#22 = CARTESIAN_POINT('',(10.0,10.0,0.0));
#23 = CARTESIAN_POINT('',(15.0,5.0,0.0));
#24 = CARTESIAN_POINT('',(20.0,10.0,0.0));

#30 = CARTESIAN_POINT('',(0.0,20.0,0.0));
#31 = CARTESIAN_POINT('',(10.0,25.0,0.0));
#32 = CARTESIAN_POINT('',(20.0,20.0,0.0));

/* 折线示例 */
#100 = ( LENGTH_MEASURE(#10,#11,#12,#13) );
#101 = POLYLINE('straight_line',#100);

/* B-Spline 曲线示例 */
#200 = ( LENGTH_MEASURE(#20,#21,#22,#23,#24) );
#201 = B_SPLINE_CURVE('bspline_curve',3,#200,.UNSPECIFIED.,.F.,.F.);

/* Bezier 曲线示例 */
#300 = ( LENGTH_MEASURE(#30,#31,#32) );
#301 = BEZIER_CURVE('bezier_curve',2,#300);

/* 圆形示例 */
#400 = AXIS2_PLACEMENT_3D('',#10,$,$);
#401 = CIRCLE('circle',#400,5.0);

/* 椭圆示例 */
#500 = AXIS2_PLACEMENT_3D('',#20,$,$);
#501 = ELLIPSE('ellipse',#500,8.0,5.0);

ENDSEC;
END-ISO-10303-21;
";

      File.WriteAllText(filePath, content);
 }

      /// <summary>
    /// 生成船体型线示例 STEP 文件
  /// </summary>
        public static void GenerateHullLinesFile(string filePath)
        {
  var content = @"ISO-10303-21;
HEADER;
FILE_DESCRIPTION(('Hull lines for USV path planning'),'2;1');
FILE_NAME('hull_lines.step','2024-01-01T12:00:00',('PathEditor'),('USV Design'),'','','');
FILE_SCHEMA(('AUTOMOTIVE_DESIGN { 1 0 10303 214 1 1 1 1 }'));
ENDSEC;

DATA;
/* 龙骨线 (Keel Line) */
#10 = CARTESIAN_POINT('',(-50.0,0.0,0.0));
#11 = CARTESIAN_POINT('',(-40.0,0.0,-0.5));
#12 = CARTESIAN_POINT('',(-30.0,0.0,-1.0));
#13 = CARTESIAN_POINT('',(-20.0,0.0,-1.2));
#14 = CARTESIAN_POINT('',(-10.0,0.0,-1.3));
#15 = CARTESIAN_POINT('',(0.0,0.0,-1.2));
#16 = CARTESIAN_POINT('',(10.0,0.0,-1.0));
#17 = CARTESIAN_POINT('',(20.0,0.0,-0.6));
#18 = CARTESIAN_POINT('',(30.0,0.0,0.0));
#100 = ( LENGTH_MEASURE(#10,#11,#12,#13,#14,#15,#16,#17,#18) );
#101 = B_SPLINE_CURVE('keel_line',3,#100,.UNSPECIFIED.,.F.,.F.);

/* 水线 1 (Waterline 1) */
#20 = CARTESIAN_POINT('',(-45.0,0.0,-0.8));
#21 = CARTESIAN_POINT('',(-40.0,2.0,-0.8));
#22 = CARTESIAN_POINT('',(-30.0,3.0,-0.8));
#23 = CARTESIAN_POINT('',(-20.0,3.5,-0.8));
#24 = CARTESIAN_POINT('',(-10.0,4.0,-0.8));
#25 = CARTESIAN_POINT('',(0.0,4.0,-0.8));
#26 = CARTESIAN_POINT('',(10.0,3.5,-0.8));
#27 = CARTESIAN_POINT('',(20.0,3.0,-0.8));
#28 = CARTESIAN_POINT('',(25.0,2.0,-0.8));
#200 = ( LENGTH_MEASURE(#20,#21,#22,#23,#24,#25,#26,#27,#28) );
#201 = B_SPLINE_CURVE('waterline_1',3,#200,.UNSPECIFIED.,.F.,.F.);

/* 水线 2 (Waterline 2) */
#30 = CARTESIAN_POINT('',(-40.0,0.0,-0.4));
#31 = CARTESIAN_POINT('',(-35.0,3.0,-0.4));
#32 = CARTESIAN_POINT('',(-25.0,4.5,-0.4));
#33 = CARTESIAN_POINT('',(-15.0,5.0,-0.4));
#34 = CARTESIAN_POINT('',(0.0,5.5,-0.4));
#35 = CARTESIAN_POINT('',(15.0,5.0,-0.4));
#36 = CARTESIAN_POINT('',(25.0,3.0,-0.4));
#300 = ( LENGTH_MEASURE(#30,#31,#32,#33,#34,#35,#36) );
#301 = B_SPLINE_CURVE('waterline_2',3,#300,.UNSPECIFIED.,.F.,.F.);

/* 水线 3 (Waterline 3 - Deck) */
#40 = CARTESIAN_POINT('',(-35.0,0.0,0.0));
#41 = CARTESIAN_POINT('',(-30.0,4.0,0.0));
#42 = CARTESIAN_POINT('',(-20.0,5.5,0.0));
#43 = CARTESIAN_POINT('',(-10.0,6.0,0.0));
#44 = CARTESIAN_POINT('',(0.0,6.2,0.0));
#45 = CARTESIAN_POINT('',(10.0,6.0,0.0));
#46 = CARTESIAN_POINT('',(20.0,5.0,0.0));
#47 = CARTESIAN_POINT('',(28.0,2.0,0.0));
#400 = ( LENGTH_MEASURE(#40,#41,#42,#43,#44,#45,#46,#47) );
#401 = B_SPLINE_CURVE('deck_line',3,#400,.UNSPECIFIED.,.F.,.F.);

ENDSEC;
END-ISO-10303-21;
";

            File.WriteAllText(filePath, content);
        }
    }
}
