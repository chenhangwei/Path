using Microsoft.Win32;
using Path.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Path.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Steps = new ObservableCollection<Path.Step>();

            // sample data
            var step1 = new Path.Step("Step 1");
            step1.USVs.Add(new Path.USV { Id = "usv_01", X = 0.0, Y = 0.0, Yaw = 0.0, Speed = 0.5 });
            step1.USVs.Add(new Path.USV { Id = "usv_02", X = 0.0, Y = 0.0, Yaw = 0.0, Speed = 0.5 });
            step1.USVs.Add(new Path.USV { Id = "usv_03", X = 0.0, Y = 0.0, Yaw = 0.0, Speed = 0.5 });

            var step2 = new Path.Step("Step 2");
            step2.USVs.Add(new Path.USV { Id = "usv_04", X = 5, Y = 8, Yaw = 12, Speed = 2 });

            Steps.Add(step1);
            Steps.Add(step2);

            SelectedStep = step1;

            OpenCommand = new RelayCommand(_ => ImportXml());
            SaveCommand = new RelayCommand(_ => ExportXml());
            ExportXmlCommand = new RelayCommand(_ => ExportXml());
            ImportXmlCommand = new RelayCommand(_ => ImportXml());

            AddStepCommand = new RelayCommand(_ => AddStep());
            RemoveStepCommand = new RelayCommand(_ => RemoveSelectedStep(), _ => SelectedStep != null);
            MoveStepUpCommand = new RelayCommand(_ => MoveStepUp(), _ => CanMoveStepUp());
            MoveStepDownCommand = new RelayCommand(_ => MoveStepDown(), _ => CanMoveStepDown());

            AddUsvCommand = new RelayCommand(_ => AddUsv(), _ => SelectedStep != null);
            RemoveUsvCommand = new RelayCommand(param => RemoveUsv(param as Path.USV), param => param is Path.USV && SelectedStep != null);
        }

        private string _statusMessage = "准备就绪";
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        public ObservableCollection<Path.Step> Steps { get; }

        private Path.Step? _selectedStep;
        public Path.Step? SelectedStep
        {
            get => _selectedStep;
            set
            {
                if (_selectedStep != value)
                {
                    _selectedStep = value;
                    OnPropertyChanged(nameof(SelectedStep));
                    // update StatusMessage when selection changes
                    StatusMessage = _selectedStep == null ? "未选中" : $"已选: {_selectedStep.DisplayName}";
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExportXmlCommand { get; }
        public ICommand ImportXmlCommand { get; }

        public ICommand AddStepCommand { get; }
        public ICommand RemoveStepCommand { get; }
        public ICommand MoveStepUpCommand { get; }
        public ICommand MoveStepDownCommand { get; }

        public ICommand AddUsvCommand { get; }
        public ICommand RemoveUsvCommand { get; }

        private void ExportXml()
        {
            try
            {
                var doc = new XDocument(new XDeclaration("1.0", "UTF-8", null));
                var cluster = new XElement("cluster", new XAttribute("type", "home"));

                for (int i = 0; i < Steps.Count; i++)
                {
                    var step = Steps[i];
                    var stepElem = new XElement("step", new XAttribute("number", (i + 1).ToString()));

                    var usvsElem = new XElement("usvs");
                    foreach (var usv in step.USVs)
                    {
                        var usvElem = new XElement("usv",
                            new XElement("usv_id", usv.Id ?? string.Empty),
                            new XElement("position",
                                new XElement("x", usv.X.ToString("F3", CultureInfo.InvariantCulture)),
                                new XElement("y", usv.Y.ToString("F3", CultureInfo.InvariantCulture))
                            ),
                            new XElement("yaw",
                                new XElement("value", usv.Yaw.ToString("F3", CultureInfo.InvariantCulture))
                            ),
                            new XElement("velocity",
                                new XElement("value", usv.Speed.ToString("F3", CultureInfo.InvariantCulture))
                            )
                        );

                        usvsElem.Add(usvElem);
                    }

                    stepElem.Add(usvsElem);
                    cluster.Add(stepElem);
                }

                doc.Add(cluster);

                var dlg = new SaveFileDialog
                {
                    Filter = "XML 文件 (*.xml)|*.xml|所有文件 (*.*)|*.*",
                    DefaultExt = "xml",
                    FileName = "cluster.xml"
                };

                if (dlg.ShowDialog() == true)
                {
                    using var stream = File.Create(dlg.FileName);
                    doc.Save(stream);
                    StatusMessage = $"已导出: {dlg.FileName}";
                    MessageBox.Show($"已导出 XML 到: {dlg.FileName}");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}");
            }
        }

        private void ImportXml()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "XML 文件 (*.xml)|*.xml|所有文件 (*.*)|*.*",
                DefaultExt = "xml"
            };

            if (dlg.ShowDialog() != true) return;

            try
            {
                var doc = XDocument.Load(dlg.FileName);
                var cluster = doc.Root;
                if (cluster == null || cluster.Name != "cluster")
                {
                    MessageBox.Show("无效的 XML 格式: 找不到 cluster 根节点");
                    return;
                }

                Steps.Clear();

                foreach (var stepElem in cluster.Elements("step"))
                {
                    var numberAttr = stepElem.Attribute("number");
                    var displayName = numberAttr != null ? $"Step {numberAttr.Value}" : "Step";
                    var step = new Path.Step(displayName);

                    var usvsElem = stepElem.Element("usvs");
                    if (usvsElem != null)
                    {
                        foreach (var usvElem in usvsElem.Elements("usv"))
                        {
                            var id = usvElem.Element("usv_id")?.Value ?? string.Empty;
                            var xText = usvElem.Element("position")?.Element("x")?.Value ?? "0";
                            var yText = usvElem.Element("position")?.Element("y")?.Value ?? "0";
                            var yawText = usvElem.Element("yaw")?.Element("value")?.Value ?? "0";
                            var velText = usvElem.Element("velocity")?.Element("value")?.Value ?? "0";

                            if (!double.TryParse(xText, NumberStyles.Any, CultureInfo.InvariantCulture, out var x))
                                double.TryParse(xText, out x);
                            if (!double.TryParse(yText, NumberStyles.Any, CultureInfo.InvariantCulture, out var y))
                                double.TryParse(yText, out y);
                            if (!double.TryParse(yawText, NumberStyles.Any, CultureInfo.InvariantCulture, out var yaw))
                                double.TryParse(yawText, out yaw);
                            if (!double.TryParse(velText, NumberStyles.Any, CultureInfo.InvariantCulture, out var vel))
                                double.TryParse(velText, out vel);

                            step.USVs.Add(new Path.USV { Id = id, X = x, Y = y, Yaw = yaw, Speed = vel });
                        }
                    }

                    Steps.Add(step);
                }

                SelectedStep = Steps.Count > 0 ? Steps[0] : null;
                StatusMessage = $"已导入: {System.IO.Path.GetFileName(dlg.FileName)}";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"导入失败: {ex.Message}");
            }
        }

        private void AddStep()
        {
            var s = new Path.Step($"Step {Steps.Count + 1}");
            Steps.Add(s);
            SelectedStep = s;
        }

        private void RemoveSelectedStep()
        {
            if (SelectedStep == null) return;
            var idx = Steps.IndexOf(SelectedStep);
            Steps.Remove(SelectedStep);
            if (Steps.Count == 0) SelectedStep = null;
            else SelectedStep = Steps[Math.Max(0, idx - 1)];
        }

        private bool CanMoveStepUp() => SelectedStep != null && Steps.IndexOf(SelectedStep) > 0;
        private bool CanMoveStepDown() => SelectedStep != null && Steps.IndexOf(SelectedStep) < Steps.Count - 1;

        private void MoveStepUp()
        {
            if (!CanMoveStepUp()) return;
            var idx = Steps.IndexOf(SelectedStep!);
            var s = SelectedStep!;
            Steps.RemoveAt(idx);
            Steps.Insert(idx - 1, s);
            SelectedStep = s;
        }

        private void MoveStepDown()
        {
            if (!CanMoveStepDown()) return;
            var idx = Steps.IndexOf(SelectedStep!);
            var s = SelectedStep!;
            Steps.RemoveAt(idx);
            Steps.Insert(idx + 1, s);
            SelectedStep = s;
        }

        private void AddUsv()
        {
            if (SelectedStep == null) return;
            var id = $"usv_{SelectedStep.USVs.Count + 1:00}";
            var u = new Path.USV { Id = id, X = 0, Y = 0, Yaw = 0, Speed = 0 };
            SelectedStep.USVs.Add(u);
        }

        private void RemoveUsv(Path.USV? usv)
        {
            if (SelectedStep == null || usv == null) return;
            SelectedStep.USVs.Remove(usv);
        }

        public void AddUsvsFromPoints(IEnumerable<System.Windows.Point> points, double speed)
        {
            if (SelectedStep == null) return;
            foreach (var pt in points)
            {
                // 假设 USV 构造函数为 USV(double x, double y, double speed)
                SelectedStep.USVs.Add(new Path.USV { X = pt.X, Y = pt.Y, Speed = speed });
            }
            OnPropertyChanged(nameof(SelectedStep));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
