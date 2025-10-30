using Path.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Path.Services
{
    /// <summary>
    /// 路径数据服务实现
    /// </summary>
    public class XmlPathDataService : IPathDataService
    {
    public List<StepModel> ImportFromXml(string filePath)
        {
 var steps = new List<StepModel>();

            try
    {
                var doc = XDocument.Load(filePath);
    var cluster = doc.Root;

      if (cluster == null || cluster.Name != "cluster")
 {
      throw new InvalidDataException("无效的 XML 格式: 找不到 cluster 根节点");
     }

       foreach (var stepElem in cluster.Elements("step"))
           {
              var numberAttr = stepElem.Attribute("number");
             var stepNumber = int.TryParse(numberAttr?.Value, out var num) ? num : 0;
          var displayName = $"Step {stepNumber}";

        var step = new StepModel
        {
              Number = stepNumber,
        DisplayName = displayName
  };

  var usvsElem = stepElem.Element("usvs");
    if (usvsElem != null)
     {
    foreach (var usvElem in usvsElem.Elements("usv"))
    {
          var usv = ParseUsvElement(usvElem);
          step.Usvs.Add(usv);
    }
     }

     steps.Add(step);
          }
            }
    catch (Exception ex)
            {
            throw new InvalidOperationException($"导入 XML 失败: {ex.Message}", ex);
            }

        return steps;
 }

     public void ExportToXml(string filePath, IEnumerable<StepModel> steps)
    {
            try
    {
      var doc = new XDocument(new XDeclaration("1.0", "UTF-8", null));
 var cluster = new XElement("cluster", new XAttribute("type", "home"));

     foreach (var step in steps)
      {
            var stepElem = new XElement("step", new XAttribute("number", step.Number.ToString()));
    var usvsElem = new XElement("usvs");

    foreach (var usv in step.Usvs)
    {
         var usvElem = CreateUsvElement(usv);
          usvsElem.Add(usvElem);
        }

           stepElem.Add(usvsElem);
         cluster.Add(stepElem);
        }

      doc.Add(cluster);

     using var stream = File.Create(filePath);
           doc.Save(stream);
    }
            catch (Exception ex)
      {
         throw new InvalidOperationException($"导出 XML 失败: {ex.Message}", ex);
    }
        }

        public bool ValidateData(IEnumerable<StepModel> steps, out string? errorMessage)
     {
     errorMessage = null;

          if (steps == null || !steps.Any())
            {
    errorMessage = "没有数据需要验证";
   return false;
}

            foreach (var step in steps)
   {
       if (step.Usvs.Count == 0)
     {
         errorMessage = $"{step.DisplayName} 没有 USV 数据";
        return false;
         }

   foreach (var usv in step.Usvs)
      {
              if (string.IsNullOrWhiteSpace(usv.Id))
 {
          errorMessage = $"{step.DisplayName} 中存在无效的 USV ID";
  return false;
        }
 }
            }

          return true;
    }

        private UsvModel ParseUsvElement(XElement usvElem)
        {
            var id = usvElem.Element("usv_id")?.Value ?? string.Empty;
            var xText = usvElem.Element("position")?.Element("x")?.Value ?? "0";
            var yText = usvElem.Element("position")?.Element("y")?.Value ?? "0";
            var zText = usvElem.Element("position")?.Element("z")?.Value ?? "0";
          var yawText = usvElem.Element("yaw")?.Element("value")?.Value ?? "0";
    var velText = usvElem.Element("velocity")?.Element("value")?.Value ?? "0";

        double.TryParse(xText, NumberStyles.Any, CultureInfo.InvariantCulture, out var x);
            double.TryParse(yText, NumberStyles.Any, CultureInfo.InvariantCulture, out var y);
 double.TryParse(zText, NumberStyles.Any, CultureInfo.InvariantCulture, out var z);
   double.TryParse(yawText, NumberStyles.Any, CultureInfo.InvariantCulture, out var yaw);
      double.TryParse(velText, NumberStyles.Any, CultureInfo.InvariantCulture, out var vel);

        return new UsvModel
            {
    Id = id,
                X = x,
     Y = y,
            Z = z,
           Yaw = yaw,
           Speed = vel
            };
        }

        private XElement CreateUsvElement(UsvModel usv)
      {
         return new XElement("usv",
     new XElement("usv_id", usv.Id ?? string.Empty),
             new XElement("position",
  new XElement("x", usv.X.ToString("F3", CultureInfo.InvariantCulture)),
          new XElement("y", usv.Y.ToString("F3", CultureInfo.InvariantCulture)),
     new XElement("z", usv.Z.ToString("F3", CultureInfo.InvariantCulture))
           ),
     new XElement("yaw",
             new XElement("value", usv.Yaw.ToString("F3", CultureInfo.InvariantCulture))
            ),
         new XElement("velocity",
   new XElement("value", usv.Speed.ToString("F3", CultureInfo.InvariantCulture))
  )
        );
        }
    }
}
