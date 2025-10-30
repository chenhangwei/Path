# Path ��Ŀ�ṹ�ĵ�

## ��Ŀ����
����һ������ .NET 8 �� WPF �� 3D ·���༭���Ϳ��ӻ�Ӧ�ó���

## ����ջ
- **���**: .NET 8 (net8.0-windows)
- **UI ���**: WPF (Windows Presentation Foundation)
- **3D ��Ⱦ**: HelixToolkit.Wpf 2.22.0
- **MVVM ���**: CommunityToolkit.Mvvm 8.2.2
- **����ע��**: Microsoft.Extensions.DependencyInjection 8.0.0
- **�ܹ�ģʽ**: MVVM (Model-View-ViewModel)

## ��Ŀ����
- **�ɿ���������**: ������ (`<Nullable>enable</Nullable>`)
- **��ʽ Usings**: ������ (`<ImplicitUsings>enable</ImplicitUsings>`)

## Ŀ¼�ṹ

```
Path/
������ Models/# ����ģ�ͣ������ݣ�ʵ�� INotifyPropertyChanged��
��   ������ StepModel.cs            # Step ����ģ��
��   ������ UsvModel.cs       # USV������ˮ��ͧ������ģ��
��
������ ViewModels/    # ��ͼģ�ͣ�ҵ���߼������
��   ������ MainViewModel.cs        # ��������ͼģ�ͣ�ʹ�� CommunityToolkit.Mvvm
��
������ Views/         # ��ͼ��XAML �ʹ�����ã�
��   ������ PathEditor3D.xaml  # 3D ·���༭���û��ؼ�
��   ������ PathEditor3D.xaml.cs    # 3D �༭���������
��   ������ PathEditor3D.Snap.cs    # 3D �༭�����ܲ�׽��չ��partial class��
��
������ Services/  # ����㣨ҵ���߼��� I/O��
��   ������ IPathDataService.cs     # ·�����ݷ���ӿ�
��   ������ XmlPathDataService.cs   # XML ���ݵ���/��������ʵ��
��   ������ IDialogService.cs       # �Ի������ӿ�
��   ������ WpfDialogService.cs     # WPF �Ի������ʵ��
��
������ Helpers/        # ������͹���
��   ������ RelayCommand.cs         # ICommand ʵ�֣�δʹ�ã���Ŀʹ�� CommunityToolkit.Mvvm��
��
������ .github/
��   ������ copilot-instructions.md # GitHub Copilot ָ��˵�������ģ�
��
������ App.xaml      # Ӧ�ó�����Դ����������
������ App.xaml.cs                 # Ӧ�ó�����ڣ���������ע��
������ MainWindow.xaml             # ������ XAML
������ MainWindow.xaml.cs # �����ڴ������
������ AssemblyInfo.cs # ������Ϣ
������ Path.csproj       # ��Ŀ�ļ�

```

## ��Ҫ���˵��

### Models��ģ�Ͳ㣩
- **StepModel**: ��ʾ·���е�һ�����裬������� USV
- **UsvModel**: ��ʾһ�� USV ��λ�ú����ԣ�X, Y, Z, Yaw, Speed��
- ����ģ�Ͷ�ʹ�� `CallerMemberName` �Ż����Ա��֪ͨ

### ViewModels����ͼģ�Ͳ㣩
- **MainViewModel**: ����ͼģ��
  - ʹ�� `CommunityToolkit.Mvvm` �� `[ObservableProperty]` �� `[RelayCommand]`
  - ���� Steps ���Ϻ͵�ǰѡ�е� Step
  - �ṩ���ImportXml, ExportXml, AddStep, RemoveStep, MoveStepUp, MoveStepDown, AddUsv, RemoveUsv
  - ͨ������ע���ȡ����

### Views����ͼ�㣩
- **PathEditor3D**: 3D ·���༭���ؼ�
  - ʹ�� HelixToolkit ���� 3D ��Ⱦ
  - ֧����꽻������ק�����š�ƽ��
  - ���ܲ�׽���ܣ����񡢶˵㡢�е㡢ͶӰ��
  - �Ӿ�������߼�����ת��
  - ֧�ֻ��ߺ�ֱ�߶���Ⱦ

### Services������㣩
- **IPathDataService / XmlPathDataService**: 
  - XML ���ݵ���/����
  - ������֤
  
- **IDialogService / WpfDialogService**:
  - �ļ��Ի���
  - ��Ϣ�Ի���
  - ȷ�϶Ի���

### ����ע������
�� `App.xaml.cs` �����ã�
```csharp
services.AddSingleton<IPathDataService, XmlPathDataService>();
services.AddSingleton<IDialogService, WpfDialogService>();
services.AddTransient<MainViewModel>();
services.AddTransient<MainWindow>();
```

## ���ģʽ�����ʵ��

### MVVM ģʽ
- **Model**: �������࣬ʵ�� `INotifyPropertyChanged`
- **View**: XAML �ļ��������� UI ����
- **ViewModel**: ҵ���߼��������״̬����

### ����ģʽ
- ʹ�� `CommunityToolkit.Mvvm` �� `[RelayCommand]` ����
- ֧�� `CanExecute` �߼�

### ����λ
- ͨ������ע���������������������
- �ӿں�ʵ�ַ��룬���ڲ��Ժ��滻

### �ɿ���������
- ��Ŀ�����˿ɿ��������ͼ��
- ʹ�� `?` ��ǿɿ�����
- ʹ�� `??` �� `?.` ���п�ֵ����

## ����淶

### ����Լ��
- **����**: PascalCase���� `MainViewModel`, `PathEditor3D`��
- **������**: PascalCase���� `LoadPath()`, `SaveChanges()`��
- **˽���ֶ�**: _camelCase���� `_currentStep`, `_pathData`��
- **����**: PascalCase���� `CurrentPosition`, `IsEnabled`��
- **���ر���**: camelCase

### MVVM �ض��淶
- ViewModel λ�� `ViewModels/` Ŀ¼
- ʹ�� `CommunityToolkit.Mvvm` ��Դ����������
- ���ݰ�������ֱ�Ӳ��� UI

### WPF �ض��淶
- XAML �ؼ�ʹ�� `x:Name` ��������
- ����ʹ�ò���������Grid, StackPanel, DockPanel��
- ��ʽ��ģ��Ӧ�ÿɸ���

## ���Ľ���
1. ~~ɾ��δʹ�õ� `Helpers/RelayCommand.cs`~~�������Ա������ݣ�
2. ��ӵ�Ԫ����
3. ��Ӵ�����־��¼
4. ������ӳ���/��������
5. �Ż� 3D ��Ⱦ����

## ά������
- ��ѭ��Ŀ�еı���淶
- �¹���Ӧ��ͨ������ͽӿ�ʵ�֣����ڲ���
- ���� MVVM �ֲ�����
- ʹ������ע����������������
- ע���ڴ�й©���ر����¼����ĺ� 3D ��Դ

## ����������
```bash
# ������Ŀ
dotnet build

# ������Ŀ
dotnet run
```

## �汾��Ϣ
- .NET SDK: 8.0
- Ŀ����: net8.0-windows
- C# ���԰汾: Ĭ�ϣ�C# 12��
