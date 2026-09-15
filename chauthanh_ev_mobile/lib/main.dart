import 'package:flutter/material.dart';
import 'package:open_filex/open_filex.dart';
import 'package:package_info_plus/package_info_plus.dart';

import 'constants/app_theme.dart';
import 'services/app_update_service.dart';

void main() => runApp(const ChauthanhEvApp());

class ChauthanhEvApp extends StatelessWidget {
  const ChauthanhEvApp({super.key});

  @override
  Widget build(BuildContext context) => MaterialApp(
    title: 'CHÂU THÀNH EV',
    debugShowCheckedModeBanner: false,
    theme: AppTheme.darkTheme,
    home: const HomeScreen(),
  );
}

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(
      title: const Text('CHÂU THÀNH EV'),
      actions: [
        IconButton(
          tooltip: 'Cài đặt',
          icon: const Icon(Icons.settings_outlined),
          onPressed: () => Navigator.of(
            context,
          ).push(MaterialPageRoute(builder: (_) => const SettingsScreen())),
        ),
      ],
    ),
    body: ListView(
      padding: const EdgeInsets.all(20),
      children: [
        Text(
          'Quản lý trạm sạc',
          style: Theme.of(context).textTheme.headlineSmall,
        ),
        const SizedBox(height: 12),
        const Card(
          child: ListTile(
            leading: Icon(Icons.ev_station, color: AppTheme.accentCyan),
            title: Text('Mạng lưới TTC Châu Thành'),
            subtitle: Text('TTC Châu Thành 1 và TTC Châu Thành 2'),
          ),
        ),
      ],
    ),
  );
}

class SettingsScreen extends StatefulWidget {
  const SettingsScreen({super.key});

  @override
  State<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends State<SettingsScreen> {
  final _updateService = const AppUpdateService();
  String _version = 'Đang tải...';
  bool _isChecking = false;
  double? _progress;
  String? _message;
  bool _isError = false;

  @override
  void initState() {
    super.initState();
    _loadVersion();
  }

  Future<void> _loadVersion() async {
    final info = await PackageInfo.fromPlatform();
    if (mounted) {
      setState(() => _version = '${info.version} (${info.buildNumber})');
    }
  }

  Future<void> _updateSoftware() async {
    setState(() {
      _isChecking = true;
      _progress = null;
      _message = 'Đang kiểm tra phiên bản mới...';
      _isError = false;
    });

    try {
      final result = await _updateService.checkForUpdate();
      if (!mounted) {
        return;
      }
      if (!result.isAvailable) {
        setState(() {
          _isChecking = false;
          _message = 'Ứng dụng đang ở phiên bản mới nhất.';
        });
        return;
      }

      final update = result.update!;
      final shouldInstall = await showDialog<bool>(
        context: context,
        builder: (dialogContext) => AlertDialog(
          title: Text(
            update.mandatory ? 'Cần cập nhật ứng dụng' : 'Có phiên bản mới',
          ),
          content: Text(
            'Phiên bản ${update.version} (${update.buildNumber}) đã sẵn sàng.\n\n${update.releaseNotes}',
          ),
          actions: [
            if (!update.mandatory)
              TextButton(
                onPressed: () => Navigator.pop(dialogContext, false),
                child: const Text('Để sau'),
              ),
            FilledButton(
              onPressed: () => Navigator.pop(dialogContext, true),
              child: const Text('Cập nhật'),
            ),
          ],
        ),
      );
      if (shouldInstall != true) {
        setState(() {
          _isChecking = false;
          _message = 'Đã bỏ qua bản cập nhật.';
        });
        return;
      }

      setState(() => _message = 'Đang tải bản cập nhật...');
      final installResult = await _updateService.downloadAndInstall(
        update,
        onProgress: (progress) {
          if (mounted) setState(() => _progress = progress);
        },
      );
      if (!mounted) {
        return;
      }
      setState(() {
        _isChecking = false;
        _message = installResult.type == ResultType.done
            ? 'Đã mở trình cài đặt. Hãy xác nhận cài đặt trên Android.'
            : 'Không thể mở trình cài đặt: ${installResult.message}';
        _isError = installResult.type != ResultType.done;
      });
    } catch (error) {
      if (!mounted) return;
      setState(() {
        _isChecking = false;
        _isError = true;
        _message = error.toString().replaceFirst('Exception: ', '');
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final updateConfigured = AppUpdateConfig.manifestUrl.isNotEmpty;
    return Scaffold(
      appBar: AppBar(title: const Text('Cài đặt')),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          const Card(
            child: ListTile(
              leading: Icon(Icons.ev_station, color: AppTheme.accentCyan),
              title: Text('CHÂU THÀNH EV'),
              subtitle: Text('Ứng dụng hỗ trợ mạng lưới trạm sạc cao tốc'),
            ),
          ),
          const SizedBox(height: 16),
          Card(
            child: ListTile(
              leading: const Icon(Icons.info_outline),
              title: const Text('Phiên bản hiện tại'),
              subtitle: Text(_version),
            ),
          ),
          const SizedBox(height: 12),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Cập nhật phần mềm',
                    style: TextStyle(fontWeight: FontWeight.w700),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    updateConfigured
                        ? 'Kiểm tra và cài đặt bản Android mới nhất.'
                        : 'Chưa cấu hình máy chủ cập nhật. Hãy build với APP_UPDATE_MANIFEST_URL.',
                  ),
                  const SizedBox(height: 16),
                  FilledButton.icon(
                    onPressed: _isChecking || !updateConfigured
                        ? null
                        : _updateSoftware,
                    icon: _isChecking
                        ? const SizedBox(
                            width: 18,
                            height: 18,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Icon(Icons.system_update_alt),
                    label: Text(
                      _isChecking ? 'Đang kiểm tra...' : 'Kiểm tra cập nhật',
                    ),
                  ),
                  if (_progress != null) ...[
                    const SizedBox(height: 12),
                    LinearProgressIndicator(value: _progress),
                    const SizedBox(height: 4),
                    Text('${(_progress! * 100).round()}%'),
                  ],
                  if (_message != null) ...[
                    const SizedBox(height: 12),
                    Text(
                      _message!,
                      style: TextStyle(
                        color: _isError
                            ? AppTheme.accentRed
                            : AppTheme.textSecondary,
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),
          const Text(
            'Lưu ý: Android sẽ yêu cầu bạn xác nhận cài đặt. Ứng dụng không thể tự cài đè âm thầm vì giới hạn bảo mật của hệ điều hành.',
            style: TextStyle(color: AppTheme.textMuted, fontSize: 12),
          ),
        ],
      ),
    );
  }
}
