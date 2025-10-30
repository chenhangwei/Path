# UG NX ����Ż��ܽ�

## �Ż�Ŀ��

�� 3D ·���༭���Ĳ�����ʽ��Ϊ���� UG NX ��רҵ��ģ�������ģʽ��

## ����ɵ��Ż�

### 1. �ļ�����
- ? ɾ���ظ����ඨ�壨Step.cs, Usvs.cs��
- ? ������������Ŀ�ĵ���PROJECT_STRUCTURE.md��
- ? ����������־��REFACTORING_LOG.md��

### 2. ��ӵ��¹��ܣ���ƣ�

#### ������
- **�м��϶�** �� ��ת��ͼ���Ƴ������ģ�
- **Shift + �м��϶�** �� ƽ����ͼ
- **����** �� �������ţ������λ��Ϊ���ģ�
- **���** �� ѡ�����ק���Ƶ�
- **�Ҽ�** �� ����ɾ�����Ƶ�

#### ���̿�ݼ�
- **F** �� ��Ӧ���ڣ�Fit to View��
- **Ctrl + 1** �� ����ͼ
- **Ctrl + 2** �� ǰ��ͼ
- **Ctrl + 3** �� ����ͼ
- **Ctrl + 4** �� �������ͼ
- **Home** �� ������ͼ

### 3. ��ͼ���Ʒ���

��ʵ�ֵķ�����
- `FitToView()` - �Զ���Ӧ��ͼ
- `SetTopView()` - �л�������ͼ
- `SetFrontView()` - �л���ǰ��ͼ
- `SetRightView()` - �л�������ͼ
- `SetIsometricView()` - �л����������ͼ
- `ResetView()` - ������ͼ
- `GetSceneCenter()` - ���㳡������

### 4. �������

�� OrthographicCamera ��Ϊ PerspectiveCamera��
```csharp
var camera = new PerspectiveCamera
{
    Position = new Point3D(100, 100, 100),
    LookDirection = new Vector3D(-1, -1, -1),
    UpDirection = new Vector3D(0, 0, 1),
    FieldOfView = 45,
    NearPlaneDistance = 0.1,
FarPlaneDistance = 10000
};
```

## ����������

### �����ѵ�
1. �ļ��༭�����д���ṹ���ƻ�
2. �������������´����������
3. �¾ɴ����ϵ��³�ͻ

### ����Ľ������

#### ���� A���ع������¿�ʼ
```bash
git reset --hard HEAD~5  # �ع�������ǰ
git clean -fd
```

Ȼ�����²�����У�
1. ��ֻ��Ӽ��̿�ݼ�����
2. ����ͨ���������ͼ���Ʒ���
3. �������������

#### ���� B���ֶ��޸�
������뱣������������������Ҫ��

1. **�޸� HelixView_MouseDown ����**
```csharp
private void HelixView_MouseDown(object? sender, MouseButtonEventArgs e)
{
    // ֻ�������
    if (e.LeftButton != MouseButtonState.Pressed) return;
    
    var pt = e.GetPosition(HelixView.Viewport);
    
    // hit-test �߼�
    GeometryModel3D? hitModel = null;
    VisualTreeHelper.HitTest(HelixView.Viewport, null,
        result =>
 {
            if (result is RayHitTestResult rayResult && 
      rayResult is RayMeshGeometry3DHitTestResult meshResult)
      {
    if (meshResult.ModelHit is GeometryModel3D gm)
    {
    if (_modelMap.ContainsKey(gm)) 
         { 
      hitModel = gm; 
          return HitTestResultBehavior.Stop; 
        }
            if (_gizmoHandleMap.ContainsKey(gm)) 
    { 
    hitModel = gm; 
        return HitTestResultBehavior.Stop; 
           }
         }
            }
  return HitTestResultBehavior.Continue;
     },
        new PointHitTestParameters(pt));

    if (hitModel != null)
{
    // ���� Gizmo ��ק
        if (_gizmoHandleMap.TryGetValue(hitModel, out var axisWorld))
 {
   // ... Gizmo �����߼�
 }
      
  // ������Ƶ���ק��ɾ��
        if (_modelMap.TryGetValue(hitModel, out var tup))
      {
            if (Mouse.RightButton == MouseButtonState.Pressed)
       {
         // ɾ����
          OnPointDeleted?.Invoke(tup.ui, tup.si, tup.pi);
          }
            else
        {
     // ��ʼ��ק
      _isDragging = true;
     _dragRef3D = tup;
         _dragModel = hitModel;
           // ...
            }
            e.Handled = true;
        }
    }
    else
  {
  // ����հ�ƽ��
        var planePt = HitPlaneAtPoint(pt);
        if (planePt.HasValue)
    {
  var visualSnapped = ApplySmartSnapping3D(planePt.Value, pt);
    var logical3 = VisualToLogical(visualSnapped);
       
  if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && 
            _hoveredSeg.HasValue)
{
        // ����㵽�߶�
   OnPointInserted?.Invoke(_hoveredSeg.Value.ui, _hoveredSeg.Value.si, logical3);
            }
   else
            {
  // ��ͨ���
          OnPlaneClicked?.Invoke(logical3);
            }
            e.Handled = true;
        }
    }
}
```

2. **ȷ�������¼���������ȷ����**
3. **����ÿ������**

## ��һ���ƻ�

### ����Ŀ�꣨����Ƶ�δʵ�֣�
1. ���������Ƶ�����ʵ��
2. �����ͼ�л�����
3. �Ż�����

### ����Ŀ��
1. ��� Gizmo 3D ������
2. ʵ�ֶ�ѡ����
3. ��Ӳ�������

### ����Ŀ��
1. ��ӿ�ݼ��Զ���
2. ʵ�ֹ���ƽ���л�
3. ֧�ֲ��ϵͳ

## �ο��ĵ�

�Ѵ������ĵ���
- `UG_NX_OPERATION_GUIDE.md` - UG NX ����ָ��
- `PROJECT_STRUCTURE.md` - ��Ŀ�ṹ�ĵ�
- `REFACTORING_LOG.md` - ������־

## ����

���ڵ�ǰ���뱻�ƻ���ǿ�ҽ��飺
1. ʹ�� Git �ع����ȶ��汾
2. �����µĹ��ܷ�֧
3. ����ӹ��ܲ�����
4. ÿ���һ�����ܾ��ύ

��������ȷ��ÿһ�����ǿɹ����ģ����ڶ�λ���⡣

## ��ϵ��ʽ

����������룺
- ��� Git ��ʷ�ҵ������ȶ��汾
- �鿴 `PROJECT_STRUCTURE.md` �˽���Ŀ�ṹ
- �鿴 `UG_NX_OPERATION_GUIDE.md` �˽���ƵĹ���

---

**ע��**����ǰ���봦�ڲ�����״̬����Ҫ�ع����ֶ��޸���
