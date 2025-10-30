using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Path.Views
{
    /// <summary>
    /// PathEditor3D�����ܲ�׽������չ
    /// </summary>
    public partial class PathEditor3D
    {
  #region ���ܲ�׽�ֶ�

        /// <summary>
  /// ��׽����ö��
    /// </summary>
   private enum SnapType3D
        {
 None,
 Grid,        // ����׽
            Endpoint,    // �˵㲶׽
  Midpoint,    // �е㲶׽
            Point,       // ���Ƶ㲶׽
       Projection   // ͶӰ��׽���㵽�߶εĴ��㣩
        }

        /// <summary>
        /// ���ܲ�׽����
        /// </summary>
      private class SmartSnapSettings3D
        {
            public bool GridSnap { get; set; } = true;
        public bool PointSnap { get; set; } = true;
            public bool MidpointSnap { get; set; } = false;
public bool EndpointSnap { get; set; } = true;
        public bool ProjectionSnap { get; set; } = false;
        public double SnapDistance { get; set; } = 15.0; // ��Ļ����
        }

    private SmartSnapSettings3D _snapSettings3D = new SmartSnapSettings3D();
 private SnapType3D _currentSnapType3D = SnapType3D.None;
        private Point3D? _currentSnapPoint3D = null;

        // ��׽ָʾ���Ӿ�����
        private SphereVisual3D? _snapIndicatorSphere = null;
        private LinesVisual3D? _snapIndicatorCross = null;
   private BillboardTextVisual3D? _snapIndicatorLabel = null;

     #endregion

      #region ���ܲ�׽�����㷨

        /// <summary>
        /// Ӧ�����ܲ�׽��3D��
    /// </summary>
   private Point3D ApplySmartSnapping3D(Point3D rawPoint, Point screenPos)
   {
          _currentSnapType3D = SnapType3D.None;
      _currentSnapPoint3D = null;

            var result = rawPoint;

        // ������Ļ���ص����絥λ��ת������̬��
  var worldPerPixel = EstimateWorldPerPixel(screenPos);
    var snapDistanceWorld = _snapSettings3D.SnapDistance * worldPerPixel;

            // 1. �˵㲶׽��������ȼ���
  if (_snapSettings3D.EndpointSnap)
            {
      var endpoint = FindNearestEndpoint3D(rawPoint, snapDistanceWorld);
                if (endpoint != null)
       {
     result = endpoint.Value;
       _currentSnapType3D = SnapType3D.Endpoint;
  _currentSnapPoint3D = result;
      ShowSnapIndicator3D(result, SnapType3D.Endpoint);
         return result;
   }
      }

     // 2. ���Ƶ㲶׽
            if (_snapSettings3D.PointSnap)
     {
           var controlPoint = FindNearestControlPoint3D(rawPoint, snapDistanceWorld);
       if (controlPoint != null)
      {
  result = controlPoint.Value;
 _currentSnapType3D = SnapType3D.Point;
     _currentSnapPoint3D = result;
   ShowSnapIndicator3D(result, SnapType3D.Point);
      return result;
    }
            }

            // 3. �е㲶׽
            if (_snapSettings3D.MidpointSnap)
    {
      var midpoint = FindNearestMidpoint3D(rawPoint, snapDistanceWorld);
         if (midpoint != null)
             {
 result = midpoint.Value;
             _currentSnapType3D = SnapType3D.Midpoint;
   _currentSnapPoint3D = result;
      ShowSnapIndicator3D(result, SnapType3D.Midpoint);
              return result;
       }
   }

            // 4. ͶӰ��׽�����㣩
            if (_snapSettings3D.ProjectionSnap)
          {
      var projection = FindNearestProjection3D(rawPoint, snapDistanceWorld);
 if (projection != null)
                {
     result = projection.Value;
      _currentSnapType3D = SnapType3D.Projection;
        _currentSnapPoint3D = result;
          ShowSnapIndicator3D(result, SnapType3D.Projection);
        return result;
       }
  }

     // 5. ����׽��������ȼ���
      if (_snapSettings3D.GridSnap)
            {
       double gridSize = _gridSpacing; // ʹ�����õ�������
    result = new Point3D(
 Math.Round(result.X / gridSize) * gridSize,
       Math.Round(result.Y / gridSize) * gridSize,
           Math.Round(result.Z / gridSize) * gridSize
         );
             _currentSnapType3D = SnapType3D.Grid;
           _currentSnapPoint3D = result;
        ShowSnapIndicator3D(result, SnapType3D.Grid);
     }
            else
            {
                HideSnapIndicator3D();
        }

            return result;
        }

 /// <summary>
        /// ������Ļ���ص����絥λ��ת������
        /// </summary>
        private double EstimateWorldPerPixel(Point screenPos)
        {
          try
     {
       var planeHit = HitPlaneAtPoint(screenPos);
              if (!planeHit.HasValue) return 1.0;

     Point screenRight = new Point(screenPos.X + 1.0, screenPos.Y);
        Point screenDown = new Point(screenPos.X, screenPos.Y + 1.0);
 var rightHit = HitPlaneAtPoint(screenRight);
       var downHit = HitPlaneAtPoint(screenDown);

    double dx = (rightHit.HasValue) ? DistanceXY(planeHit.Value, rightHit.Value) : 0.0;
       double dy = (downHit.HasValue) ? DistanceXY(planeHit.Value, downHit.Value) : 0.0;

        if (dx > 1e-9 && dy > 1e-9) return (dx + dy) * 0.5;
                else if (dx > 1e-9) return dx;
  else if (dy > 1e-9) return dy;
            }
            catch { }
            return 1.0;
        }

  /// <summary>
        /// ��������Ķ˵㣨�߼����꣩
   /// </summary>
        private Point3D? FindNearestEndpoint3D(Point3D point, double threshold)
        {
          Point3D? nearest = null;
      double minDist = threshold;

            if (_lastUsvs == null) return null;

   foreach (var usv in _lastUsvs)
            {
    foreach (var seg in usv.Segments)
      {
      if (seg.ControlPoints.Count == 0) continue;

          // �׵�
       var first = seg.ControlPoints[0];
        var dist = DistanceXY(point, first);
     if (dist < minDist)
         {
  minDist = dist;
            nearest = first;
  }

         // ĩ��
       var last = seg.ControlPoints[seg.ControlPoints.Count - 1];
               dist = DistanceXY(point, last);
     if (dist < minDist)
 {
      minDist = dist;
    nearest = last;
   }
        }
            }

            return nearest;
        }

        /// <summary>
     /// ��������Ŀ��Ƶ㣨�߼����꣩
 /// </summary>
        private Point3D? FindNearestControlPoint3D(Point3D point, double threshold)
        {
        Point3D? nearest = null;
  double minDist = threshold;

 foreach (var kv in _logicalPositions)
            {
   var cp = kv.Value;
          var dist = DistanceXY(point, cp);
       if (dist < minDist)
                {
    minDist = dist;
      nearest = cp;
        }
   }

         return nearest;
        }

   /// <summary>
        /// ����������е㣨�߼����꣩
        /// </summary>
        private Point3D? FindNearestMidpoint3D(Point3D point, double threshold)
        {
            Point3D? nearest = null;
  double minDist = threshold;

        if (_lastUsvs == null) return null;

            foreach (var usv in _lastUsvs)
        {
     foreach (var seg in usv.Segments)
         {
     for (int i = 0; i + 1 < seg.ControlPoints.Count; i++)
             {
            var a = seg.ControlPoints[i];
            var b = seg.ControlPoints[i + 1];
            var midpoint = new Point3D(
             (a.X + b.X) / 2,
        (a.Y + b.Y) / 2,
       (a.Z + b.Z) / 2
   );

         var dist = DistanceXY(point, midpoint);
       if (dist < minDist)
            {
      minDist = dist;
        nearest = midpoint;
       }
     }
      }
    }

  return nearest;
        }

        /// <summary>
      /// ���������ͶӰ�㣨�㵽�߶εĴ��㣩
 /// </summary>
        private Point3D? FindNearestProjection3D(Point3D point, double threshold)
        {
            Point3D? nearest = null;
     double minDist = threshold;

     if (_lastUsvs == null) return null;

            foreach (var usv in _lastUsvs)
   {
       foreach (var seg in usv.Segments)
    {
            for (int i = 0; i + 1 < seg.ControlPoints.Count; i++)
     {
             var a = seg.ControlPoints[i];
    var b = seg.ControlPoints[i + 1];

        // ����ͶӰ��
                var projection = ProjectPointToSegment3D(point, a, b);
             var dist = DistanceXY(point, projection);

          if (dist < minDist)
      {
         minDist = dist;
         nearest = projection;
    }
       }
    }
            }

     return nearest;
   }

        /// <summary>
        /// ����ͶӰ��3D�߶���
    /// </summary>
        private Point3D ProjectPointToSegment3D(Point3D p, Point3D a, Point3D b)
        {
  var v = new Vector3D(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
         var w = new Vector3D(p.X - a.X, p.Y - a.Y, p.Z - a.Z);

            var c1 = Vector3D.DotProduct(w, v);
 if (c1 <= 0) return a;

          var c2 = Vector3D.DotProduct(v, v);
            if (c2 <= c1) return b;

          var t = c1 / c2;
   return new Point3D(
     a.X + t * v.X,
                a.Y + t * v.Y,
      a.Z + t * v.Z
            );
     }

        #endregion

        #region �Ӿ�����

        /// <summary>
        /// ��ʾ3D��׽ָʾ��
        /// </summary>
      private void ShowSnapIndicator3D(Point3D logicalPoint, SnapType3D snapType)
        {
      try
         {
     // ����ɵ�ָʾ��
         HideSnapIndicator3D();

             // ת��Ϊ�Ӿ�����
var visualPoint = LogicalToVisual(logicalPoint);

   // ������׽ָʾ��
     Color color;
      string typeName;
     double radius;

           switch (snapType)
           {
        case SnapType3D.Endpoint:
    color = Colors.Red;
         typeName = "�˵�";
  radius = 8.0 * _visualScale;
         break;

             case SnapType3D.Point:
     color = Colors.Lime;
       typeName = "���Ƶ�";
       radius = 8.0 * _visualScale;
         break;

      case SnapType3D.Midpoint:
    color = Colors.Cyan;
       typeName = "�е�";
        radius = 8.0 * _visualScale;
        break;

            case SnapType3D.Projection:
            color = Colors.Magenta;
typeName = "ͶӰ";
             radius = 7.0 * _visualScale;
        break;

   case SnapType3D.Grid:
                color = Colors.Gray;
        typeName = "����";
       radius = 5.0 * _visualScale;
                 break;

    default:
     return;
         }

    // ������͸������ָʾ��
                _snapIndicatorSphere = new SphereVisual3D
    {
       Center = visualPoint,
       Radius = radius,
         Fill = new SolidColorBrush(Color.FromArgb(128, color.R, color.G, color.B))
             };
              SceneRoot.Children.Add(_snapIndicatorSphere);

           // ����ʮ����ָʾ��
    _snapIndicatorCross = new LinesVisual3D
     {
       Color = color,
                    Thickness = 2,
 Points = new Point3DCollection()
        };

    double crossSize = 15.0 * _visualScale;
       // X��ʮ��
     _snapIndicatorCross.Points.Add(new Point3D(visualPoint.X - crossSize, visualPoint.Y, visualPoint.Z));
           _snapIndicatorCross.Points.Add(new Point3D(visualPoint.X + crossSize, visualPoint.Y, visualPoint.Z));
  // Y��ʮ��
      _snapIndicatorCross.Points.Add(new Point3D(visualPoint.X, visualPoint.Y - crossSize, visualPoint.Z));
              _snapIndicatorCross.Points.Add(new Point3D(visualPoint.X, visualPoint.Y + crossSize, visualPoint.Z));

   SceneRoot.Children.Add(_snapIndicatorCross);

 // �������ֱ�ǩ
   var displayX = logicalPoint.X * _exportScale;
     var displayY = logicalPoint.Y * _exportScale;
       var displayZ = logicalPoint.Z * _exportScale;

    _snapIndicatorLabel = new BillboardTextVisual3D
           {
           Text = $"?? {typeName}\n({displayX:F1}, {displayY:F1}, {displayZ:F1})",
Position = new Point3D(
  visualPoint.X + 10 * _visualScale,
         visualPoint.Y + 10 * _visualScale,
        visualPoint.Z + 10 * _visualScale
      ),
        Foreground = Brushes.Black,
       Background = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
  FontSize = 12,
     FontWeight = FontWeights.Bold
                };
    SceneRoot.Children.Add(_snapIndicatorLabel);
            }
            catch { }
  }

        /// <summary>
        /// ����3D��׽ָʾ��
        /// </summary>
        private void HideSnapIndicator3D()
    {
            try
         {
       if (_snapIndicatorSphere != null)
              {
  SceneRoot.Children.Remove(_snapIndicatorSphere);
          _snapIndicatorSphere = null;
       }

 if (_snapIndicatorCross != null)
  {
     SceneRoot.Children.Remove(_snapIndicatorCross);
     _snapIndicatorCross = null;
   }

        if (_snapIndicatorLabel != null)
            {
            SceneRoot.Children.Remove(_snapIndicatorLabel);
         _snapIndicatorLabel = null;
             }
            }
         catch { }
   }

        #endregion

 #region �����ӿ�

 /// <summary>
   /// ���ò�׽ѡ��
   /// </summary>
        public void SetSnapOptions(bool gridSnap, bool pointSnap, bool midpointSnap, bool endpointSnap, bool projectionSnap, double snapDistance)
        {
       _snapSettings3D.GridSnap = gridSnap;
      _snapSettings3D.PointSnap = pointSnap;
        _snapSettings3D.MidpointSnap = midpointSnap;
         _snapSettings3D.EndpointSnap = endpointSnap;
            _snapSettings3D.ProjectionSnap = projectionSnap;
         _snapSettings3D.SnapDistance = snapDistance;
        }

        /// <summary>
      /// �л�����׽
     /// </summary>
        public void ToggleGridSnap()
        {
            _snapSettings3D.GridSnap = !_snapSettings3D.GridSnap;
     }

      /// <summary>
        /// �л��㲶׽
    /// </summary>
        public void TogglePointSnap()
        {
  _snapSettings3D.PointSnap = !_snapSettings3D.PointSnap;
        }

   /// <summary>
        /// �л��е㲶׽
        /// </summary>
        public void ToggleMidpointSnap()
        {
     _snapSettings3D.MidpointSnap = !_snapSettings3D.MidpointSnap;
  }

        /// <summary>
        /// �л��˵㲶׽
        /// </summary>
        public void ToggleEndpointSnap()
        {
            _snapSettings3D.EndpointSnap = !_snapSettings3D.EndpointSnap;
        }

 /// <summary>
        /// �л�ͶӰ��׽
        /// </summary>
        public void ToggleProjectionSnap()
      {
            _snapSettings3D.ProjectionSnap = !_snapSettings3D.ProjectionSnap;
        }

   #endregion
    }
}
