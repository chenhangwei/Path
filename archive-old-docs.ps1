# �ĵ�����ű�
# �˽ű����ɵĸ�Ŀ¼ Markdown �ļ��ƶ��� archive Ŀ¼
# ʹ��ǰ����ȷ�����ĵ��Ѿ����Ƴɹ�

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   �ĵ�����ű� v1.0" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ����Ƿ�����Ŀ��Ŀ¼
if (-not (Test-Path "Path.csproj")) {
    Write-Host "����: ������Ŀ��Ŀ¼���д˽ű�!" -ForegroundColor Red
    exit 1
}

# �����鵵Ŀ¼
$archiveDir = "docs_archive"
if (-not (Test-Path $archiveDir)) {
    New-Item -ItemType Directory -Path $archiveDir -Force | Out-Null
    Write-Host "? �����鵵Ŀ¼: $archiveDir" -ForegroundColor Green
}

# ��Ҫ�鵵���ļ��б�
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

Write-Host "׼���鵵�����ļ�:" -ForegroundColor Yellow
foreach ($file in $filesToArchive) {
    if (Test-Path $file) {
 Write-Host "  - $file" -ForegroundColor White
    }
}
Write-Host ""

# ѯ���û�ȷ��
$confirmation = Read-Host "ȷ��Ҫ����Щ�ļ��ƶ����鵵Ŀ¼��? (y/n)"
if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
    Write-Host "������ȡ��" -ForegroundColor Yellow
 exit 0
}

Write-Host ""
Write-Host "��ʼ�鵵..." -ForegroundColor Cyan

# �ƶ��ļ�
$movedCount = 0
$notFoundCount = 0

foreach ($file in $filesToArchive) {
    if (Test-Path $file) {
        try {
    Move-Item -Path $file -Destination $archiveDir -Force
       Write-Host "? �ѹ鵵: $file" -ForegroundColor Green
            $movedCount++
    }
        catch {
      Write-Host "? �鵵ʧ��: $file" -ForegroundColor Red
            Write-Host "  ����: $_" -ForegroundColor Red
        }
  }
    else {
        Write-Host "? �ļ�������: $file" -ForegroundColor Gray
      $notFoundCount++
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "�鵵���!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "���ƶ�: $movedCount ���ļ�" -ForegroundColor Green
Write-Host "δ�ҵ�: $notFoundCount ���ļ�" -ForegroundColor Gray
Write-Host ""
Write-Host "���ļ����ƶ���: $archiveDir/" -ForegroundColor Yellow
Write-Host "���ĵ�λ��: docs/" -ForegroundColor Green
Write-Host ""
Write-Host "��ʾ:" -ForegroundColor Cyan
Write-Host "- �������ĵ��Ƿ�����" -ForegroundColor White
Write-Host "- ȷ����������ɾ���鵵Ŀ¼" -ForegroundColor White
Write-Host "- ���߽��鵵Ŀ¼��ӵ� .gitignore" -ForegroundColor White
Write-Host ""

# ��ѡ������һ��ժҪ�ļ�
$summaryFile = Join-Path $archiveDir "�鵵˵��.txt"
$summary = @"
�ĵ��鵵˵��
============

�鵵ʱ��: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

��Щ�ļ��ѱ��µ��ĵ��ṹȡ�����鵵�Թ��ο���

���ĵ�λ��:
- ��Ŀ����: docs/01-��Ŀ����/
- ����ָ��: docs/02-����ָ��/
- �û��ֲ�: docs/03-�û��ֲ�/
- ά����־: docs/04-ά����־/

�ĵ�����: docs/README.md

�鵵�ļ��б�:
"@

foreach ($file in $filesToArchive) {
    $summary += "`n- $file"
}

Set-Content -Path $summaryFile -Value $summary -Encoding UTF8
Write-Host "? �Ѵ����鵵˵���ļ�: $summaryFile" -ForegroundColor Green
Write-Host ""
