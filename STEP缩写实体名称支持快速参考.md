# STEP 缩写实体名称支持快速参考

## ? 问题解决

**问题**: STEP 文件使用缩写实体名称（如 CRTPNT, DRCTN）  
**解决**: 已添加自动识别和标准化功能

---

## ?? 支持的缩写

| 缩写 | 标准名称 | 类型 |
|------|---------|------|
| **CRTPNT** | CARTESIAN_POINT | 点 |
| **DRCTN** | DIRECTION | 方向 |
| **TRMCRV** | TRIMMED_CURVE | 曲线 |
| **PRLYAS** | POLYLINE | 曲线 |
| **BSPCRV** | B_SPLINE_CURVE | 曲线 |
| **VCT/VCTR** | VECTOR | 向量 |
| **AX2PL3** | AXIS2_PLACEMENT_3D | 坐标系 |

完整列表: `STEP缩写实体名称支持说明.md`

---

## ?? 使用方法

### 导入

```
1. 导入STEP (Ctrl+I)
2. 选择文件
3. 自动识别缩写
4. 正常显示曲线
```

### 诊断

如提示"未找到曲线"：

```
1. 生成诊断报告
2. 查看"检测到缩写实体名称"部分
3. 确认是否正确识别
```

---

## ?? 诊断报告示例

```
实体类型统计 (标准化后):
  CARTESIAN_POINT  : 35
  LINE   : 32
  ...

检测到缩写实体名称:
  CRTPNT : 35 → CARTESIAN_POINT
  TRMCRV : 32 → TRIMMED_CURVE
  ...

? 文件包含曲线数据，应该可以正常导入
  注意: 文件使用了缩写实体名称（已自动处理）
```

---

## ? 验证

```
编译: 生成成功 ?
缩写识别: ?
曲线提取: ?
诊断报告: ?
```

---

**详细说明**: `STEP缩写实体名称支持说明.md`
