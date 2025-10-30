# STEP 214 导入快速参考

## ? 功能完成

**完整的 STEP 214 文件导入功能！**

---

## ?? 支持的文件

| 格式 | 扩展名 | 状态 |
|------|-------|------|
| STEP 214 (AP214) | .step, .stp | ? 支持 |
| ISO-10303-21 | .step, .stp | ? 支持 |

---

## ?? 支持的曲线

| 曲线类型 | 状态 |
|---------|------|
| B-Spline | ? |
| Bezier | ? |
| NURBS | ? |
| Polyline | ? |
| Line | ? |
| Circle | ? |
| Ellipse | ? |
| Trimmed Curve | ? |
| Composite Curve | ? |

---

## ??? 支持的 3D 软件

? CATIA V5/V6  
? SolidWorks  
? Siemens NX  
? AutoCAD  
? Inventor  
? Rhino  
? Fusion 360  
? FreeCAD  

---

## ?? 使用方法

### 3 步导入

```
1. ?? 点击"导入STEP" (Ctrl+I)
2. ?? 选择 STEP 文件
3. ? 曲线自动显示在 3D 视图
```

### 完整工作流

```
导入STEP → 自动命名 → 放样全部 → 生成步骤 → 导出XML
```

---

## ?? 导出设置建议

### CATIA
```
格式: STEP AP214
单位: 毫米
```

### SolidWorks
```
格式: STEP 214
选项: 保存所有曲线
```

### NX
```
格式: STEP214
导出: 线框几何
```

---

## ?? 常见问题

### 未提取到曲线？

```
? 确认导出时包含曲线（不是实体）
? 选择 STEP AP214 格式
? 检查文件是否包含线框几何
```

### 格式错误？

```
? 检查文件头是否包含 "ISO-10303"
? 确认扩展名为 .step 或 .stp
? 用文本编辑器检查文件格式
```

---

## ?? 新增文件

```
Services/Step214/
├── StepEntity.cs      # STEP 实体定义
├── Step214Parser.cs   # STEP 文件解析器
└── Step214CurveExtractor.cs  # 曲线提取器
```

---

## ?? 主要特性

- ? 支持 10+ 种曲线类型
- ? 兼容主流 3D 软件
- ? 自动解析和显示
- ? 实时 3D 预览
- ? 错误容忍处理

---

**详细文档**: `STEP214导入功能完善总结.md`
