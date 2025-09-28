using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Documents;

namespace Path.Views
{
    public partial class PathEditor : UserControl
    {
        private enum SegmentType { Polyline, Arc }
        private class Segment
        {
            public SegmentType Type { get; set; }
            public List<Point> ControlPoints { get; } = new();
            // preserved samples for this segment (from Confirm Samples)
            public List<Point> ConfirmedSamples { get; set; } = new();

            public Segment(SegmentType type) { Type = type; }
            public List<Point> GetSamples(double spacing)
            {
                if (Type == SegmentType.Polyline)
                {
                    var all = new List<Point>();
                    var pts = ControlPoints;
                    for (int i = 0; i + 1 < pts.Count; i++)
                    {
                        var a = pts[i];
                        var b = pts[i + 1];
                        var seg = SampleSegment(a, b, spacing, includeLast: i + 1 == pts.Count - 1);
                        if (all.Count > 0 && seg.Count > 0)
                        {
                            if (Math.Abs(all.Last().X - seg.First().X) < 1e-6 && Math.Abs(all.Last().Y - seg.First().Y) < 1e-6)
                                seg.RemoveAt(0);
                        }
                        all.AddRange(seg);
                    }
                    if (pts.Count == 1) all.Add(pts[0]);
                    return all;
                }
                else
                {
                    if (ControlPoints.Count < 3) return new List<Point>();
                    return SampleArcByThreePoints(ControlPoints[0], ControlPoints[1], ControlPoints[2], spacing) ?? new List<Point>();
                }
            }
        }

        private class Usv
        {
            public List<Segment> Segments { get; } = new();
            public string Name { get; set; } = "usv_01";
            public Brush Color { get; set; } = Brushes.SteelBlue;
            public List<Point> GetSamples(double spacing)
            {
                var all = new List<Point>();
                foreach (var seg in Segments)
                {
                    var s = seg.GetSamples(spacing);
                    if (all.Count > 0 && s.Count > 0)
                    {
                        if (Math.Abs(all.Last().X - s.First().X) < 1e-6 && Math.Abs(all.Last().Y - s.First().Y) < 1e-6)
                            s.RemoveAt(0);
                    }
                    all.AddRange(s);
                }
                return all;
            }
            public Point? FirstPoint() { foreach (var seg in Segments) if (seg.ControlPoints.Count>0) return seg.ControlPoints.First(); return null; }
            public Point? LastPoint() { for (int i=Segments.Count-1;i>=0;i--) if (Segments[i].ControlPoints.Count>0) return Segments[i].ControlPoints.Last(); return null; }
        }

        private readonly List<Usv> _usvs = new();
        // preserved flattened confirmed samples per USV index to ensure persistence
        private readonly List<List<Point>> _preservedPerUsv = new();
        private List<List<Point>>? _generatedPerUsv;
        private int _selectedUsvIndex = -1;
        private int _selectedSegmentIndex = -1; // -1 = no specific segment selected
        private readonly List<Point> _arcBuffer = new();
        private double _scale = 1.0;
        private double _translateX = 0.0;
        private double _translateY = 0.0;
        private bool _isPanning = false;
        private Point _panStartPoint;
        private double _panStartHOffset;
        private double _panStartVOffset;

        // display unit multiplier: show coordinates and labels multiplied by this factor (0.1 = shrink 10x)
        private double _displayUnitScale = 0.1;

        private static readonly Brush[] _palette = new Brush[] { Brushes.SteelBlue, Brushes.Orange, Brushes.Green, Brushes.Purple, Brushes.Brown, Brushes.Teal, Brushes.Magenta, Brushes.Gold };

        private Canvas CanvasElement => (Canvas)FindName("DrawCanvas");
        private Canvas OverlayCanvasElement => (Canvas)FindName("OverlayCanvas");
        private ScrollViewer CanvasScrollViewerElement => (ScrollViewer)FindName("CanvasScrollViewer");
        private TextBox SpacingTextBox => (TextBox)FindName("TbSpacing");
        private TextBox UsvNameTextBox => (TextBox)FindName("TbUsvName");
        private ComboBox ColorCombo => (ComboBox)FindName("CbColor");
        private TextBlock PropTextBlock => (TextBlock)FindName("TbProp");
        private RadioButton ArcRadio => (RadioButton)FindName("RbArc");
        private RadioButton PolyRadio => (RadioButton)FindName("RbPolyline");

        public PathEditor()
        {
            InitializeComponent();
            EnsureCurrentUsv();
            UpdateColorComboSelection();
            // allow Delete key to remove selected segment
            try { this.Focusable = true; this.KeyDown += PathEditor_KeyDown; } catch { }
            // wire Enter key on spacing textbox to confirm sample count; also listen to PreviewKeyDown
            try
            {
                var tb = SpacingTextBox;
                if (tb != null)
                {
                    tb.KeyDown += SpacingTextBox_KeyDown;
                    tb.PreviewKeyDown += SpacingTextBox_KeyDown;
                }
            }
            catch { }
            // ensure Confirm button reliablywired (some XAML reload scenarios may lose hookup)
            try
            {
                var btn = FindName("BtnConfirmSamples") as Button;
                if (btn != null) btn.Click += BtnConfirmSamples_Click;
            }
            catch { }
            Redraw();
        }

        private void SpacingTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnConfirmSamples_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private void EnsureCurrentUsv()
        {
            if (_usvs.Count == 0)
            {
                var u = new Usv { Name = GenerateUsvName(1), Color = _palette[0] };
                // start with an empty polyline segment so clicks add points
                u.Segments.Add(new Segment(SegmentType.Polyline));
                _usvs.Add(u);
                _preservedPerUsv.Add(new List<Point>());
                _selectedUsvIndex = 0;
                UpdatePropertiesPanel();
                return;
            }

            // 如果已有 USV 但没有选中，选第一个（或其它合理策略）
            if (_selectedUsvIndex < 0 || _selectedUsvIndex >= _usvs.Count)
            {
                _selectedUsvIndex = 0;
                UpdatePropertiesPanel();
            }
        }

        private static string GenerateUsvName(int index) => $"usv_{index:00}";

        // Update _generatedPerUsv from preserved per-usv samples
        private void UpdateGeneratedFromPreserved()
        {
            if (_preservedPerUsv.Count == 0) { _generatedPerUsv = null; return; }
            var any = false; var gen = new List<List<Point>>();
            for (int i = 0; i < _preservedPerUsv.Count; i++)
            {
                var list = _preservedPerUsv[i] ?? new List<Point>();
                gen.Add(new List<Point>(list));
                if (list.Count > 0) any = true;
            }
            _generatedPerUsv = any ? gen : null;
        }

        private void DrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // use canvas-space coordinates directly
            var canvasPos = e.GetPosition(CanvasElement);
            var logical = ScreenToLogical(canvasPos);
            _lastDebugText = $"\n\nDebug:\nCanvas: {canvasPos.X:F2},{canvasPos.Y:F2}\nLogical: {logical.X:F2},{logical.Y:F2}\nDisplay: {logical.X * _displayUnitScale:F2},{logical.Y * _displayUnitScale:F2}";

            // Hit-test overlay rectangles (control points) by logical coordinates to start dragging
            // Prefer WPF hit testing on the overlay canvas (handles transforms/scrolling correctly)
            if (OverlayCanvasElement != null)
            {
                var hit = System.Windows.Media.VisualTreeHelper.HitTest(OverlayCanvasElement, e.GetPosition(OverlayCanvasElement));
                if (hit?.VisualHit is FrameworkElement fhe && fhe.Tag is ValueTuple<int, int, int> tag)
                {
                    var (ui, si, pi) = tag;
                    // start dragging this control point (collect linked points)
                    StartDraggingPoint(ui, si, pi);
                    _draggingElement = fhe;
                    try { fhe.CaptureMouse(); } catch { }
                    e.Handled = true;
                }
            }

            // first: selection test (try to select a single segment or USV)
            // If the last action created a new USV we want the first click(s) to add points, not re-select an existing segment.
            if (!_suppressSelectionOnNextClick && TrySelectUsvAt(logical)) { Redraw(); return; }
            // DO NOT reset _suppressSelectionOnNextClick here — keep suppression until we actually add points to the new USV.

            if (_selectedUsvIndex < 0) EnsureCurrentUsv();
            var cur = _usvs[_selectedUsvIndex];

            if (ArcRadio != null && ArcRadio.IsChecked == true)
            {
                // arc input: support seeding start point from previous segment end so arcs can be continuous
                // If buffer is empty and current USV has a last point, seed it as the arc start and treat
                // the first user click as the mid/control point. After two user clicks (mid + end) we have start/mid/end.
                if (_arcBuffer.Count == 0)
                {
                    var last = cur.LastPoint();
                    if (last.HasValue)
                    {
                        // seed start
                        _arcBuffer.Add(last.Value);
                        // now push the user's click as the middle control point
                        _arcBuffer.Add(logical);
                    }
                    else
                    {
                        // no existing start: collect normally
                        _arcBuffer.Add(logical);
                    }
                }
                else
                {
                    // subsequent click -> add as next point
                    _arcBuffer.Add(logical);
                }

                // We need 3 points (start,middle,end). When we have 3, create arc.
                if (_arcBuffer.Count == 3)
                {
                    var arcSeg = new Segment(SegmentType.Arc);
                    arcSeg.ControlPoints.AddRange(_arcBuffer);
                    cur.Segments.Add(arcSeg);
                    _arcBuffer.Clear();
                    // we've just added actual control points to the current USV — stop suppressing selection
                    _suppressSelectionOnNextClick = false;
                    // geometry changed: clear preserved samples for this USV
                    if (_selectedUsvIndex >= 0 && _selectedUsvIndex < _preservedPerUsv.Count)
                    {
                        _preservedPerUsv[_selectedUsvIndex] = new List<Point>();
                        UpdateGeneratedFromPreserved();
                    }
                }
            }
            else
            {
                // polyline input: append to current polyline segment
                var needNewPolyline = (cur.Segments.Count == 0) || (cur.Segments.Last().Type != SegmentType.Polyline);
                if (needNewPolyline)
                {
                    var newSeg = new Segment(SegmentType.Polyline);
                    // if previous segment exists and has an end point, seed it so the polyline starts at the previous end (continuous)
                    var prevEnd = cur.LastPoint();
                    if (prevEnd.HasValue)
                    {
                        newSeg.ControlPoints.Add(prevEnd.Value);
                    }
                    cur.Segments.Add(newSeg);
                }
                // add the clicked point as the next control point
                cur.Segments.Last().ControlPoints.Add(logical);
                // we've added a control point to current USV — stop suppressing selection
                _suppressSelectionOnNextClick = false;
                // geometry changed: clear preserved samples for this USV
                if (_selectedUsvIndex >= 0 && _selectedUsvIndex < _preservedPerUsv.Count)
                {
                    _preservedPerUsv[_selectedUsvIndex] = new List<Point>();
                    UpdateGeneratedFromPreserved();
                }
            }

            // remember last click logical position; actual marker drawn in Redraw so it survives clear
            _lastClickLogical = logical;
            Redraw();
        }

        private void BtnNewUsv_Click(object sender, RoutedEventArgs e)
        {
            var idx = _usvs.Count + 1;
            var u = new Usv { Name = GenerateUsvName(idx), Color = _palette[(idx - 1) % _palette.Length] };
            u.Segments.Add(new Segment(SegmentType.Polyline));
            _usvs.Add(u);
            // keep preserved list aligned
            _preservedPerUsv.Add(new List<Point>());
            _selectedUsvIndex = _usvs.Count - 1;
            _selectedSegmentIndex = -1;
            _arcBuffer.Clear();
            // 抑制接下来若干次点击的选择逻辑，直到新 USV 实际接收到控制点
            _suppressSelectionOnNextClick = true;
            UpdatePropertiesPanel();
            Redraw();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            _usvs.Clear(); _generatedPerUsv = null; _preservedPerUsv.Clear(); _selectedUsvIndex = -1; _arcBuffer.Clear(); EnsureCurrentUsv(); Redraw();
        }

        // Merge adjacent USVs where end of one equals start of next (within tolerance). Merge colors by keeping first USV's color.
        private void MergeConnectedUsvs()
        {
            if (_usvs.Count < 2) return;
            var merged = new List<Usv>();
            var mergedPreserved = new List<List<Point>>();
            const double tol = 1e-6;
            Usv current = _usvs[0];
            var currentPres = (_preservedPerUsv.Count > 0) ? new List<Point>(_preservedPerUsv[0]) : new List<Point>();
            for (int i = 1; i < _usvs.Count; i++)
            {
                var next = _usvs[i];
                var nextPres = (_preservedPerUsv.Count > i) ? new List<Point>(_preservedPerUsv[i]) : new List<Point>();
                var last = current.LastPoint();
                var first = next.FirstPoint();
                if (last.HasValue && first.HasValue && Math.Abs(last.Value.X - first.Value.X) <= tol && Math.Abs(last.Value.Y - first.Value.Y) <= tol)
                {
                    // merge next's segments into current (avoid duplicate shared point in polyline segments)
                    foreach (var seg in next.Segments)
                    {
                        if (seg.Type == SegmentType.Polyline && current.Segments.Count > 0 && current.Segments.Last().Type == SegmentType.Polyline)
                        {
                            // append control points, avoid duplicate
                            var toAdd = seg.ControlPoints.ToList();
                            if (toAdd.Count > 0 && current.Segments.Last().ControlPoints.Count > 0)
                            {
                                if (Math.Abs(current.Segments.Last().ControlPoints.Last().X - toAdd.First().X) < 1e-6 && Math.Abs(current.Segments.Last().ControlPoints.Last().Y - toAdd.First().Y) < 1e-6)
                                    toAdd.RemoveAt(0);
                            }
                            current.Segments.Last().ControlPoints.AddRange(toAdd);
                        }
                        else
                        {
                            current.Segments.Add(seg);
                        }
                    }
                    // merge preserved samples by appending
                    if (nextPres.Count > 0) { if (currentPres == null) currentPres = new List<Point>(); currentPres.AddRange(nextPres); }
                    // keep current color/name
                    continue; // skip adding next separately
                }
                merged.Add(current);
                mergedPreserved.Add(currentPres ?? new List<Point>());
                current = next;
                currentPres = nextPres;
            }
            merged.Add(current);
            mergedPreserved.Add(currentPres ?? new List<Point>());

            // reassign names and ensure colors
            for (int i = 0; i < merged.Count; i++)
            {
                merged[i].Name = GenerateUsvName(i + 1);
                if (merged[i].Color == null) merged[i].Color = _palette[i % _palette.Length];
            }

            _usvs.Clear();
            _usvs.AddRange(merged);
            _preservedPerUsv.Clear();
            _preservedPerUsv.AddRange(mergedPreserved);
            if (_selectedUsvIndex >= _usvs.Count) _selectedUsvIndex = _usvs.Count - 1;
            UpdateGeneratedFromPreserved();
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            // Build XML where each sample index across USVs becomes a step
            // Ensure we have some samples (generated or preserved or computable)
            var anySamples = false;
            var perUsvSamples = new List<List<Point>>();
            for (int i = 0; i < _usvs.Count; i++)
            {
                List<Point> listForUsv = new();
                if (_generatedPerUsv != null && i < _generatedPerUsv.Count && _generatedPerUsv[i] != null && _generatedPerUsv[i].Count > 0)
                    listForUsv = _generatedPerUsv[i];
                else if (i < _preservedPerUsv.Count && _preservedPerUsv[i] != null && _preservedPerUsv[i].Count > 0)
                    listForUsv = _preservedPerUsv[i];
                else
                {
                    // fallback: sample by spacing if available
                    if (double.TryParse(SpacingTextBox.Text, out var sp) && sp > 0)
                    {
                        var flat = _usvs[i].GetSamples(sp);
                        if (flat != null && flat.Count > 0) listForUsv = flat;
                    }
                }
                if (listForUsv != null && listForUsv.Count > 0) anySamples = true;
                perUsvSamples.Add(listForUsv ?? new List<Point>());
            }
            if (!anySamples)
            {
                MessageBox.Show("没有可用的采样点，请先生成采样点（Confirm Samples）。");
                return;
            }

            // determine max steps
            int maxSteps = perUsvSamples.Max(l => l.Count);
            if (maxSteps == 0) { MessageBox.Show("没有可用的采样点。"); return; }

            // build XML
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<cluster type=\"triangle\">");
            const double defaultYaw = 0.5; const double defaultSpeed = 0.5;
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            for (int step = 0; step < maxSteps; step++)
            {
                sb.AppendLine($"    <step number=\"{step+1}\"> ");
                sb.AppendLine("        <usvs>");
                for (int ui = 0; ui < _usvs.Count; ui++)
                {
                    var u = _usvs[ui];
                    // choose point: if usv has fewer samples, use last available point
                    var samples = perUsvSamples[ui];
                    Point pos;
                    if (samples != null && samples.Count > 0)
                    {
                        if (step < samples.Count) pos = samples[step]; else pos = samples.Last();
                    }
                    else
                    {
                        var fp = u.FirstPoint(); pos = fp ?? new Point(0, 0);
                    }
                    // convert to display units
                    var dx = pos.X * _displayUnitScale; var dy = pos.Y * _displayUnitScale;
                    sb.AppendLine("            <usv>");
                    sb.AppendLine($"                <usv_id>{System.Security.SecurityElement.Escape(u.Name)}</usv_id>");
                    sb.AppendLine("                <position>");
                    sb.AppendLine($"                    <x>{dx.ToString("F1", culture)}</x>");
                    sb.AppendLine($"                    <y>{dy.ToString("F1", culture)}</y>");
                    sb.AppendLine("                </position>");
                    sb.AppendLine("                <yaw>");
                    sb.AppendLine($"                    <value>{defaultYaw.ToString("F1", culture)}</value>");
                    sb.AppendLine("                </yaw>");
                    sb.AppendLine("                <velocity>");
                    sb.AppendLine($"                    <value>{defaultSpeed.ToString("F1", culture)}</value>");
                    sb.AppendLine("                </velocity>");
                    sb.AppendLine("            </usv>");
                }
                sb.AppendLine("        </usvs>");
                sb.AppendLine("    </step>");
            }
            sb.AppendLine("</cluster>");

            var xml = sb.ToString();
            // copy to clipboard when possible (do not prompt to save)
            try { System.Windows.Clipboard.SetText(xml); } catch { }

            // If bound to MainViewModel, populate Steps so left-side tables refresh
            if (DataContext is Path.ViewModels.MainViewModel vm)
            {
                // clear existing steps and create new ones from perUsvSamples where each sample index is a step
                vm.Steps.Clear();
                int max = maxSteps;
                for (int s = 0; s < max; s++)
                {
                    var step = new Path.Step($"Step {s + 1}");
                    for (int ui = 0; ui < _usvs.Count; ui++)
                    {
                        var u = _usvs[ui];
                        var samples = perUsvSamples[ui];
                        Point pos;
                        if (samples != null && samples.Count > 0)
                        {
                            pos = (s < samples.Count) ? samples[s] : samples.Last();
                        }
                        else
                        {
                            var fp = u.FirstPoint(); pos = fp ?? new Point(0, 0);
                        }
                        var dx = pos.X * _displayUnitScale; var dy = pos.Y * _displayUnitScale;
                        var usvModel = new Path.USV { Id = u.Name, X = dx, Y = dy, Yaw = defaultYaw, Speed = defaultSpeed };
                        step.USVs.Add(usvModel);
                    }
                    vm.Steps.Add(step);
                }
                vm.SelectedStep = vm.Steps.Count > 0 ? vm.Steps[0] : null;
                vm.StatusMessage = $"已生成 {vm.Steps.Count} 步 (来自 PathEditor)";

                // also refresh local visuals
                UpdateGeneratedFromPreserved();
                Redraw();
                MessageBox.Show($"已生成 XML 并保存，已在左侧步骤表中创建 {vm.Steps.Count} 个步骤。");
            }
            else
            {
                Redraw();
                MessageBox.Show("已生成 XML 并复制到剪贴板。请保存或导入以查看步骤。");
            }
        }

        private static List<Point> SampleSegment(Point a, Point b, double spacing, bool includeLast)
        {
            var list = new List<Point>();
            var dx = b.X - a.X; var dy = b.Y - a.Y; var len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 1e-6) { if (includeLast) list.Add(b); return list; }
            var steps = (int)Math.Floor(len / spacing);
            if (steps == 0) { if (includeLast) list.Add(b); else list.Add(a); return list; }
            var ux = dx / len; var uy = dy / len;
            for (int i = 0; i < steps; i++) { var d = i * spacing; var p = new Point(a.X + ux * d, a.Y + uy * d); list.Add(p); }
            if (includeLast) list.Add(b); return list;
        }

        private static List<Point>? SampleArcByThreePoints(Point p1, Point p2, Point p3, double spacing)
        {
            double ax = p1.X, ay = p1.Y; double bx = p2.X, by = p2.Y; double cx = p3.X, cy = p3.Y;
            double d = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by)); if (Math.Abs(d) < 1e-9) return null;
            double ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
            double uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;
            var center = new Point(ux, uy); var r = Distance(center, p1); if (r < 1e-6) return null;
            double ang1 = Math.Atan2(p1.Y - center.Y, p1.X - center.X); double ang2 = Math.Atan2(p2.Y - center.Y, p2.X - center.X); double ang3 = Math.Atan2(p3.Y - center.Y, p3.X - center.X);
            var candidates = new[] { (start: ang1, end: ang3, dir: 1), (start: ang1, end: ang3, dir: -1) };
            double startAng = 0, sweep = 0; bool found = false;
            foreach (var cand in candidates)
            {
                double s = cand.start; double e = cand.end; double sw;
                if (cand.dir > 0)
                {
                    sw = NormalizeAnglePositive(e - s); var mid = NormalizeAnglePositive(ang2 - s);
                    if (mid > 0 && mid < sw) { startAng = s; sweep = sw; found = true; break; }
                }
                else
                {
                    sw = NormalizeAnglePositive(s - e); var mid = NormalizeAnglePositive(s - ang2);
                    if (mid > 0 && mid < sw) { startAng = e; sweep = sw; found = true; break; }
                }
            }
            if (!found) { startAng = ang1; sweep = SmallestSignedAngle(ang1, ang3); sweep = Math.Abs(sweep); }
            var arcLength = r * sweep; var count = Math.Max(1, (int)Math.Floor(arcLength / spacing)); var list = new List<Point>();
            for (int i = 0; i <= count; i++) { var t = (double)i / count; var ang = startAng + t * sweep; var px = center.X + r * Math.Cos(ang); var py = center.Y + r * Math.Sin(ang); list.Add(new Point(px, py)); }
            return list;
        }

        private static double SmallestSignedAngle(double a, double b) { var diff = b - a; while (diff <= -Math.PI) diff += 2 * Math.PI; while (diff > Math.PI) diff -= 2 * Math.PI; return diff; }
        private static double NormalizeAnglePositive(double a) { while (a < 0) a += 2 * Math.PI; while (a >= 2 * Math.PI) a -= 2 * Math.PI; return a; }
        private static double Distance(Point a, Point b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

        private Ellipse? _lastClickMarker;
        private int _hoveredUsvIndex = -1;
        private Point? _lastClickLogical;
        private bool _isDraggingPoint = false;
        private (int usvIndex, int segIndex, int pointIndex)? _draggingPointRef;
        private Point? _dragOriginalPoint;
        // when dragging a control point, track all linked control points (same coordinates) so they move together
        private List<(int ui, int si, int pi, Point orig)> _linkedDraggingPoints = new();
        private FrameworkElement? _draggingElement;
        private string? _lastDebugText;

        // simple undo stack for last action
        private Stack<Action> _undoStack = new();

        private void Redraw()
        {
            CanvasElement.Children.Clear(); if (OverlayCanvasElement!=null) OverlayCanvasElement.Children.Clear(); /* no RenderTransform usage; draw manually */ UpdateCanvasTransform(); DrawGrid();
            // draw control points for each USV so start points are clear
            for (int ui = 0; ui < _usvs.Count; ui++)
            {
                var u = _usvs[ui];
                // draw each segment; arcs drawn via sampling for smooth curve
                for (int si = 0; si < u.Segments.Count; si++)
                {
                    var seg = u.Segments[si];
                    if (seg.Type == SegmentType.Polyline)
                    {
                        for (int i = 0; i + 1 < seg.ControlPoints.Count; i++)
                        {
                            var p1 = seg.ControlPoints[i]; var p2 = seg.ControlPoints[i + 1];
                            var sp1 = LogicalToScreen(p1); var sp2 = LogicalToScreen(p2);
                            var isThisSegmentSelected = (_selectedUsvIndex == ui && _selectedSegmentIndex == si);
                            var line = new Line { X1 = sp1.X, Y1 = sp1.Y, X2 = sp2.X, Y2 = sp2.Y, Stroke = isThisSegmentSelected ? Brushes.Red : u.Color, StrokeThickness = isThisSegmentSelected ? 4 : ((_selectedUsvIndex==ui)?3:2) };
                            CanvasElement.Children.Add(line);
                        }
                    }
                    else if (seg.Type == SegmentType.Arc && seg.ControlPoints.Count >= 3)
                    {
                        var samples = SampleArcByThreePoints(seg.ControlPoints[0], seg.ControlPoints[1], seg.ControlPoints[2], 2.0) ?? new List<Point>();
                        for (int i = 0; i + 1 < samples.Count; i++)
                        {
                            var p1 = samples[i]; var p2 = samples[i + 1];
                            var sp1 = LogicalToScreen(p1); var sp2 = LogicalToScreen(p2);
                            var isThisSegmentSelected = (_selectedUsvIndex == ui && _selectedSegmentIndex == si);
                            var line = new Line { X1 = sp1.X, Y1 = sp1.Y, X2 = sp2.X, Y2 = sp2.Y, Stroke = isThisSegmentSelected ? Brushes.Red : u.Color, StrokeThickness = isThisSegmentSelected ? 4 : ((_selectedUsvIndex==ui)?3:2) };
                            CanvasElement.Children.Add(line);
                        }
                    }
                }
                // draw USV label at first available point
                var fp = u.FirstPoint();
                if (fp.HasValue)
                {
                    var lbl = new TextBlock { Text = u.Name, Foreground = Brushes.Black, FontWeight = FontWeights.Bold, Background = new SolidColorBrush(Colors.White), Padding = new Thickness(2), FontSize = 12 };
                    var s = LogicalToScreen(fp.Value);
                    Canvas.SetLeft(lbl, s.X + 6); Canvas.SetTop(lbl, s.Y - 10); CanvasElement.Children.Add(lbl);
                }
                // draw control point markers onto overlay canvas so they remain interactive and on top
                if (OverlayCanvasElement != null)
                {
                    for (int si = 0; si < u.Segments.Count; si++)
                    {
                        var seg = u.Segments[si];
                        for (int pi = 0; pi < seg.ControlPoints.Count; pi++)
                        {
                            var cp = seg.ControlPoints[pi];
                            var isStart = (ui == 0 && pi == 0) || (pi == 0);
                            var rect = new Rectangle { Width = isStart ? 8 : 6, Height = isStart ? 8 : 6, Fill = isStart ? Brushes.Red : Brushes.White, Stroke = Brushes.Black, StrokeThickness = 1, Cursor = Cursors.Hand, IsHitTestVisible = true };
                            var sc = LogicalToScreen(cp);
                            Canvas.SetLeft(rect, sc.X - (rect.Width / 2)); Canvas.SetTop(rect, sc.Y - (rect.Height / 2));
                            rect.Tag = (ui, si, pi);
                            // also attach handlers (fallback) and ensure hit testing
                            rect.MouseDown += ControlPoint_MouseDown;
                            rect.MouseMove += ControlPoint_MouseMove;
                            rect.MouseUp += ControlPoint_MouseUp;
                            OverlayCanvasElement.Children.Add(rect);
                        }
                    }
                }
            }

            // draw preserved or generated samples
            if (_generatedPerUsv != null)
            {
                for (int ui = 0; ui < _generatedPerUsv.Count; ui++)
                {
                    var samples = _generatedPerUsv[ui];
                    var color = _usvs[Math.Min(ui, _usvs.Count - 1)].Color;
                    // compute threshold from TbMinSpacing if present
                    double thr = 2.0; try { var tbm = FindName("TbMinSpacing") as TextBox; if (tbm!=null && double.TryParse(tbm.Text, out var vt)) thr = vt; } catch { }
                    for (int si = 0; si < samples.Count; si++)
                    {
                        var sp = samples[si];
                        var spScreen = LogicalToScreen(sp);
                        // decide fill color: red if gap to next sample (display units) less than threshold
                        Brush fill = color;
                        if (si + 1 < samples.Count)
                        {
                            var gap = Distance(samples[si], samples[si + 1]) * _displayUnitScale;
                            if (gap < thr) fill = Brushes.Red;
                        }
                        var ell = new Ellipse { Width = 6, Height = 6, Fill = fill, Stroke = Brushes.Black, StrokeThickness = 1 };
                        Canvas.SetLeft(ell, spScreen.X - 3); Canvas.SetTop(ell, spScreen.Y - 3); CanvasElement.Children.Add(ell);
                        var tb = new TextBlock { Text = (si + 1).ToString(), FontSize = 10, Foreground = Brushes.Black, Background = new SolidColorBrush(Colors.White) };
                        // bold label when highlighted
                        try { if (si + 1 < samples.Count && Distance(samples[si], samples[si + 1]) * _displayUnitScale < thr) tb.FontWeight = FontWeights.Bold; } catch { }
                        Canvas.SetLeft(tb, spScreen.X + 4); Canvas.SetTop(tb, spScreen.Y - 6); CanvasElement.Children.Add(tb);
                    }
                }
            }
            else
            {
                // fall back to preserved lists
                for (int ui = 0; ui < _preservedPerUsv.Count; ui++)
                {
                    var samples = _preservedPerUsv[ui];
                    if (samples == null || samples.Count == 0) continue;
                    var color = _usvs[Math.Min(ui, _usvs.Count - 1)].Color;
                    // compute threshold
                    double thr2 = 2.0; try { var tbm2 = FindName("TbMinSpacing") as TextBox; if (tbm2!=null && double.TryParse(tbm2.Text, out var vt2)) thr2 = vt2; } catch { }
                    for (int si = 0; si < samples.Count; si++)
                    {
                        var sp = samples[si]; var spScreen = LogicalToScreen(sp);
                        Brush fill = color;
                        if (si + 1 < samples.Count)
                        {
                            var gap = Distance(samples[si], samples[si + 1]) * _displayUnitScale;
                            if (gap < thr2) fill = Brushes.Red;
                        }
                        var ell = new Ellipse { Width = 6, Height = 6, Fill = fill, Stroke = Brushes.Black, StrokeThickness = 1 };
                        Canvas.SetLeft(ell, spScreen.X - 3); Canvas.SetTop(ell, spScreen.Y - 3); CanvasElement.Children.Add(ell);
                        var tb = new TextBlock { Text = (si + 1).ToString(), FontSize = 10, Foreground = Brushes.Black, Background = new SolidColorBrush(Colors.White) };
                        try { if (si + 1 < samples.Count && Distance(samples[si], samples[si + 1]) * _displayUnitScale < thr2) tb.FontWeight = FontWeights.Bold; } catch { }
                        Canvas.SetLeft(tb, spScreen.X + 4); Canvas.SetTop(tb, spScreen.Y - 6); CanvasElement.Children.Add(tb);
                    }
                }
            }

            // draw last click marker after everything else so it's visible
            if (_lastClickLogical.HasValue)
            {
                if (_lastClickMarker != null) CanvasElement.Children.Remove(_lastClickMarker);
                var lp = _lastClickLogical.Value;
                var marker = new Ellipse { Width = 6, Height = 6, Fill = Brushes.Blue, Stroke = Brushes.Black, StrokeThickness = 0 };
                var sm = LogicalToScreen(lp);
                Canvas.SetLeft(marker, sm.X - 3); Canvas.SetTop(marker, sm.Y - 3);
                CanvasElement.Children.Add(marker);
                _lastClickMarker = marker;
            }
            UpdatePropertiesPanel();
        }

        private Point LogicalToScreen(Point p)
        {
            return new Point(p.X * _scale + _translateX, p.Y * _scale + _translateY);
        }

        private void UpdateCanvasTransform()
        {
            // No RenderTransform on canvases: we perform manual logical->screen mapping when drawing.
        }

        private void DrawGrid()
        {
            var canvasWidth = CanvasElement.Width; if (double.IsNaN(canvasWidth) || canvasWidth <= 0) canvasWidth = CanvasElement.ActualWidth;
            var canvasHeight = CanvasElement.Height; if (double.IsNaN(canvasHeight) || canvasHeight <= 0) canvasHeight = CanvasElement.ActualHeight;
            if (canvasWidth <= 0 || canvasHeight <= 0) return;
            // compute logical extents visible in the canvas
            var logicalMinX = (0 - _translateX) / _scale;
            var logicalMaxX = (canvasWidth - _translateX) / _scale;
            var logicalMinY = (0 - _translateY) / _scale;
            var logicalMaxY = (canvasHeight - _translateY) / _scale;
            double lw = Math.Max(0, logicalMaxX - logicalMinX);
            double lh = Math.Max(0, logicalMaxY - logicalMinY);
            // choose minor/major ticks. Start with 1 and 10 (meters) but adapt if too many ticks would be drawn
            int minor = 1; int major = 10;
            // estimate tick counts and cap to avoid UI freeze
            const int maxTicks = 1200;
            var estCountX = (int)Math.Ceiling((logicalMaxX - logicalMinX) / (double)minor);
            var estCountY = (int)Math.Ceiling((logicalMaxY - logicalMinY) / (double)minor);
            var maxCount = Math.Max(estCountX, estCountY);
            if (maxCount > maxTicks)
            {
                // increase minor so we draw fewer ticks
                minor = (int)Math.Ceiling((double)maxCount / maxTicks);
                // ensure major remains a multiple of minor
                major = Math.Max(10, ((major + minor - 1) / minor) * minor);
            }
            // compute start/end in logical coordinates rounded to minor grid
            var startX = (int)Math.Floor(logicalMinX / minor) * minor;
            var endX = (int)Math.Ceiling(logicalMaxX / minor) * minor;
            double lastLabelX = double.NegativeInfinity;
            for (int x = startX; x <= endX; x += minor)
            {
                var px = LogicalToScreen(new Point(x, 0)).X;
                var isMajor = (x % major == 0);
                var tick = new Line { X1 = px, Y1 = 0, X2 = px, Y2 = isMajor ? 12 : 6, Stroke = Brushes.Gray, StrokeThickness = isMajor ? 1 : 0.5 };
                CanvasElement.Children.Add(tick);
                if (isMajor)
                {
                    // avoid overlapping labels: require at least 40 pixels between labels
                    if (double.IsInfinity(lastLabelX) || Math.Abs(px - lastLabelX) >= 40)
                    {
                        var lbl = new TextBlock { Text = (x * _displayUnitScale).ToString(), FontSize = 10 };
                        Canvas.SetLeft(lbl, px + 2); Canvas.SetTop(lbl, 12);
                        CanvasElement.Children.Add(lbl);
                        lastLabelX = px;
                    }
                }
            }
            var startY = (int)Math.Floor(logicalMinY / minor) * minor;
            var endY = (int)Math.Ceiling((logicalMaxY) / minor) * minor;
            double lastLabelY = double.NegativeInfinity;
            for (int y = startY; y <= endY; y += minor)
            {
                var py = LogicalToScreen(new Point(0, y)).Y;
                var isMajor = (y % major == 0);
                var tick = new Line { X1 = 0, Y1 = py, X2 = isMajor ? 12 : 6, Y2 = py, Stroke = Brushes.Gray, StrokeThickness = isMajor ? 1 : 0.5 };
                CanvasElement.Children.Add(tick);
                if (isMajor)
                {
                    // avoid overlapping labels vertically
                    if (double.IsInfinity(lastLabelY) || Math.Abs(py - lastLabelY) >= 14)
                    {
                        var lbl = new TextBlock { Text = (y * _displayUnitScale).ToString(), FontSize = 10 };
                        Canvas.SetLeft(lbl, 2); Canvas.SetTop(lbl, py + 2);
                        CanvasElement.Children.Add(lbl);
                        lastLabelY = py;
                    }
                }
            }
            var org = new TextBlock { Text = "0,0", FontSize = 10 }; Canvas.SetLeft(org, 2); Canvas.SetTop(org, 14 / _scale); CanvasElement.Children.Add(org);
            var xlabel = new TextBlock { Text = "X", FontSize = 12, FontWeight = FontWeights.Bold }; Canvas.SetLeft(xlabel, Math.Max(0, lw - 20)); Canvas.SetTop(xlabel, 20 / _scale); CanvasElement.Children.Add(xlabel);
            var ylabel = new TextBlock { Text = "Y", FontSize = 12, FontWeight = FontWeights.Bold }; Canvas.SetLeft(ylabel, 8 / _scale); Canvas.SetTop(ylabel, Math.Max(0, lh - 20)); CanvasElement.Children.Add(ylabel);
        }

        private Point ScreenToLogical(Point screen)
        {
            // If 'screen' already lies within canvas bounds, treat it as canvas-space point.
            var canvasW = CanvasElement.ActualWidth; var canvasH = CanvasElement.ActualHeight;
            Point canvasPoint = screen;
            if (!(screen.X >= 0 && screen.X <= canvasW && screen.Y >= 0 && screen.Y <= canvasH))
            {
                // assume input is window-relative; convert to screen then to canvas coords
                try
                {
                    var wnd = Window.GetWindow(this);
                    if (wnd != null)
                    {
                        var screenPoint = wnd.PointToScreen(screen);
                        canvasPoint = CanvasElement.PointFromScreen(screenPoint);
                    }
                    else
                    {
                        canvasPoint = screen;
                    }
                }
                catch { canvasPoint = screen; }
            }
            var lx = (canvasPoint.X - _translateX) / _scale;
            var ly = (canvasPoint.Y - _translateY) / _scale;
            return new Point(lx, ly);
         }

        private void DrawCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // get canvas-space position
            var canvasPosReliable = e.GetPosition(CanvasElement);
            var logical = ScreenToLogical(canvasPosReliable);
            // If dragging a point, move it
            if (_isDraggingPoint && _draggingPointRef.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                var wndPt2 = e.GetPosition(this);
                var pos2 = CanvasElement.PointFromScreen(this.PointToScreen(wndPt2));
                var logical2 = new Point((pos2.X - _translateX) / _scale, (pos2.Y - _translateY) / _scale);
                // move all linked control points to follow mouse delta relative to original anchor
                if (_linkedDraggingPoints.Count > 0 && _dragOriginalPoint.HasValue)
                {
                    var orig = _dragOriginalPoint.Value;
                    var dx = logical2.X - orig.X; var dy = logical2.Y - orig.Y;
                    foreach (var idx in _linkedDraggingPoints)
                    {
                        if (idx.ui >= 0 && idx.ui < _usvs.Count && idx.si >= 0 && idx.si < _usvs[idx.ui].Segments.Count && idx.pi >= 0 && idx.pi < _usvs[idx.ui].Segments[idx.si].ControlPoints.Count)
                        {
                            var newp = new Point(idx.orig.X + dx, idx.orig.Y + dy);
                            _usvs[idx.ui].Segments[idx.si].ControlPoints[idx.pi] = newp;
                        }
                    }
                    // update properties and visuals so the line moves with the drag
                    UpdatePropertiesPanel();
                    // keep the visual feedback live
                    Redraw();
                }
                e.Handled = true;
            }

            // panning: when middle button pressed, scroll the ScrollViewer instead of changing translate
            if (e.MiddleButton == MouseButtonState.Pressed && !_isPanning)
            {
                var sv = CanvasScrollViewerElement;
                if (sv != null)
                {
                    _isPanning = true;
                    _panStartPoint = e.GetPosition(sv);
                    _panStartHOffset = sv.HorizontalOffset;
                    _panStartVOffset = sv.VerticalOffset;
                    CanvasElement.CaptureMouse();
                }
            }
            if (_isPanning && e.MiddleButton == MouseButtonState.Pressed)
            {
                var sv = CanvasScrollViewerElement;
                if (sv != null)
                {
                    var cur = e.GetPosition(sv);
                    var dx = cur.X - _panStartPoint.X; var dy = cur.Y - _panStartPoint.Y;
                    sv.ScrollToHorizontalOffset(Math.Max(0, _panStartHOffset - dx));
                    sv.ScrollToVerticalOffset(Math.Max(0, _panStartVOffset - dy));
                }
            }
            if (_isPanning && e.MiddleButton == MouseButtonState.Released)
            {
                _isPanning = false; CanvasElement.ReleaseMouseCapture();
            }
        }

        private void DrawCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // end dragging if any
            EndDraggingPoint();
        }

        private bool TrySelectUsvAt(Point p)
        {
            const double thresh = 6.0;
            // Do not clear existing selection here; only update if we find a hit.
            int foundUsv = -1;
            int foundSeg = -1;
            for (int ui = 0; ui < _usvs.Count; ui++)
            {
                var u = _usvs[ui];
                for (int si = 0; si < u.Segments.Count; si++)
                {
                    var seg = u.Segments[si];
                    if (seg.Type == SegmentType.Polyline)
                    {
                        for (int i = 0; i + 1 < seg.ControlPoints.Count; i++)
                        {
                            var a = seg.ControlPoints[i];
                            var b = seg.ControlPoints[i + 1];
                            if (DistancePointToSegment(p, a, b) <= thresh)
                            {
                                foundUsv = ui;
                                foundSeg = si;
                                break;
                            }
                        }
                    }
                    else if (seg.Type == SegmentType.Arc && seg.ControlPoints.Count >= 3)
                    {
                        if (IsPointNearArc(p, seg.ControlPoints[0], seg.ControlPoints[1], seg.ControlPoints[2], thresh))
                        {
                            foundUsv = ui;
                            foundSeg = si;
                            break;
                        }
                    }
                    if (foundUsv != -1) break;
                }
                if (foundUsv != -1) break;
            }

            if (foundUsv != -1)
            {
                _selectedUsvIndex = foundUsv;
                _selectedSegmentIndex = foundSeg;
                return true;
            }
            return false;
        }

        private static bool IsPointNearArc(Point p, Point p1, Point p2, Point p3, double tol)
        {
            // compute circumcenter
            double ax = p1.X, ay = p1.Y; double bx = p2.X, by = p2.Y; double cx = p3.X, cy = p3.Y;
            double d = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by)); if (Math.Abs(d) < 1e-9) return false;
            double ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
            double uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;
            var center = new Point(ux, uy); var r = Distance(center, p1); if (r < 1e-6) return false;
            var ang1 = Math.Atan2(p1.Y - center.Y, p1.X - center.X); var ang2 = Math.Atan2(p2.Y - center.Y, p2.X - center.X); var ang3 = Math.Atan2(p3.Y - center.Y, p3.X - center.X);
            // determine arc start and sweep same as sampling
            var candidates = new[] { (start: ang1, end: ang3, dir: 1), (start: ang1, end: ang3, dir: -1) };
            double startAng = 0, sweep = 0; bool found = false;
            foreach (var cand in candidates)
            {
                double s = cand.start; double e = cand.end; double sw;
                if (cand.dir > 0) { sw = NormalizeAnglePositive(e - s); var mid = NormalizeAnglePositive(ang2 - s); if (mid > 0 && mid < sw) { startAng = s; sweep = sw; found = true; break; } }
                else { sw = NormalizeAnglePositive(s - e); var mid = NormalizeAnglePositive(s - ang2); if (mid > 0 && mid < sw) { startAng = e; sweep = sw; found = true; break; } }
            }
            if (!found) { startAng = ang1; sweep = SmallestSignedAngle(ang1, ang3); sweep = Math.Abs(sweep); }
            // check angle
            var angP = Math.Atan2(p1.Y - center.Y, p1.X - center.X);
            var rel = NormalizeAnglePositive(angP - startAng);
            if (rel < 0) rel += 2 * Math.PI;
            if (rel <= sweep + 1e-6)
            {
                var dr = Math.Abs(Distance(center, p) - r);
                return dr <= tol;
            }
            return false;
        }

        private static double DistancePointToSegment(Point p, Point a, Point b)
        {
            var vx = b.X - a.X; var vy = b.Y - a.Y; var wx = p.X - a.X; var wy = p.Y - a.Y; var c1 = vx * wx + vy * wy; if (c1 <= 0) return Distance(p, a); var c2 = vx * vx + vy * vy; if (c2 <= c1) return Distance(p, b); var t = c1 / c2; var proj = new Point(a.X + t * vx, a.Y + t * vy); return Distance(p, proj);
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (_undoStack.Count > 0)
            {
                var action = _undoStack.Pop();
                action();
            }
        }

        private void UpdatePropertiesPanel()
        {
            if (_selectedUsvIndex >= 0 && _selectedUsvIndex < _usvs.Count)
            {
                var u = _usvs[_selectedUsvIndex]; UsvNameTextBox.Text = u.Name; ColorCombo.SelectedIndex = Array.IndexOf(_palette, u.Color);
                // compute total length of this USV (logical units)
                var totalLength = GetUsvTotalLength(u);
                var totalLengthDisplay = totalLength * _displayUnitScale;
                int sampleCount = 0;
                // prefer generatedPerUsv if present
                List<Point> samplesForDisplay = new();
                if (_generatedPerUsv != null && _selectedUsvIndex < _generatedPerUsv.Count)
                {
                    samplesForDisplay = new List<Point>(_generatedPerUsv[_selectedUsvIndex]);
                    sampleCount = samplesForDisplay.Count;
                }
                else
                {
                    // otherwise prefer preserved per-segment confirmed samples if available
                    var segSum = u.Segments.Sum(s => s.ConfirmedSamples?.Count ?? 0);
                    if (segSum > 0)
                    {
                        foreach (var s in u.Segments) if (s.ConfirmedSamples != null && s.ConfirmedSamples.Count > 0) samplesForDisplay.AddRange(s.ConfirmedSamples);
                        sampleCount = samplesForDisplay.Count;
                    }
                    else if (double.TryParse(SpacingTextBox.Text, out var sp))
                    {
                        samplesForDisplay = u.GetSamples(sp);
                        sampleCount = samplesForDisplay.Count;
                    }
                }

                // show sample count and first/last point display coords
                var first = u.FirstPoint(); var last = u.LastPoint();
                var firstDisplay = first.HasValue ? ($"{first.Value.X * _displayUnitScale:F2},{first.Value.Y * _displayUnitScale:F2}") : "-";
                var lastDisplay = last.HasValue ? ($"{last.Value.X * _displayUnitScale:F2},{last.Value.Y * _displayUnitScale:F2}") : "-";

                // configurable threshold: look for a TextBox named TbMinSpacing; fallback to 2.0
                double threshold = 2.0;
                try
                {
                    var tb = FindName("TbMinSpacing") as TextBox;
                    if (tb != null && double.TryParse(tb.Text, out var t)) threshold = t;
                }
                catch { }

                // compute distances between consecutive sampling points (in display units)
                var distancesDisplay = new List<double>();
                for (int i = 0; i + 1 < samplesForDisplay.Count; i++)
                {
                    var d = Distance(samplesForDisplay[i], samplesForDisplay[i + 1]) * _displayUnitScale;
                    distancesDisplay.Add(d);
                }

                // prepare formatted property text using Inlines so we can style distances
                PropTextBlock.Inlines.Clear();
                PropTextBlock.Inlines.Add(new Run($"名称: {u.Name}\n"));
                PropTextBlock.Inlines.Add(new Run($"颜色: {GetColorName(u.Color)}\n"));
                PropTextBlock.Inlines.Add(new Run($"采样点: {sampleCount}\n"));
                // segment info or total control points
                if (_selectedSegmentIndex >= 0 && _selectedSegmentIndex < u.Segments.Count)
                {
                    var seg = u.Segments[_selectedSegmentIndex];
                    var pts = seg.ControlPoints.Count;
                    PropTextBlock.Inlines.Add(new Run($"所选段: {_selectedSegmentIndex + 1} (点数: {pts})\n"));
                }
                else
                {
                    var totalPts = u.Segments.Sum(s => s.ControlPoints.Count);
                    PropTextBlock.Inlines.Add(new Run($"总控制点: {totalPts}\n"));
                }
                PropTextBlock.Inlines.Add(new Run($"首点(显示): {firstDisplay}\n"));
                PropTextBlock.Inlines.Add(new Run($"末点(显示): {lastDisplay}\n"));
                PropTextBlock.Inlines.Add(new Run($"总长度(显示): {totalLengthDisplay:F2}\n"));

                // distances details: list each segment distance and style if below threshold
                if (distancesDisplay.Count > 0)
                {
                    PropTextBlock.Inlines.Add(new Run($"样点间距(显示，单位与阈值相同)：\n"));
                    for (int i = 0; i < distancesDisplay.Count; i++)
                    {
                        var d = distancesDisplay[i];
                        var run = new Run($"  {i + 1} → {i + 2}: {d:F2}\n");
                        if (d < threshold)
                        {
                            run.Foreground = Brushes.Red;
                            run.FontWeight = FontWeights.Bold;
                        }
                        PropTextBlock.Inlines.Add(run);
                    }
                    // show summary
                    var min = distancesDisplay.Min(); var max = distancesDisplay.Max(); var avg = distancesDisplay.Average();
                    PropTextBlock.Inlines.Add(new Run($"样点间距 summary (min/avg/max): {min:F2}/{avg:F2}/{max:F2}\n"));
                }
                else
                {
                    PropTextBlock.Inlines.Add(new Run("样点间距: -\n"));
                }

                if (!string.IsNullOrEmpty(_lastDebugText)) { PropTextBlock.Inlines.Add(new Run(_lastDebugText)); }
            }
            else { UsvNameTextBox.Text = string.Empty; ColorCombo.SelectedIndex = -1; PropTextBlock.Inlines.Clear(); PropTextBlock.Inlines.Add(new Run("未选中任何线")); }
        }

        private string GetColorName(Brush b) { if (b == Brushes.SteelBlue) return "SteelBlue"; if (b == Brushes.Orange) return "Orange"; if (b == Brushes.Green) return "Green"; if (b == Brushes.Purple) return "Purple"; if (b == Brushes.Brown) return "Brown"; if (b == Brushes.Teal) return "Teal"; if (b == Brushes.Magenta) return "Magenta"; if (b == Brushes.Gold) return "Gold"; return b.ToString(); }

        private void UpdateColorComboSelection() { ColorCombo.SelectedIndex = 0; }
        private void TbUsvName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedUsvIndex >= 0 && _selectedUsvIndex < _usvs.Count)
            {
                _usvs[_selectedUsvIndex].Name = UsvNameTextBox.Text;
                Redraw();
            }
        }
        private void CbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedUsvIndex >= 0 && _selectedUsvIndex < _usvs.Count && ColorCombo.SelectedItem is ComboBoxItem it)
            {
                var name = it.Content as string ?? "SteelBlue";
                var brush = Brushes.SteelBlue;
                switch (name)
                {
                    case "SteelBlue": brush = Brushes.SteelBlue; break;
                    case "Orange": brush = Brushes.Orange; break;
                    case "Green": brush = Brushes.Green; break;
                    case "Purple": brush = Brushes.Purple; break;
                    case "Brown": brush = Brushes.Brown; break;
                    case "Teal": brush = Brushes.Teal; break;
                    case "Magenta": brush = Brushes.Magenta; break;
                    case "Gold": brush = Brushes.Gold; break;
                }
                _usvs[_selectedUsvIndex].Color = brush;
                Redraw();
            }
        }

        private static Point ProjectPointToSegment(Point p, Point a, Point b)
        {
            var vx = b.X - a.X;
            var vy = b.Y - a.Y;
            var wx = p.X - a.X;
            var wy = p.Y - a.Y;
            var c1 = vx * wx + vy * wy;
            if (c1 <= 0) return a;
            var c2 = vx * vx + vy * vy;
            if (c2 <= c1) return b;
            var t = c1 / c2;
            return new Point(a.X + t * vx, a.Y + t * vy);
        }

        private void DrawCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // delete nearest control point if close (right-click)
            var pos = e.GetPosition(CanvasElement);
            var logical = ScreenToLogical(pos);
            double best = double.MaxValue; int buri = -1, bsi = -1, bpi = -1;
            for (int ui = 0; ui < _usvs.Count; ui++)
            {
                var u = _usvs[ui];
                for (int si = 0; si < u.Segments.Count; si++)
                {
                    var seg = u.Segments[si];
                    for (int pi = 0; pi < seg.ControlPoints.Count; pi++)
                    {
                        var d = Distance(logical, seg.ControlPoints[pi]);
                        if (d < best) { best = d; buri = ui; bsi = si; bpi = pi; }
                    }
                }
            }
            if (buri != -1 && best < 10.0)
            {
                var removed = _usvs[buri].Segments[bsi].ControlPoints[bpi];
                _undoStack.Push(() => { _usvs[buri].Segments[bsi].ControlPoints.Insert(bpi, removed); Redraw(); });
                _usvs[buri].Segments[bsi].ControlPoints.RemoveAt(bpi);
                // clear confirmed samples for this usv because geometry changed
                foreach (var s in _usvs[buri].Segments) s.ConfirmedSamples = new List<Point>();
                UpdateGeneratedFromPreserved();
                Redraw();
            }
        }

        private void StartDraggingPoint(int ui, int si, int pi)
        {
            _isDraggingPoint = true; _draggingPointRef = (ui, si, pi);
            _dragOriginalPoint = _usvs[ui].Segments[si].ControlPoints[pi];
            _linkedDraggingPoints.Clear();
            // record originals for all control points coincident with this one (within tolerance)
            var orig = _usvs[ui].Segments[si].ControlPoints[pi];
            const double tol = 1e-6;
            for (int uii = 0; uii < _usvs.Count; uii++)
            {
                var u = _usvs[uii];
                for (int sii = 0; sii < u.Segments.Count; sii++)
                {
                    var seg = u.Segments[sii];
                    for (int pii = 0; pii < seg.ControlPoints.Count; pii++)
                    {
                        var p = seg.ControlPoints[pii];
                        if (Math.Abs(p.X - orig.X) <= tol && Math.Abs(p.Y - orig.Y) <= tol)
                        {
                            _linkedDraggingPoints.Add((uii, sii, pii, p));
                        }
                    }
                }
            }
            // set single original for compatibility
            _dragOriginalPoint = orig;
        }

        private void EndDraggingPoint()
        {
            if (_isDraggingPoint && _draggingPointRef.HasValue && _linkedDraggingPoints.Count > 0)
            {
                // capture current indices and originals to restore
                var saved = _linkedDraggingPoints.Select(x => x).ToList();
                _undoStack.Push(() =>
                {
                    foreach (var it in saved)
                    {
                        if (it.ui >= 0 && it.ui < _usvs.Count && it.si >= 0 && it.si < _usvs[it.ui].Segments.Count && it.pi >= 0 && it.pi < _usvs[it.ui].Segments[it.si].ControlPoints.Count)
                        {
                            _usvs[it.ui].Segments[it.si].ControlPoints[it.pi] = it.orig;
                        }
                    }
                    Redraw();
                });

                // geometry changed: clear confirmed samples for affected USVs
                var affectedUsv = new HashSet<int>(_linkedDraggingPoints.Select(x => x.ui));
                foreach (var ui in affectedUsv)
                {
                    if (ui >= 0 && ui < _usvs.Count)
                    {
                        foreach (var s in _usvs[ui].Segments) s.ConfirmedSamples = new List<Point>();
                    }
                }
                UpdateGeneratedFromPreserved();
            }
            _isDraggingPoint = false; _draggingPointRef = null; _dragOriginalPoint = null; _linkedDraggingPoints.Clear();
        }

        private bool TryFindNearestControlPoint(Point logical, out int usvIndex, out int segIndex, out int pointIndex, out double distance)
        {
            usvIndex = -1; segIndex = -1; pointIndex = -1; distance = double.MaxValue;
            for (int ui = 0; ui < _usvs.Count; ui++)
            {
                var u = _usvs[ui];
                for (int si = 0; si < u.Segments.Count; si++)
                {
                    var seg = u.Segments[si];
                    for (int pi = 0; pi < seg.ControlPoints.Count; pi++)
                    {
                        var d = Distance(logical, seg.ControlPoints[pi]);
                        if (d < distance)
                        {
                            distance = d; usvIndex = ui; segIndex = si; pointIndex = pi;
                        }
                    }
                }
            }
            return usvIndex != -1;
        }

        // handlers used when control point rectangles are clicked/dragged
        private void ControlPoint_MouseDown(object? sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is ValueTuple<int, int, int> t)
            {
                var (ui, si, pi) = t;
                // use StartDraggingPoint to collect linked points
                StartDraggingPoint(ui, si, pi);
                _draggingElement = fe;
                try { fe.CaptureMouse(); } catch { }
                e.Handled = true;
            }
        }

        private void ControlPoint_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDraggingPoint && _draggingPointRef.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                var wndPt2 = e.GetPosition(this);
                var pos2 = CanvasElement.PointFromScreen(this.PointToScreen(wndPt2));
                var logical2 = new Point((pos2.X - _translateX) / _scale, (pos2.Y - _translateY) / _scale);
                if (_linkedDraggingPoints.Count > 0 && _dragOriginalPoint.HasValue)
                {
                    var orig = _dragOriginalPoint.Value;
                    var dx = logical2.X - orig.X; var dy = logical2.Y - orig.Y;
                    for (int i = 0; i < _linkedDraggingPoints.Count; i++)
                    {
                        var it = _linkedDraggingPoints[i];
                        if (it.ui >= 0 && it.ui < _usvs.Count && it.si >= 0 && it.si < _usvs[it.ui].Segments.Count && it.pi >= 0 && it.pi < _usvs[it.ui].Segments[it.si].ControlPoints.Count)
                        {
                            var newp = new Point(it.orig.X + dx, it.orig.Y + dy);
                            _usvs[it.ui].Segments[it.si].ControlPoints[it.pi] = newp;
                        }
                    }
                    UpdatePropertiesPanel();
                    // redraw to reflect moved lines dynamically
                    Redraw();
                }
                e.Handled = true;
            }
        }

        private void ControlPoint_MouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (_isDraggingPoint && _draggingElement is FrameworkElement fe)
            {
                fe.ReleaseMouseCapture();
            }
            EndDraggingPoint();
            Redraw();
            e.Handled = true;
        }

        private void DrawCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning && e.ChangedButton == MouseButton.Middle)
            {
                _isPanning = false;
                Mouse.Capture(null);
            }
        }

        private void DrawCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // double middle-click resets view
            if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 2)
            {
                var sv = CanvasScrollViewerElement;
                if (sv != null)
                {
                    sv.ScrollToHorizontalOffset(0);
                    sv.ScrollToVerticalOffset(0);
                }
                _scale = 1.0; _translateX = 0.0; _translateY = 0.0; Redraw();
            }
        }

        private void DrawCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCanvasTransform(); Redraw();
        }

        private bool _suppressSelectionOnNextClick = false;

        // New: Confirm samples button and sampling-by-count helpers
        private void BtnConfirmSamples_Click(object sender, RoutedEventArgs e)
        {
            // ensure there is at least one USV
            EnsureCurrentUsv();
            if (_selectedUsvIndex < 0 || _selectedUsvIndex >= _usvs.Count)
            {
                MessageBox.Show("请先选中一条线（点击画布上的线段）以应用采样数量。");
                return;
            }

            var raw = (SpacingTextBox?.Text ?? string.Empty).Trim();
            if (!int.TryParse(raw, out var count) || count <= 0)
            {
                MessageBox.Show("请输入有效的采样点数量（正整数）。");
                return;
            }

            // compute per-segment samples and persist them
            var perSeg = SampleUsvByCountPerSegment(_usvs[_selectedUsvIndex], count);
            // assign ConfirmedSamples on each segment for selected usv
            for (int si = 0; si < perSeg.Count; si++)
            {
                _usvs[_selectedUsvIndex].Segments[si].ConfirmedSamples = perSeg[si] ?? new List<Point>();
            }
            // save flattened preserved samples so they persist across selection changes
            if (_selectedUsvIndex >= 0)
            {
                var flat = new List<Point>();
                foreach (var s in _usvs[_selectedUsvIndex].Segments) if (s.ConfirmedSamples != null && s.ConfirmedSamples.Count > 0) flat.AddRange(s.ConfirmedSamples);
                // ensure preserved list is aligned
                while (_preservedPerUsv.Count < _usvs.Count) _preservedPerUsv.Add(new List<Point>());
                if (_selectedUsvIndex < _preservedPerUsv.Count) _preservedPerUsv[_selectedUsvIndex] = flat;
            }
            // refresh global generated view from all preserved samples
            UpdateGeneratedFromPreserved();
            Redraw();
        }

        private List<List<Point>> SampleUsvByCountPerSegment(Usv u, int count)
        {
            var resultPerSeg = new List<List<Point>>();
            foreach (var s in u.Segments) resultPerSeg.Add(new List<Point>());
            if (count <= 0) return resultPerSeg;
            var segs = u.Segments;
            var segLengths = new List<double>();
            double total = 0.0;
            foreach (var s in segs)
            {
                var L = GetSegmentLength(s);
                segLengths.Add(L);
                total += L;
            }
            if (total <= 1e-9)
            {
                var fp = u.FirstPoint(); if (fp.HasValue) { if (segs.Count>0) resultPerSeg[0].Add(fp.Value); } return resultPerSeg;
            }

            for (int i = 0; i < count; i++)
            {
                double t = (count == 1) ? 0.0 : (double)i * total / (count - 1);
                double acc = 0.0; int si = 0; double local = t;
                for (; si < segs.Count; si++)
                {
                    if (t <= acc + segLengths[si] || si == segs.Count - 1) { local = t - acc; break; }
                    acc += segLengths[si];
                }
                var seg = segs[si];
                var p = GetPointAlongSegment(seg, localDistance: local);
                resultPerSeg[si].Add(p);
            }
            return resultPerSeg;
        }

        private List<Point> SampleUsvByCount(Usv u, int count)
        {
            // kept for compatibility: flatten per-seg result
            var per = SampleUsvByCountPerSegment(u, count);
            var flat = new List<Point>(); foreach (var s in per) if (s != null) flat.AddRange(s); return flat;
        }

        private double GetSegmentLength(Segment seg)
        {
            if (seg.Type == SegmentType.Polyline)
            {
                var pts = seg.ControlPoints;
                if (pts.Count < 2) return 0.0;
                double s = 0.0; for (int i = 0; i + 1 < pts.Count; i++) s += Distance(pts[i], pts[i + 1]); return s;
            }
            else
            {
                if (seg.ControlPoints.Count < 3) return 0.0;
                // reuse arc length logic from SampleArcByThreePoints
                var p1 = seg.ControlPoints[0]; var p2 = seg.ControlPoints[1]; var p3 = seg.ControlPoints[2];
                double ax = p1.X, ay = p1.Y; double bx = p2.X, by = p2.Y; double cx = p3.X, cy = p3.Y;
                double d = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by)); if (Math.Abs(d) < 1e-9) return 0.0;
                double ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
                double uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;
                var center = new Point(ux, uy); var r = Distance(center, p1); if (r < 1e-6) return 0.0;
                double ang1 = Math.Atan2(p1.Y - center.Y, p1.X - center.X); double ang2 = Math.Atan2(p2.Y - center.Y, p2.X - center.X); double ang3 = Math.Atan2(p3.Y - center.Y, p3.X - center.X);
                var candidates = new[] { (start: ang1, end: ang3, dir: 1), (start: ang1, end: ang3, dir: -1) };
                double startAng = 0, sweep = 0; bool found = false;
                foreach (var cand in candidates)
                {
                    double s = cand.start; double e = cand.end; double sw;
                    if (cand.dir > 0)
                    {
                        sw = NormalizeAnglePositive(e - s); var mid = NormalizeAnglePositive(ang2 - s);
                        if (mid > 0 && mid < sw) { startAng = s; sweep = sw; found = true; break; }
                    }
                    else
                    {
                        sw = NormalizeAnglePositive(s - e); var mid = NormalizeAnglePositive(s - ang2);
                        if (mid > 0 && mid < sw) { startAng = e; sweep = sw; found = true; break; }
                    }
                }
                if (!found) { startAng = ang1; sweep = SmallestSignedAngle(ang1, ang3); sweep = Math.Abs(sweep); }
                return r * sweep;
            }
        }

        private Point GetPointAlongSegment(Segment seg, double localDistance)
        {
            if (seg.Type == SegmentType.Polyline)
            {
                var pts = seg.ControlPoints;
                if (pts.Count == 0) return new Point(0, 0);
                if (pts.Count == 1) return pts[0];
                double acc = 0.0;
                for (int i = 0; i + 1 < pts.Count; i++)
                {
                    var a = pts[i]; var b = pts[i + 1]; var d = Distance(a, b);
                    if (localDistance <= acc + d || i + 1 == pts.Count - 1)
                    {
                        var rem = localDistance - acc; var t = (d < 1e-9) ? 0.0 : rem / d; return new Point(a.X + t * (b.X - a.X), a.Y + t * (b.Y - a.Y));
                    }
                    acc += d;
                }
                return pts.Last();
            }
            else
            {
                // arc: compute center, start angle, then angle = start + (localDistance / r)
                var cp = seg.ControlPoints;
                if (cp.Count < 3) return new Point(0, 0);
                var p1 = cp[0]; var p2 = cp[1]; var p3 = cp[2];
                double ax = p1.X, ay = p1.Y; double bx = p2.X, by = p2.Y; double cx = p3.X, cy = p3.Y;
                double d = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by)); if (Math.Abs(d) < 1e-9) return p1;
                double ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
                double uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;
                var center = new Point(ux, uy); var r = Distance(center, p1); if (r < 1e-6) return p1;
                double ang1 = Math.Atan2(p1.Y - center.Y, p1.X - center.X); double ang2 = Math.Atan2(p2.Y - center.Y, p2.X - center.X); double ang3 = Math.Atan2(p3.Y - center.Y, p3.X - center.X);
                var candidates = new[] { (start: ang1, end: ang3, dir: 1), (start: ang1, end: ang3, dir: -1) };
                double startAng = 0, sweep = 0; bool found = false;
                foreach (var cand in candidates)
                {
                    double s = cand.start; double e = cand.end; double sw;
                    if (cand.dir > 0)
                    {
                        sw = NormalizeAnglePositive(e - s); var mid = NormalizeAnglePositive(ang2 - s);
                        if (mid > 0 && mid < sw) { startAng = s; sweep = sw; found = true; break; }
                    }
                    else
                    {
                        sw = NormalizeAnglePositive(s - e); var mid = NormalizeAnglePositive(s - ang2);
                        if (mid > 0 && mid < sw) { startAng = e; sweep = sw; found = true; break; }
                    }
                }
                if (!found) { startAng = ang1; sweep = SmallestSignedAngle(ang1, ang3); sweep = Math.Abs(sweep); }
                // clamp localDistance to [0, r*sweep]
                var segLen = r * sweep; var frac = (segLen < 1e-9) ? 0.0 : Math.Max(0.0, Math.Min(1.0, localDistance / segLen));
                var ang = startAng + frac * sweep;
                return new Point(center.X + r * Math.Cos(ang), center.Y + r * Math.Sin(ang));
            }
        }

        // New handlers for clearing samples and exporting CSV
        private void BtnClearSamples_Click(object sender, RoutedEventArgs e)
        {
            // clear generated and preserved samples so visuals are removed
            _generatedPerUsv = null;
            // clear confirmed samples on all segments
            foreach (var u in _usvs) foreach (var s in u.Segments) s.ConfirmedSamples = new List<Point>();
            // clear preserved per-usv flattened lists
            for (int i = 0; i < _preservedPerUsv.Count; i++) _preservedPerUsv[i] = new List<Point>();
            UpdateGeneratedFromPreserved();
            MessageBox.Show("已清除已生成的采样点。");
            Redraw();
        }

        private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
        {
            // build CSV text with header per USV that has samples
            var sb = new StringBuilder();
            for (int i = 0; i < _usvs.Count; i++)
            {
                List<Point> listForUsv = new();
                // prefer generatedPerUsv
                if (_generatedPerUsv != null && i < _generatedPerUsv.Count && _generatedPerUsv[i] != null && _generatedPerUsv[i].Count > 0)
                    listForUsv = _generatedPerUsv[i];
                else
                {
                    // fallback to preserved per-segment confirmed samples
                    foreach (var s in _usvs[i].Segments) if (s.ConfirmedSamples != null && s.ConfirmedSamples.Count > 0) listForUsv.AddRange(s.ConfirmedSamples);
                }
                if (listForUsv == null || listForUsv.Count == 0) continue;
                sb.AppendLine($"# {_usvs[Math.Min(i, _usvs.Count-1)].Name}");
                sb.AppendLine("index,x,y");
                for (int j = 0; j < listForUsv.Count; j++)
                {
                    var p = listForUsv[j];
                    // export in display units
                    var ex = p.X * _displayUnitScale; var ey = p.Y * _displayUnitScale;
                    sb.AppendLine($"{j+1},{ex:F6},{ey:F6}");
                }
                sb.AppendLine();
            }
            var csv = sb.ToString();
            if (string.IsNullOrWhiteSpace(csv))
            {
                MessageBox.Show("没有可导出的采样点。");
                return;
            }
            try
            {
                System.Windows.Clipboard.SetText(csv);
                MessageBox.Show("已将采样点导出为CSV并复制到剪贴板。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("复制到剪贴板失败: " + ex.Message);
            }
        }

        private void PathEditor_KeyDown(object? sender, KeyEventArgs e)
        {
            // Ctrl+Z -> undo
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Z)
            {
                if (_undoStack.Count > 0)
                {
                    var action = _undoStack.Pop();
                    try { action(); } catch { }
                }
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Delete)
            {
                BtnDeleteSegment_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private void BtnDeleteSegment_Click(object? sender, RoutedEventArgs e)
        {
            if (_selectedUsvIndex < 0 || _selectedUsvIndex >= _usvs.Count)
            {
                MessageBox.Show("未选中任何USV。");
                return;
            }
            var u = _usvs[_selectedUsvIndex];
            if (_selectedSegmentIndex < 0 || _selectedSegmentIndex >= u.Segments.Count)
            {
                MessageBox.Show("未选中任何段。");
                return;
            }
            // confirm
            if (MessageBox.Show($"确认删除当前所选段（USV: {u.Name}，段号: {_selectedSegmentIndex+1}）?", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var removed = u.Segments[_selectedSegmentIndex];
            var usvIndex = _selectedUsvIndex;
            var segIndex = _selectedSegmentIndex;

            // perform removal
            u.Segments.RemoveAt(segIndex);

            // push undo that reinserts the same segment object at same index
            _undoStack.Push(() =>
            {
                if (usvIndex >= 0 && usvIndex < _usvs.Count)
                {
                    var uu = _usvs[usvIndex];
                    if (segIndex >= 0 && segIndex <= uu.Segments.Count) uu.Segments.Insert(segIndex, removed);
                }
                UpdateGeneratedFromPreserved();
                Redraw();
            });

            // clear preserved/confirmed samples for this usv because geometry changed
            foreach (var s in _usvs[usvIndex].Segments) s.ConfirmedSamples = new List<Point>();
            if (usvIndex >= 0 && usvIndex < _preservedPerUsv.Count) _preservedPerUsv[usvIndex] = new List<Point>();
            UpdateGeneratedFromPreserved();

            // adjust selected segment index
            if (_usvs[usvIndex].Segments.Count == 0)
            {
                // ensure at least one empty polyline exists
                _usvs[usvIndex].Segments.Add(new Segment(SegmentType.Polyline));
                _selectedSegmentIndex = -1;
            }
            else
            {
                _selectedSegmentIndex = Math.Min(segIndex, _usvs[usvIndex].Segments.Count - 1);
            }
            Redraw();
        }

        // Compute total length (logical units) of the USV by summing segment lengths (polylines and arcs)
        private double GetUsvTotalLength(Usv u)
        {
            double sum = 0.0;
            foreach (var seg in u.Segments)
            {
                sum += GetSegmentLength(seg);
            }
            return sum;
        }

        // Load Steps from a collection (produced by MainViewModel.ImportXml/ExportXml)
        public void LoadFromSteps(IEnumerable<Path.Step> steps)
        {
            if (steps == null) return;
            // clear current geometry
            _usvs.Clear(); _preservedPerUsv.Clear(); _generatedPerUsv = null; _selectedUsvIndex = -1; _selectedSegmentIndex = -1;

            // collect points per USV id in order of first appearance
            var idOrder = new List<string>();
            var perIdPoints = new Dictionary<string, List<Point>>();

            foreach (var step in steps)
            {
                if (step.USVs == null) continue;
                foreach (var u in step.USVs)
                {
                    var id = u.Id ?? string.Empty;
                    if (!perIdPoints.ContainsKey(id)) { perIdPoints[id] = new List<Point>(); idOrder.Add(id); }
                    // convert display units back to logical coordinates
                    var lx = u.X / _displayUnitScale; var ly = u.Y / _displayUnitScale;
                    perIdPoints[id].Add(new Point(lx, ly));
                }
            }

            for (int i = 0; i < idOrder.Count; i++)
            {
                var id = idOrder[i];
                var pts = perIdPoints[id];
                var uu = new Usv { Name = string.IsNullOrEmpty(id) ? GenerateUsvName(i + 1) : id, Color = _palette[i % _palette.Length] };
                var seg = new Segment(SegmentType.Polyline);
                foreach (var p in pts) seg.ControlPoints.Add(p);
                if (seg.ControlPoints.Count == 0) seg.ControlPoints.Add(new Point(0, 0));
                uu.Segments.Add(seg);
                _usvs.Add(uu);
                _preservedPerUsv.Add(new List<Point>(pts));
            }

            UpdateGeneratedFromPreserved();
            if (_usvs.Count > 0) _selectedUsvIndex = 0;
            Redraw();
        }
    }
}