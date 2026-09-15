# Phat hanh Android va cap nhat trong ung dung

## Cách đơn giản nhất: chỉ nhập link APK

Sau khi đã push workflow lên GitHub và bật Pages một lần:

1. Mở repository trên GitHub, vào `Actions`.
2. Chọn workflow `Deploy Android APK to GitHub Pages`.
3. Chọn `Run workflow`.
4. Dán link HTTPS trực tiếp tới file `.apk` vào ô `Link HTTPS trực tiếp tới APK`.
5. Nếu để trống, workflow dùng APK vừa build và tự tạo link.
6. Chọn `Run workflow`.

Manifest cập nhật cố định là:

```text
https://phamvantu467.github.io/ChauThanhEV/update.json
```

Link APK mặc định là:

```text
https://phamvantu467.github.io/ChauThanhEV/chauthanh-ev-latest.apk
```

Lưu ý: link APK phải là link tải trực tiếp qua HTTPS, không phải link trang GitHub/Google Drive preview.

## Build APK release

Ban dau can tao Android release ke ky (release signing key). Khong phat hanh APK debug cho nguoi dung.

```powershell
flutter pub get
flutter build apk --release --build-name=1.0.1 --build-number=2
```

File tao ra thuong nam tai:

```text
build/app/outputs/flutter-apk/app-release.apk
```

Doi ten file thanh `chauthanh-ev-1.0.1.apk` va upload len GitHub Release/hosting HTTPS.

## Build voi URL manifest

```powershell
flutter build apk --release `
  --build-name=1.0.1 `
  --build-number=2 `
  --dart-define=APP_UPDATE_MANIFEST_URL=https://YOUR_HOST/update.json
```

## Update manifest

Copy `update.json.example` thanh `update.json`, thay `apkUrl` bang URL download HTTPS that. `buildNumber` phai tang dan. Ung dung chi hien nut cap nhat tren Android va chi tai ban moi khi buildNumber lon hon ban dang cai.

Android van hien hop thoai xac nhan cai dat. Ung dung khong duoc phep tu cai de am tham.

## Phat hanh ban moi

1. Tang `version` va `buildNumber`.
2. Build APK release voi cung `APP_UPDATE_MANIFEST_URL`.
3. Upload APK vao GitHub Release/hosting.
4. Cap nhat `update.json` voi version, buildNumber, apkUrl va releaseNotes.
5. Nguoi dung mo Cai dat > Cap nhat phan mem > Kiem tra cap nhat.

iOS khong ho tro cach tai va cai IPA tu URL ngoai. iOS can TestFlight/App Store (hoac MDM noi bo).
