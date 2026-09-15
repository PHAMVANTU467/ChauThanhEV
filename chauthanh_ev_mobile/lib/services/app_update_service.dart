import 'dart:convert';
import 'dart:io';

import 'package:http/http.dart' as http;
import 'package:open_filex/open_filex.dart';
import 'package:package_info_plus/package_info_plus.dart';
import 'package:path_provider/path_provider.dart';

class AppUpdateConfig {
  const AppUpdateConfig._();

  // Set this at build time with --dart-define=APP_UPDATE_MANIFEST_URL=...
  static const manifestUrl = String.fromEnvironment('APP_UPDATE_MANIFEST_URL');
}

class AppUpdateInfo {
  const AppUpdateInfo({
    required this.version,
    required this.buildNumber,
    required this.apkUrl,
    required this.releaseNotes,
    this.mandatory = false,
  });

  final String version;
  final int buildNumber;
  final String apkUrl;
  final String releaseNotes;
  final bool mandatory;

  factory AppUpdateInfo.fromJson(Map<String, dynamic> json) {
    return AppUpdateInfo(
      version: json['version']?.toString() ?? '',
      buildNumber: int.tryParse(json['buildNumber']?.toString() ?? '') ?? 0,
      apkUrl: json['apkUrl']?.toString() ?? '',
      releaseNotes: json['releaseNotes']?.toString() ?? '',
      mandatory: json['mandatory'] == true,
    );
  }
}

class AppUpdateResult {
  const AppUpdateResult({
    required this.currentVersion,
    required this.currentBuildNumber,
    this.update,
  });

  final String currentVersion;
  final int currentBuildNumber;
  final AppUpdateInfo? update;

  bool get isAvailable => update != null;
}

class AppUpdateService {
  const AppUpdateService();

  Future<AppUpdateResult> checkForUpdate() async {
    final packageInfo = await PackageInfo.fromPlatform();
    final currentBuild = int.tryParse(packageInfo.buildNumber) ?? 0;

    if (AppUpdateConfig.manifestUrl.isEmpty || !Platform.isAndroid) {
      return AppUpdateResult(
        currentVersion: packageInfo.version,
        currentBuildNumber: currentBuild,
      );
    }

    final response = await http.get(Uri.parse(AppUpdateConfig.manifestUrl));
    if (response.statusCode != 200) {
      throw Exception(
        'Không thể kiểm tra phiên bản mới (${response.statusCode}).',
      );
    }

    final update = AppUpdateInfo.fromJson(
      jsonDecode(response.body) as Map<String, dynamic>,
    );
    if (update.apkUrl.isEmpty || update.buildNumber <= currentBuild) {
      return AppUpdateResult(
        currentVersion: packageInfo.version,
        currentBuildNumber: currentBuild,
      );
    }

    return AppUpdateResult(
      currentVersion: packageInfo.version,
      currentBuildNumber: currentBuild,
      update: update,
    );
  }

  Future<OpenResult> downloadAndInstall(
    AppUpdateInfo update, {
    void Function(double progress)? onProgress,
  }) async {
    if (!Platform.isAndroid) {
      throw UnsupportedError('Cập nhật APK chỉ hỗ trợ trên Android.');
    }

    final request = http.Request('GET', Uri.parse(update.apkUrl));
    final response = await http.Client().send(request);
    if (response.statusCode != 200) {
      throw Exception('Không thể tải bản cập nhật (${response.statusCode}).');
    }

    final directory = await getTemporaryDirectory();
    final apkFile = File(
      '${directory.path}/chauthanh-ev-${update.version}+${update.buildNumber}.apk',
    );
    final sink = apkFile.openWrite();
    var received = 0;
    final total = response.contentLength ?? 0;

    await for (final chunk in response.stream) {
      sink.add(chunk);
      received += chunk.length;
      if (total > 0) onProgress?.call(received / total);
    }
    await sink.close();

    return OpenFilex.open(
      apkFile.path,
      type: 'application/vnd.android.package-archive',
    );
  }
}
