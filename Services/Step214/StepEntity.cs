namespace Path.Services.Step214
{
    /// <summary>
    /// STEP ʵ�����
    /// </summary>
    public class StepEntity
    {
        public int Id { get; set; }
public string Type { get; set; } = string.Empty;
        public List<object> Parameters { get; set; } = new();
    }

/// <summary>
    /// STEP ʵ������
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
