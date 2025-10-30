# GitHub Copilot ��Ŀָ��˵��

## ��Ŀ����
����һ������ .NET 8 �� WPF Ӧ�ó������� 3D ·���༭�Ϳ��ӻ���

## ����ջ
- **���**: .NET 8 (net8.0-windows)
- **UI ���**: WPF (Windows Presentation Foundation)
- **3D ��Ⱦ**: HelixToolkit.Wpf 2.22.0
- **�ܹ�ģʽ**: MVVM (Model-View-ViewModel)

## ����淶�����ʵ��

### C# ����淶
1. **��ֵ����**: ��Ŀ������ Nullable �������� (`<Nullable>enable</Nullable>`)
   - ʼ����ȷ��ע�ɿ����� (`string?` vs `string`)
   - ʹ�� null �ϲ������ `??` �� null ��������� `?.`
   - ����ʹ�� null ��������� `!` ���Ǿ��Ա�Ҫ

2. **Using ����**: ��Ŀ��������ʽ usings (`<ImplicitUsings>enable</ImplicitUsings>`)
   - ����Ҫ��ӳ����� System �����ռ�
   - ֻ������Ŀ�ض��������ռ�

3. **����Լ��**:
   - ����ʹ�� PascalCase: `MainViewModel`, `PathEditor`
   - ������ʹ�� PascalCase: `LoadPath()`, `SaveChanges()`
   - ˽���ֶ�ʹ�� camelCase �� _camelCase: `_currentStep`, `pathData`
   - ����ʹ�� PascalCase: `CurrentPosition`, `IsEnabled`

### MVVM �ܹ�Ҫ��

1. **ViewModels ��֯**:
   - ���� ViewModel ���� `ViewModels/` Ŀ¼
   - ViewModel Ӧ�̳� `INotifyPropertyChanged` ��ʹ�� MVVM ��ܻ���
   - ʹ�� `RelayCommand` (λ�� `Helpers/RelayCommand.cs`) ��������

2. **Views ��֯**:
   - ���� View ���� `Views/` Ŀ¼
   - XAML �ļ��ʹ��������ļ��ɶԳ���
   - ��������Ӧ������࣬ҵ���߼����� ViewModel

3. **���ݰ�**:
   - ����ʹ�����ݰ󶨶��Ǵ���ֱ�Ӳ��� UI
   - ʹ�� `{Binding}` �﷨���� View �� ViewModel
   - �����ʹ�� `ICommand` �ӿ�

### WPF �ض�ָ��

1. **XAML ������**:
- ʹ��������� `x:Name` ����
   - ����ʹ�� Grid, StackPanel, DockPanel �Ȳ�������
   - ��ʽ��ģ��Ӧ���Ǹ�����

2. **3D ��Ⱦ (HelixToolkit)**:
   - ʹ�� HelixToolkit �� HelixViewport3D �ؼ�
   - 3D ģ�Ͳ���Ӧ�� ViewModel �д���
   - ע�� 3D ����������Ż�

### ��Ŀ�ض�����

1. **Step ��**:
   - ������ Step ���ļ� (`Step.cs` �� `ViewModels/Step.cs`)
   - ��ȷ��������ģ�ͺ���ͼģ��
   - ����ѭ������

2. **Helper ��**:
   - ��������� `Helpers/` Ŀ¼
   - ���ָ�����ĵ�һְ��
   - `RelayCommand` ����ʵ�� ICommand ģʽ

3. **���ܿ���**:
   - 3D ��Ⱦ����������Դ��ע���ڴ����
 - ��ȷʵ�� `IDisposable` �ͷŷ��й���Դ
   - �������ݰ�ʱʹ�����⻯����

### ��������Ҫ��

1. **�첽���**:
   - ʹ�� `async/await` �����ʱ����
   - �������� UI �߳�
   - �첽�������� `Async` ��β

2. **�쳣����**:
   - ʹ�� try-catch ����Ԥ���쳣
   - ��¼�쳣��Ϣ���ڵ���
   - ���û���ʾ�ѺõĴ�����Ϣ

3. **����ע��**:
   - ���� API ʹ�� XML �ĵ�ע�� (`///`)
   - �����㷨��ӽ�����ע��
   - �����Զ��׼���ע��

### ���Խ���
- Ϊ ViewModel ��д��Ԫ����
- �������ݰ󶨺�����ִ���߼�
- ʹ�� Mock �����������

## �ļ���֯�ṹ
```
Path/
������ Views/    # XAML ��ͼ�ļ�
������ ViewModels/    # ��ͼģ��
������ Helpers/   # ������͹���
������ Models/         # (����) ����ģ��
������ Services/         # (����) ҵ������
```

## ����ע�뽨��
������������ע������ (�� Microsoft.Extensions.DependencyInjection) ����ߴ���Ŀɲ����ԺͿ�ά���ԡ�

## �汾����
- ��ѭ Git ���ʵ��
- �ύ��ϢӦ����������������
- ��Ҫ�ύ `bin/` �� `obj/` Ŀ¼
