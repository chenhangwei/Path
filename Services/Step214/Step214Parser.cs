using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;

namespace Path.Services.Step214
{
  /// <summary>
    /// STEP 214 �ļ�������
    /// </summary>
    public class Step214Parser
    {
 private readonly Dictionary<int, StepEntity> _entities = new();

        /// <summary>
        /// ���� STEP �ļ�
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

         // ������ݶο�ʼ
                if (trimmed == "DATA;")
        {
  inDataSection = true;
             continue;
      }

    // ������ݶν���
     if (trimmed == "ENDSEC;")
{
     inDataSection = false;
     continue;
           }

        if (!inDataSection)
      continue;

   // �ۻ�����ʵ�嶨��
       currentLine += trimmed;

       // ������ԷֺŽ�������ʾʵ�嶨�����
       if (trimmed.EndsWith(';'))
     {
         ParseEntity(currentLine);
          currentLine = "";
       }
            }

return _entities;
        }

   /// <summary>
        /// ��������ʵ��
        /// </summary>
        private void ParseEntity(string line)
        {
          try
        {
             // ��ʽ: #123 = ENTITY_TYPE(param1, param2, ...);
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
    // ���Խ����������
            }
        }

    /// <summary>
        /// ���������б�
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
     // �����ָ���
    parameters.Add(ParseParameter(current.Trim()));
  current = "";
           }
     else
{
            current += c;
      }
   }

            // ������һ������
        if (!string.IsNullOrWhiteSpace(current))
   {
                parameters.Add(ParseParameter(current.Trim()));
       }

            return parameters;
    }

        /// <summary>
        /// ������������
        /// </summary>
        private object ParseParameter(string param)
        {
            if (string.IsNullOrWhiteSpace(param))
    return "";

            // δ����ֵ
            if (param == "$" || param == "*")
    return null!;

            // ���� (#123)
       if (param.StartsWith('#'))
            {
         if (int.TryParse(param.Substring(1), out var refId))
         return new StepReference(refId);
 }

         // �ַ��� ('text')
    if (param.StartsWith('\'') && param.EndsWith('\''))
{
          return param.Substring(1, param.Length - 2);
            }

    // ����
 if (double.TryParse(param, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
    {
          return number;
         }

     // Ƕ��ʵ�� (TYPE(...))
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

   // ������������ַ���
          return param;
     }

     /// <summary>
        /// ��ȡʵ��
        /// </summary>
        public StepEntity? GetEntity(int id)
        {
            return _entities.TryGetValue(id, out var entity) ? entity : null;
        }

        /// <summary>
   /// ��ȡ����ָ�����͵�ʵ��
        /// </summary>
   public List<StepEntity> GetEntitiesByType(string type)
        {
       return _entities.Values
         .Where(e => e.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
