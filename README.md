# 3D ·���༭��

���� .NET 8 �� WPF �� 3D ·���滮�Ϳ��ӻ��༭���ߣ�רΪ USV������ˮ��ͧ��·���滮��ơ�

## ? ����

- ?? **רҵ 3D �༭**: ���� HelixToolkit.Wpf ��ǿ�� 3D ���ӻ�
- ?? **UG NX ������**: ����רҵ CAD ���ϰ�ߵĽ�����ʽ
- ?? **���ܲ�׽ϵͳ**: ����/��/�е�/�˵�/ͶӰ���ֲ�׽ģʽ
- ? **ʵʱˢ��**: �����޸ĺ� 3D ��ͼ��ʱ����
- ?? **ֱ�ۿ���**: 
  - �м���ת��ͼ
  - Shift+�м�ƽ��
  - ������������
  - ��ݼ���ͼ�л�
- ?? **���ݳ־û�**: XML ��ʽ����/����
- ??? **MVVM �ܹ�**: �����Ĵ�����֯�Ϳ�ά����

## ?? ��ͼ

*(����ӽ�ͼ)*

## ?? ���ٿ�ʼ

### ϵͳҪ��

- Windows 10/11 (64-bit)
- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

### ��װ

1. �� [Releases](https://github.com/chenhangwei/Path/releases) �������°汾
2. ��ѹ������Ŀ¼
3. ���� `Path.exe`

���ߴ�Դ����룺

```bash
git clone https://github.com/chenhangwei/Path.git
cd Path
dotnet build
dotnet run
```

### ����ʹ��

1. **��Ӳ���**: ��� `? ��Ӳ���` �� `Ctrl+N`
2. **��� USV**: ��� `?? ��� USV` �� `Ctrl+U`
3. **�༭����**: ���м�����ݱ���б༭����Ͳ���
4. **�鿴 3D**: �Ҳ� 3D ��ͼ�Զ���ʾ·��
5. **������Ŀ**: ��� `?? ����` �� `Ctrl+S`

��ϸʹ��˵������� [���ٿ�ʼָ��](docs/03-�û��ֲ�/���ٿ�ʼ.md)

## ?? ����ָ��

### ������

| ���� | ���� |
|------|------|
| �м��϶� | ��ת��ͼ |
| Shift + �м� | ƽ����ͼ |
| ���� | ������ͼ |
| ������� | ѡ����Ƶ� |
| ����϶� | �ƶ����Ƶ� |

### ���̿�ݼ�

| ��ݼ� | ���� |
|--------|------|
| `F` | ��Ӧ���� |
| `Ctrl+1/2/3/4` | �л���ͼ����/ǰ/��/����⣩ |
| `Home` | ������ͼ |
| `F9/F10/F11` | �л���׽ģʽ |
| `Ctrl+O` | �����ļ� |
| `Ctrl+S` | �����ļ� |
| `Ctrl+N` | ��Ӳ��� |
| `Ctrl+U` | ��� USV |

���������ֲ�: [UG NX ����ָ��](docs/03-�û��ֲ�/UGNX����ָ��.md)

## ?? �ĵ�

�������ĵ���ϵ��������Һ�ѧϰ��

### ?? ��Ŀ����
- [��Ŀ�ṹ](docs/01-��Ŀ����/��Ŀ�ṹ.md) - ��ϸ����Ŀ�ܹ�˵��
- [����ջ](docs/01-��Ŀ����/����ջ.md) - ʹ�õļ����Ϳ��

### ??? ����ָ��
- [3D�༭��Ǩ��ָ��](docs/02-����ָ��/����ʵ��/3D�༭��Ǩ��ָ��.md)
- [���ܲ�׽����ָ��](docs/02-����ָ��/����ʵ��/���ܲ�׽����ָ��.md)
- [ʵʱˢ��ָ��](docs/02-����ָ��/����ʵ��/ʵʱˢ��ָ��.md)
- [UG NX����Ż�](docs/02-����ָ��/����ʵ��/UGNX����Ż�.md)
- [Siemens���Ľ�](docs/02-����ָ��/����ʵ��/Siemens���Ľ�.md)

### ?? �û��ֲ�
- [���ٿ�ʼ](docs/03-�û��ֲ�/���ٿ�ʼ.md) - ���û�����ָ��
- [UG NX����ָ��](docs/03-�û��ֲ�/UGNX����ָ��.md) - ���������ֲ�
- [���ܲ�׽����](docs/03-�û��ֲ�/���ܲ�׽����.md) - ��ȷ��λ����

### ?? ά����־
- [�ع���־](docs/04-ά����־/�ع���־.md) - �����ع���¼
- [UG NX�Ż��ܽ�](docs/04-ά����־/UGNX�Ż��ܽ�.md) - �Ż����̼�¼
- [�����ʷ](docs/04-ά����־/�����ʷ.md) - �汾������ʷ

**?? �����ĵ�����**: [docs/README.md](docs/README.md)

## ??? ����ջ

- **���**: .NET 8.0
- **UI**: WPF (Windows Presentation Foundation)
- **3D ��Ⱦ**: HelixToolkit.Wpf 2.22.0
- **MVVM**: CommunityToolkit.Mvvm 8.4.0
- **����ע��**: Microsoft.Extensions.DependencyInjection 8.0.0
- **�ܹ�ģʽ**: MVVM

��ϸ˵��: [����ջ�ĵ�](docs/01-��Ŀ����/����ջ.md)

## ?? ��Ŀ�ṹ

```
Path/
������ docs/   # ?? �����ĵ�
��   ������ 01-��Ŀ����/# ��Ŀ���ܺͼܹ�
��   ������ 02-����ָ��/   # �������ĵ�
�������� 03-�û��ֲ�/   # �û�����ָ��
��   ������ 04-ά����־/   # �����ά����¼
������ Models/            # ����ģ��
������ ViewModels/   # ��ͼģ��
������ Views/       # �û�����
��   ������ PathEditor3D/  # 3D �༭�����
������ Services/  # �����
������ Helpers/           # ��������

```

��ϸ�ṹ: [��Ŀ�ṹ�ĵ�](docs/01-��Ŀ����/��Ŀ�ṹ.md)

## ?? ����

��ӭ���ף�����ѭ���²��裺

1. Fork ���ֿ�
2. �������Է�֧ (`git checkout -b feature/AmazingFeature`)
3. �ύ���� (`git commit -m 'Add some AmazingFeature'`)
4. ���͵���֧ (`git push origin feature/AmazingFeature`)
5. ���� Pull Request

### �����淶

����� [����淶](.github/copilot-instructions.md) �˽⣺
- C# ����淶
- MVVM ģʽҪ��
- WPF ���ʵ��
- ����Լ��

## ?? ���֤

����Ŀ���� Apache License 2.0 ���֤ - ��� [LICENSE](LICENSE) �ļ�

## ?? ��л

- [HelixToolkit](https://github.com/helix-toolkit/helix-toolkit) - ǿ��� WPF 3D ���߰�
- [CommunityToolkit](https://github.com/CommunityToolkit/dotnet) - MVVM ���߼�
- UG NX - ������������Դ

## ?? ��ϵ��ʽ

- **���ⷴ��**: [GitHub Issues](https://github.com/chenhangwei/Path/issues)
- **���ܽ���**: [GitHub Discussions](https://github.com/chenhangwei/Path/discussions)
- **��Ŀ��ҳ**: [https://github.com/chenhangwei/Path](https://github.com/chenhangwei/Path)

---

**? ������������Ŀ�����а���������� Star��**

---

**�汾**: 2.0.0  
**������**: 2024-12-22  
**ά����**: ��Ŀ�Ŷ�