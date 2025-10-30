namespace Path.Services.Step214
{
    /// <summary>
    /// STEP 实体基类
    /// </summary>
    public class StepEntity
    {
        public int Id { get; set; }
public string Type { get; set; } = string.Empty;
        public List<object> Parameters { get; set; } = new();
    }

/// <summary>
    /// STEP 实体引用
    /// </summary>
    public class StepReference
    {
        public int Id { get; set; }

        public StepReference(int id)
   {
            Id = id;
    }

        public override string ToString() => $"#{Id}";
    }
}
