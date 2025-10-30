using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;

namespace Path.Services.Step214
{
  /// <summary>
    /// STEP 214 文件解析器
    /// </summary>
    public class Step214Parser
    {
 private readonly Dictionary<int, StepEntity> _entities = new();

        /// <summary>
        /// 解析 STEP 文件
        /// </summary>
    public Dictionary<int, StepEntity> Parse(string filePath)
  {
      _entities.Clear();

      var lines = File.ReadAllLines(filePath);
     var inDataSection = false;
            var currentLine = "";

       foreach (var line in lines)
            {
 var trimmed = line.Trim();

         // 检查数据段开始
                if (trimmed == "DATA;")
        {
  inDataSection = true;
             continue;
      }

    // 检查数据段结束
     if (trimmed == "ENDSEC;")
{
     inDataSection = false;
     continue;
           }

        if (!inDataSection)
      continue;

   // 累积多行实体定义
       currentLine += trimmed;

       // 如果行以分号结束，表示实体定义完成
       if (trimmed.EndsWith(';'))
     {
         ParseEntity(currentLine);
          currentLine = "";
       }
            }

return _entities;
        }

   /// <summary>
        /// 解析单个实体
        /// </summary>
        private void ParseEntity(string line)
        {
          try
        {
             // 格式: #123 = ENTITY_TYPE(param1, param2, ...);
     var match = Regex.Match(line, @"#(\d+)\s*=\s*([A-Z_]+)\((.*)\);?");
  if (!match.Success)
        return;

      var id = int.Parse(match.Groups[1].Value);
     var type = match.Groups[2].Value;
      var paramsStr = match.Groups[3].Value;

           var entity = new StepEntity
     {
              Id = id,
          Type = type,
     Parameters = ParseParameters(paramsStr)
          };

            _entities[id] = entity;
          }
    catch
{
    // 忽略解析错误的行
            }
        }

    /// <summary>
        /// 解析参数列表
        /// </summary>
   private List<object> ParseParameters(string paramsStr)
  {
            var parameters = new List<object>();
     if (string.IsNullOrWhiteSpace(paramsStr))
      return parameters;

   var depth = 0;
  var current = "";

        for (int i = 0; i < paramsStr.Length; i++)
     {
       var c = paramsStr[i];

       if (c == '(')
     {
            depth++;
           current += c;
        }
      else if (c == ')')
    {
  depth--;
current += c;
            }
            else if (c == ',' && depth == 0)
          {
     // 参数分隔符
    parameters.Add(ParseParameter(current.Trim()));
  current = "";
           }
     else
{
            current += c;
      }
   }

            // 添加最后一个参数
        if (!string.IsNullOrWhiteSpace(current))
   {
                parameters.Add(ParseParameter(current.Trim()));
       }

            return parameters;
    }

        /// <summary>
        /// 解析单个参数
        /// </summary>
        private object ParseParameter(string param)
        {
            if (string.IsNullOrWhiteSpace(param))
    return "";

            // 未定义值
            if (param == "$" || param == "*")
    return null!;

            // 引用 (#123)
       if (param.StartsWith('#'))
            {
         if (int.TryParse(param.Substring(1), out var refId))
         return new StepReference(refId);
 }

         // 字符串 ('text')
    if (param.StartsWith('\'') && param.EndsWith('\''))
{
          return param.Substring(1, param.Length - 2);
            }

    // 数字
 if (double.TryParse(param, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
    {
          return number;
         }

     // 嵌套实体 (TYPE(...))
      var nestedMatch = Regex.Match(param, @"([A-Z_]+)\((.*)\)");
if (nestedMatch.Success)
       {
           var type = nestedMatch.Groups[1].Value;
       var paramsStr = nestedMatch.Groups[2].Value;
    return new StepEntity
    {
       Type = type,
           Parameters = ParseParameters(paramsStr)
     };
       }

   // 其他情况返回字符串
          return param;
     }

     /// <summary>
        /// 获取实体
        /// </summary>
        public StepEntity? GetEntity(int id)
        {
            return _entities.TryGetValue(id, out var entity) ? entity : null;
        }

        /// <summary>
   /// 获取所有指定类型的实体
        /// </summary>
   public List<StepEntity> GetEntitiesByType(string type)
        {
       return _entities.Values
         .Where(e => e.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
