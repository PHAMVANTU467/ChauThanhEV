class ConnectorModel {
  final int id;
  final String code;
  final String connectorType;
  String status; // Available, Charging, Fault, Maintenance

  ConnectorModel({
    required this.id,
    required this.code,
    required this.connectorType,
    required this.status,
  });

  bool get isAvailable => status == 'Available';
  bool get isCharging => status == 'Charging';
  bool get isFault => status == 'Fault' || status == 'Maintenance';

  factory ConnectorModel.fromJson(Map<String, dynamic> json) {
    return ConnectorModel(
      id: json['id'] ?? json['Id'] ?? 0,
      code: json['code'] ?? json['Code'] ?? '',
      connectorType: json['connectorType'] ?? json['ConnectorType'] ?? 'CCS2',
      status: json['status'] ?? json['Status'] ?? 'Available',
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'code': code,
    'connectorType': connectorType,
    'status': status,
  };
}

class ChargerModel {
  final int id;
  final String code;
  final String name;
  final double powerKw;
  String status; // Online, Offline, Maintenance
  final List<ConnectorModel> connectors;

  ChargerModel({
    required this.id,
    required this.code,
    required this.name,
    required this.powerKw,
    required this.status,
    required this.connectors,
  });

  bool get isOnline => status == 'Online';

  factory ChargerModel.fromJson(Map<String, dynamic> json) {
    var rawList = json['connectors'] ?? json['Connectors'] ?? [];
    List<ConnectorModel> conns = [];
    if (rawList is List) {
      conns = rawList.map((c) => ConnectorModel.fromJson(c as Map<String, dynamic>)).toList();
    }
    return ChargerModel(
      id: json['id'] ?? json['Id'] ?? 0,
      code: json['code'] ?? json['Code'] ?? '',
      name: json['name'] ?? json['Name'] ?? '',
      powerKw: (json['powerKw'] ?? json['PowerKw'] ?? 60.0).toDouble(),
      status: json['status'] ?? json['Status'] ?? 'Online',
      connectors: conns,
    );
  }
}

class StationModel {
  final int id;
  final String code;
  final String name;
  final String address;
  final double distanceKm;
  final double defaultElectricityPrice;
  final bool active;
  final int totalConnectors;
  final int availableConnectors;
  final int chargingConnectors;
  final List<ChargerModel> chargers;
  final List<String> amenities;

  StationModel({
    required this.id,
    required this.code,
    required this.name,
    required this.address,
    this.distanceKm = 1.5,
    this.defaultElectricityPrice = 3800.0,
    this.active = true,
    this.totalConnectors = 16,
    this.availableConnectors = 12,
    this.chargingConnectors = 4,
    required this.chargers,
    this.amenities = const ['Trạm dừng chân', 'Cà phê 24/7', 'WC sạch sẽ', 'Mái che', 'Wifi miễn phí'],
  });

  double get maxPowerKw {
    if (chargers.isEmpty) return 180.0;
    return chargers.map((c) => c.powerKw).reduce((a, b) => a > b ? a : b);
  }

  factory StationModel.fromJson(Map<String, dynamic> json) {
    var rawChargers = json['chargers'] ?? json['Chargers'] ?? [];
    List<ChargerModel> chList = [];
    if (rawChargers is List) {
      chList = rawChargers.map((c) => ChargerModel.fromJson(c as Map<String, dynamic>)).toList();
    }

    int total = json['totalConnectors'] ?? json['TotalConnectors'] ?? 0;
    int avail = json['availableConnectors'] ?? json['AvailableConnectors'] ?? 0;
    int charging = json['chargingConnectors'] ?? json['ChargingConnectors'] ?? 0;

    if (total == 0 && chList.isNotEmpty) {
      total = chList.fold(0, (sum, c) => sum + c.connectors.length);
      avail = chList.fold(0, (sum, c) => sum + c.connectors.where((cn) => cn.isAvailable).length);
      charging = chList.fold(0, (sum, c) => sum + c.connectors.where((cn) => cn.isCharging).length);
    }

    return StationModel(
      id: json['id'] ?? json['Id'] ?? 0,
      code: json['code'] ?? json['Code'] ?? '',
      name: json['name'] ?? json['Name'] ?? '',
      address: json['address'] ?? json['Address'] ?? '',
      distanceKm: (json['distanceKm'] ?? (json['id'] ?? 1) * 2.3).toDouble(),
      defaultElectricityPrice: (json['defaultElectricityPrice'] ?? json['DefaultElectricityPrice'] ?? 3800.0).toDouble(),
      active: json['active'] ?? json['Active'] ?? true,
      totalConnectors: total,
      availableConnectors: avail,
      chargingConnectors: charging,
      chargers: chList,
    );
  }
}

