# 文档清理脚本
# 此脚本将旧的根目录 Markdown 文件移动到 archive 目录
# 使用前请先确认新文档已经复制成功

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   文档清理脚本 v1.0" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 检查是否在项目根目录
if (-not (Test-Path "Path.csproj")) {
    Write-Host "错误: 请在项目根目录运行此脚本!" -ForegroundColor Red
    exit 1
}

# 创建归档目录
$archiveDir = "docs_archive"
if (-not (Test-Path $archiveDir)) {
    New-Item -ItemType Directory -Path $archiveDir -Force | Out-Null
    Write-Host "? 创建归档目录: $archiveDir" -ForegroundColor Green
}

# 需要归档的文件列表
$filesToArchive = @(
    "3D_APP_MIGRATION_GUIDE.md",
    "3D_SNAP_INTEGRATION_GUIDE.md",
    "REALTIME_REFRESH_GUIDE.md",
    "SIEMENS_STYLE_3D_IMPROVEMENTS.md",
    "UG_NX_OPERATION_GUIDE.md",
    "SNAP_SUMMARY.md",
    "REFACTORING_LOG.md",
    "UG_NX_OPTIMIZATION_SUMMARY.md",
  "PROJECT_STRUCTURE.md"
)

Write-Host "准备归档以下文件:" -ForegroundColor Yellow
foreach ($file in $filesToArchive) {
    if (Test-Path $file) {
 Write-Host "  - $file" -ForegroundColor White
    }
}
Write-Host ""

# 询问用户确认
$confirmation = Read-Host "确认要将这些文件移动到归档目录吗? (y/n)"
if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
    Write-Host "操作已取消" -ForegroundColor Yellow
 exit 0
}

Write-Host ""
Write-Host "开始归档..." -ForegroundColor Cyan

# 移动文件
$movedCount = 0
$notFoundCount = 0

foreach ($file in $filesToArchive) {
    if (Test-Path $file) {
        try {
    Move-Item -Path $file -Destination $archiveDir -Force
       Write-Host "? 已归档: $file" -ForegroundColor Green
            $movedCount++
    }
        catch {
      Write-Host "? 归档失败: $file" -ForegroundColor Red
            Write-Host "  错误: $_" -ForegroundColor Red
        }
  }
    else {
        Write-Host "? 文件不存在: $file" -ForegroundColor Gray
      $notFoundCount++
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "归档完成!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "已移动: $movedCount 个文件" -ForegroundColor Green
Write-Host "未找到: $notFoundCount 个文件" -ForegroundColor Gray
Write-Host ""
Write-Host "旧文件已移动到: $archiveDir/" -ForegroundColor Yellow
Write-Host "新文档位于: docs/" -ForegroundColor Green
Write-Host ""
Write-Host "提示:" -ForegroundColor Cyan
Write-Host "- 请检查新文档是否完整" -ForegroundColor White
Write-Host "- 确认无误后可以删除归档目录" -ForegroundColor White
Write-Host "- 或者将归档目录添加到 .gitignore" -ForegroundColor White
Write-Host ""

# 可选：创建一个摘要文件
$summaryFile = Join-Path $archiveDir "归档说明.txt"
$summary = @"
文档归档说明
============

归档时间: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

这些文件已被新的文档结构取代，归档以供参考。

新文档位置:
- 项目概览: docs/01-项目概览/
- 开发指南: docs/02-开发指南/
- 用户手册: docs/03-用户手册/
- 维护日志: docs/04-维护日志/

文档导航: docs/README.md

归档文件列表:
"@

foreach ($file in $filesToArchive) {
    $summary += "`n- $file"
}

Set-Content -Path $summaryFile -Value $summary -Encoding UTF8
Write-Host "? 已创建归档说明文件: $summaryFile" -ForegroundColor Green
Write-Host ""
